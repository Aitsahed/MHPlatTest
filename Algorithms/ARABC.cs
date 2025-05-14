
using MathNet.Numerics.Random;
using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using MHPlatTest.Models;
using MHPlatTest.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHPlatTest.Algorithms
{
    /// <summary> 
    ///  A ranking-based adaptive artificial bee colony algorithm for global numerical optimization
    ///       L.Cui, G.Li, X.Wang, Q.Lin, J.Chen, N.Lu, et al.
    /// Information Sciences 2017 Vol. 417 Pages 169-185
    ///DOI: https://doi.org/10.1016/j.ins.2017.07.011
    /// https://www.sciencedirect.com/science/article/pii/S0020025516307289
    /// </summary>
    internal class ARABC : IMHAlgorithm
    {


        /// <summary>
        /// Create a new instance of the metaheuristic optimization algorithm
        /// </summary>
        /// <param name="optimizationConfiguration">the list of optimization configuration to be uapplied to the Algo</param>
        public ARABC(List<OptimizationParameter> optimizationConfiguration, string description, int instanceID)
        {
            MakePersonalOptimizationConfigurationListCopy(optimizationConfiguration, description, instanceID);

        }

        public ARABC()
        {
            ////Make the thread sleep for 10 ms to make sure that the random generator
            ////has different seed when the time between creating two instances is small 
            //Thread.Sleep(10);

            //Generate unique identifier for current instance
            Random random = new Random();
            InstanceID = random.Next();
        }

        /// <summary>
        /// The name of the current metaheuristic optimization algorithm
        /// </summary>
        public string Name { get; set; } = "ARABC";



        /// <summary>
        /// description of the current optimization algorithm
        /// when no decription is available, returns the 'InstanceID'
        /// </summary>
        private string _Description = "";

        public string Description
        {
            get
            {
                if (_Description == "")
                {
                    return InstanceID.ToString();
                }
                return _Description;
            }
            set { _Description = value; }
        }


        /// <summary>
        /// Contains an unique idenifier used to differentiate 
        /// between created instances of the same type
        /// </summary>
        public int InstanceID { get; set; }

        /// <summary>
        /// Contains the configuration to be applied with current optimization algorithm
        /// </summary>
        public List<OptimizationParameter> OptimizationConfiguration { get; set; } = new List<OptimizationParameter>();


        //Lock object used to thread safe the optimization result array
        object AccessSharedObjectLock = new object();


        /// <summary>
        /// Start the ABC algorithm with the specified configuration and returns the optimal value found
        /// </summary>
        /// <param name="threadSafeMethodArgs"></param>
        /// 
        public void ComputeOptimum(IBenchmark benchmarkFunction, List<OptimizationResultModel> resultsData, int randomSeed, double[,]? initialSolutions)
        {
            //Check to see whether the list of optimization configuration has been loaded
            //throw an exception if the list is empty
            if (OptimizationConfiguration.Count == 0)
            {
                throw new ArgumentNullException("optimizationConfiguration", "Please provide the optimization configuration parameters when selecting the metaheuristic optimization algorithm");
            }

            #region Making the method thread safe by making local variables for shared global objects
            //Making a distinct personal copy of the parameter list in this method to
            //make all variables local to this method in order to make it thread-safe
            List<OptimizationParameter> localOptimizationConfiguration = new();
            OptimizationParameter parameter;
            object? createdInstance;

            lock (AccessSharedObjectLock)
            {
                foreach (var ParameterItem in OptimizationConfiguration)
                {
                    parameter = new OptimizationParameter(ParameterItem.Name, ParameterItem.Value, ParameterItem.IsEssentialInfo);
                    localOptimizationConfiguration.Add(parameter);
                }

            }
            #endregion


            var watch = new System.Diagnostics.Stopwatch();
            watch.Restart();

            #region Reading and parsing algorithm parameters

            //Population Size
            int populationSize;
            try
            {
                populationSize = (int)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.PopulationSize).First().Value;
            }
            catch (Exception)
            {
                throw new Exception("Please select the population size !");
            }


            //Stopping Criteria
            StoppingCriteriaType stoppingCriteria;
            int maxItertaionNumber;
            int maxFunctionEvaluationNumber;
            double FunctionValueMinimumEnhancementThreshold;

            try
            {
                stoppingCriteria = (StoppingCriteriaType)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.StoppingCriteriaType).First().Value;
            }
            catch (Exception)
            {
                throw new Exception("Please select the stopping criteria type !");
            }

            switch (stoppingCriteria)
            {
                case StoppingCriteriaType.MaximalNumberOfIteration:
                    maxFunctionEvaluationNumber = int.MaxValue;
                    maxItertaionNumber = (int)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.MaxItertaionNumber).First().Value;
                    FunctionValueMinimumEnhancementThreshold = 0;
                    break;
                case StoppingCriteriaType.MaximalNumberOfFunctionEvaluation:
                    maxFunctionEvaluationNumber = (int)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.MaxFunctionEvaluationNumber).First().Value;
                    maxItertaionNumber = int.MaxValue;// (int)Math.Ceiling((double)maxFunctionEvaluationNumber / (double)populationSize);
                    FunctionValueMinimumEnhancementThreshold = 0;
                    break;
                case StoppingCriteriaType.FunctionValueTolerance:
                    maxItertaionNumber = int.MaxValue;//
                    maxFunctionEvaluationNumber = int.MaxValue;
                    FunctionValueMinimumEnhancementThreshold = (double)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.FunctionValueMinimumEnhancementThreshold).First().Value;
                    break;
                default:
                    throw new Exception("Please select the stopping criteria type !");
            }


            //Problem Dimension
            int ProblemDimension;
            try
            {
                ProblemDimension = (int)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ProblemDimension).First().Value;
            }
            catch (Exception)
            {
                throw new Exception("Please select the problem dimension !");
            }
            //Limit the dimension if its outside the benchmar function dimension limit
            if (benchmarkFunction.MaxProblemDimension < ProblemDimension)
            {
                ProblemDimension = benchmarkFunction.MaxProblemDimension;
            }

            if (benchmarkFunction.MinProblemDimension > ProblemDimension)
            {
                ProblemDimension = benchmarkFunction.MinProblemDimension;
            }


            //Optimization Type (Maximization / minimisation)
            OptimizationProblemType optimizationType;

            try
            {
                optimizationType = (OptimizationProblemType)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.OptimizationType).First().Value;
            }
            catch (Exception)
            {
                throw new Exception("Please select the optimization type (maximization/minimization) !");
            }


            // Population Initialization Scheme
            PopulationInitilizationType populationInitilizationScheme;

            try
            {
                populationInitilizationScheme = (PopulationInitilizationType)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.PopulationInitilization).First().Value;
            }
            catch (Exception)
            {
                throw new Exception("Please select the Population Initialization Scheme !");
            }


            //ABC limit value
            int limitValue;
            try
            {
                limitValue = (int)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value;
            }
            catch (Exception)
            {
                throw new Exception("Please select the limit parameter value !");
            }




            //FunctionValueSigmaTolerance Parameter value
            double FunctionValueSigmaTolerance = double.MinValue;

            try
            {
                FunctionValueSigmaTolerance = (double)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.FunctionValueSigmaTolerance).First().Value;
            }
            catch (Exception)
            {
            }


            //ShiftObjectiveFunctionOptimumValueToZero Parameter value
            bool ShiftObjectiveFunctionOptimumValueToZero = false;

            try
            {
                ShiftObjectiveFunctionOptimumValueToZero = (bool)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ShiftObjectiveFunctionOptimumValueToZero).First().Value;
            }
            catch (Exception)
            {
            }


            //StopOptimizationWhenOptimumIsReached Parameter value
            bool StopOptimizationWhenOptimumIsReached = false;

            try
            {
                StopOptimizationWhenOptimumIsReached = (bool)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.StopOptimizationWhenOptimumIsReached).First().Value;
            }
            catch (Exception)
            {
            }



            #endregion



            #region Declataion of variables, objects and array
            int numerOfScoutBeesGenerated = 0;
            //ARABC variables
            double ARABC_alphaMin = 0;
            double ARABC_alphaMax = 5;
            double ARABC_alpha = (ARABC_alphaMax + ARABC_alphaMin) / 2.0;
            double ARABC_delta = 0.01;
            int[] ARABC_Counter = new int[populationSize];
            double ARABC_c = 0;
            double ARABC_sc = 0;
            double ARABC_PreviousSuccessRate = 0.5;


            int nbrEmployed = (int)Math.Floor(populationSize / 2.0d);
            int nbrOnlooker = (int)Math.Ceiling(populationSize / 2.0d);
            double[] searchSpaceMinValue, searchSpaceMaxValue, searchSpaceRangeValue;
            double[,] populationArray = new double[nbrEmployed, ProblemDimension];
            double[] valueObjectiveFunctionArray = new double[nbrEmployed];
            double[] fitnessArray = new double[nbrEmployed];
            int[] fitnessIndexArray = new int[nbrEmployed];
            List<double> bestObjectiveFunctionEvaluationData = new List<double>();
            int intervalObjFunEval = 10000;
            List<int> ListFirstFoodSourceID = new();


            int[] limitValueForFoodSourcesArray = new int[nbrEmployed];
            double globalBestFitness;
            double globalBestValueObjectiveFunction;
            double[] globalBestPosition = new double[ProblemDimension];

            int currentNumberofunctionEvaluation = 0;

            //Variables used to store data about total/Successfull mutation count
            int TotalMutationCount = 0;
            int SuccessfullMutationCount = 0;
            List<int> TotalMutationCountList = new List<int>();
            List<int> SuccessfullMutationCountList = new List<int>();



            //Computing the current optimal value (depend on 'ShiftObjectiveFunctionOptimumValueToZero' )
            double currentObjectiveFunctionOptimum;
            if (ShiftObjectiveFunctionOptimumValueToZero == true)
            {
                currentObjectiveFunctionOptimum = 0;
            }
            else
            {
                currentObjectiveFunctionOptimum = benchmarkFunction.OptimalFunctionValue(ProblemDimension);
            }
            #endregion



            #region Algorithm Initialization

            //If the same search space limit is fr all dimensions
            //extend this limit for the other dimension starting
            //from the first if only the first dimension limit is set
            if (benchmarkFunction.SearchSpaceMinValue.Length < ProblemDimension)
            {
                double[] currentSearchSpaceMinValue = benchmarkFunction.SearchSpaceMinValue;
                benchmarkFunction.SearchSpaceMinValue = new double[ProblemDimension];

                for (int i = 0; i < ProblemDimension; i++)
                {
                    if (i < currentSearchSpaceMinValue.Length)
                    {
                        benchmarkFunction.SearchSpaceMinValue[i] = currentSearchSpaceMinValue[i];
                    }
                    else
                    {
                        benchmarkFunction.SearchSpaceMinValue[i] = benchmarkFunction.SearchSpaceMinValue[i - 1];
                    }
                }
            }
            if (benchmarkFunction.SearchSpaceMaxValue.Length < ProblemDimension)
            {
                double[] currentSearchSpaceMaxValue = benchmarkFunction.SearchSpaceMaxValue;
                benchmarkFunction.SearchSpaceMaxValue = new double[ProblemDimension];

                for (int i = 0; i < ProblemDimension; i++)
                {
                    if (i < currentSearchSpaceMaxValue.Length)
                    {
                        benchmarkFunction.SearchSpaceMaxValue[i] = currentSearchSpaceMaxValue[i];
                    }
                    else
                    {
                        benchmarkFunction.SearchSpaceMaxValue[i] = benchmarkFunction.SearchSpaceMaxValue[i - 1];
                    }
                }
            }

            var rand = new Random(randomSeed);
            searchSpaceMinValue = benchmarkFunction.SearchSpaceMinValue;
            searchSpaceMaxValue = benchmarkFunction.SearchSpaceMaxValue;
            searchSpaceRangeValue = new double[searchSpaceMaxValue.Length];

            for (int i = 0; i < searchSpaceMaxValue.Length; i++)
            {
                searchSpaceRangeValue[i] = searchSpaceMaxValue[i] - searchSpaceMinValue[i];
            }

            globalBestFitness = double.MinValue;
            switch (optimizationType)
            {
                case OptimizationProblemType.Maximization:
                    globalBestValueObjectiveFunction = double.MinValue;
                    break;
                case OptimizationProblemType.Minimization:
                    globalBestValueObjectiveFunction = double.MaxValue;
                    break;
                default:
                    globalBestValueObjectiveFunction = double.MaxValue;
                    break;
            }
            #endregion



            #region Population initialzation
            switch (populationInitilizationScheme)
            {
                case PopulationInitilizationType.Random:
                    for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                    {
                        for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                        {
                            populationArray[arrayLine, arrayColumn] = searchSpaceMinValue[arrayColumn] + ((double)rand.NextDouble() * searchSpaceRangeValue[arrayColumn]);
                        }
                        limitValueForFoodSourcesArray[arrayLine] = 0;
                    }
                    break;
                default:
                    for (int arrayLine = 0; arrayLine < populationSize; arrayLine++)
                    {
                        for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                        {
                            populationArray[arrayLine, arrayColumn] = searchSpaceMinValue[arrayColumn] + ((double)rand.NextDouble() * searchSpaceRangeValue[arrayColumn]);
                        }
                        limitValueForFoodSourcesArray[arrayLine] = 0;
                    }
                    break;
            }


            if (initialSolutions != null)
            {
                //Introducing inital solutions if they exist
                for (int arrayLine = 0; arrayLine < (initialSolutions.Length / ProblemDimension); arrayLine++)
                {
                    for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                    {
                        populationArray[arrayLine, arrayColumn] = initialSolutions[arrayLine, arrayColumn];
                    }
                }
            }

            #endregion




            #region Computing the cost of the initial employed food sources
            for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
            {
                double[] currentParticlePositionArray = new double[ProblemDimension];

                //Obtaining the position of current particle
                for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                {
                    currentParticlePositionArray[arrayColumn] = populationArray[arrayLine, arrayColumn];
                }

                //Computing the cost of the current particle
                valueObjectiveFunctionArray[arrayLine] = benchmarkFunction.ComputeValue(currentParticlePositionArray, ref currentNumberofunctionEvaluation, ShiftObjectiveFunctionOptimumValueToZero);

                //Checking if the FunctionValueSigmaTolerance has been reached
                if (FunctionValueSigmaTolerance > Math.Abs(valueObjectiveFunctionArray[arrayLine]))
                {
                    valueObjectiveFunctionArray[arrayLine] = 0;
                }

                //collect current best function eval list for post data analysis 
                if (currentNumberofunctionEvaluation % intervalObjFunEval == 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);


                //Computing the fitness value
                fitnessArray[arrayLine] = valueObjectiveFunctionArray[arrayLine].ComputeFitness(optimizationType);


                //Updating the GlobalBest Position
                if (globalBestFitness < fitnessArray[arrayLine])
                {
                    for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                    {
                        globalBestPosition[arrayColumn] = populationArray[arrayLine, arrayColumn];
                    }
                    globalBestValueObjectiveFunction = valueObjectiveFunctionArray[arrayLine];
                    globalBestFitness = fitnessArray[arrayLine];

                    //Check whether the optimization process should be
                    //stopped if the optimal value has been reached
                    if (StopOptimizationWhenOptimumIsReached == true && (Math.Abs(currentObjectiveFunctionOptimum - globalBestValueObjectiveFunction) < FunctionValueSigmaTolerance || Math.Abs(currentObjectiveFunctionOptimum - globalBestValueObjectiveFunction) < double.MinValue))
                    {
                        TotalMutationCountList.Add(0);
                        SuccessfullMutationCountList.Add(0);

                        PrepareResultData(resultsData, MHOptimizationResult.OptimalFunctionValue, globalBestValueObjectiveFunction);
                        PrepareResultData(resultsData, MHOptimizationResult.NumberOfFunctionEvaluation, currentNumberofunctionEvaluation);
                        PrepareResultData(resultsData, MHOptimizationResult.NumberOfTotalIteration, 0);
                        PrepareResultData(resultsData, MHOptimizationResult.OptimalPoint, globalBestPosition);
                        PrepareResultData(resultsData, MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds);
                        PrepareResultData(resultsData, MHOptimizationResult.OptimumFound, true);
                        PrepareResultData(resultsData, MHOptimizationResult.TotalMutationCountData, 0);
                        PrepareResultData(resultsData, MHOptimizationResult.TotalSuccessfullMutationCountData, 0);
                        PrepareResultData(resultsData, MHOptimizationResult.ObjectiveFunctionEvaluationData, bestObjectiveFunctionEvaluationData);
                        PrepareResultData(resultsData, MHOptimizationResult.ScoutBeesGeneratedCount, numerOfScoutBeesGenerated);

                        return;
                    }
                }

                //Check to see whether we have depleted the number of allowed function evaluation
                if (currentNumberofunctionEvaluation > maxFunctionEvaluationNumber)
                {
                    TotalMutationCountList.Add(0);
                    SuccessfullMutationCountList.Add(0);

                    PrepareResultData(resultsData, MHOptimizationResult.OptimalFunctionValue, globalBestValueObjectiveFunction);
                    PrepareResultData(resultsData, MHOptimizationResult.NumberOfFunctionEvaluation, currentNumberofunctionEvaluation);
                    PrepareResultData(resultsData, MHOptimizationResult.NumberOfTotalIteration, 0);
                    PrepareResultData(resultsData, MHOptimizationResult.OptimalPoint, globalBestPosition);
                    PrepareResultData(resultsData, MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds);
                    PrepareResultData(resultsData, MHOptimizationResult.OptimumFound, false);
                    PrepareResultData(resultsData, MHOptimizationResult.TotalMutationCountData, 0);
                    PrepareResultData(resultsData, MHOptimizationResult.TotalSuccessfullMutationCountData, 0);
                    PrepareResultData(resultsData, MHOptimizationResult.ObjectiveFunctionEvaluationData, bestObjectiveFunctionEvaluationData);
                    PrepareResultData(resultsData, MHOptimizationResult.ScoutBeesGeneratedCount, numerOfScoutBeesGenerated);

                    return;
                }
            }
            #endregion


            #region Iterative process
            int iterationNumber = 0;

            while (iterationNumber < maxItertaionNumber)
            {
                iterationNumber++;

                #region Employed bee phase
                //Ranking solution from best to worst
                //1- Assign an indice to each solution
                for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                {
                    fitnessIndexArray[arrayLine] = arrayLine;
                }

                //2-Order the the employed food source based on their fitnesses
                Array.Sort(fitnessArray, fitnessIndexArray, new myReverserClass());

                //2.0-Reorder the arrays of the population and fitness
                double[,] tempPopulationArray = new double[nbrEmployed, ProblemDimension];
                //double[] tempFitnessArray = new double[nbrEmployed];
                int i = 0;

                foreach (var indexOrdered in fitnessIndexArray)
                {
                    // tempFitnessArray[i] = fitnessArray[indexOrdered];

                    for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                    {
                        tempPopulationArray[i, arrayColumn] = populationArray[indexOrdered, arrayColumn];
                    }

                    i++;
                }

                populationArray = tempPopulationArray;

                //3- Compute the rank array
                double[] rankFoodSourceArray = new double[nbrEmployed];

                for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                {
                    rankFoodSourceArray[arrayLine] = ((double)nbrEmployed - (arrayLine + 1.0)) + 1.0;
                }

                //4- Computing the probability of each food source o be selected
                double[] probabilityArray = new double[nbrEmployed];

                for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                {
                    probabilityArray[arrayLine] = Math.Pow(rankFoodSourceArray[arrayLine] / (double)nbrEmployed, ARABC_alpha);
                }

                //Start itertaing through employed bees
                for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                {
                    double randomValue = rand.NextDouble();

                    if (randomValue > probabilityArray[arrayLine])
                    {
                        continue;
                    }

                    //Selecting r1,r2 and r3 using algorithm2 of the paper
                    int r1 = 0;
                    int r2 = 0;
                    int r3 = 0;
                    double tempRandomValue;

                    ListFirstFoodSourceID.Clear();
                    for (int i1 = 0; i1 < nbrEmployed; i1++)
                    {
                        if (i1 != arrayLine)
                        {
                            ListFirstFoodSourceID.Add(i1);
                        }
                    }


                    int foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                    r1 = ListFirstFoodSourceID[foosSourceIndex];


                    //r1 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { arrayLine }, rand);
                    tempRandomValue = rand.NextDouble();
                    while (tempRandomValue > probabilityArray[r1])
                    {
                        tempRandomValue = rand.NextDouble();
                        //r1 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { arrayLine }, rand);
                        foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                        r1 = ListFirstFoodSourceID[foosSourceIndex];
                    }

                    //r2
                    ListFirstFoodSourceID.Remove(r1);

                    foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                    r2 = ListFirstFoodSourceID[foosSourceIndex];
                    //r2 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { arrayLine, r1 }, rand);


                    tempRandomValue = rand.NextDouble();
                    while (tempRandomValue > probabilityArray[r2])
                    {
                        tempRandomValue = rand.NextDouble();
                        //r2 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { arrayLine, r1 }, rand);
                        foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                        r2 = ListFirstFoodSourceID[foosSourceIndex];
                    }


                    //r3
                    ListFirstFoodSourceID.Remove(r2);

                    foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                    r3 = ListFirstFoodSourceID[foosSourceIndex];
                    //r3 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { arrayLine, r1, r2 }, rand);


                    tempRandomValue = rand.NextDouble();
                    while (tempRandomValue > probabilityArray[r3])
                    {
                        tempRandomValue = rand.NextDouble();
                        //r3 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { arrayLine, r1, r2 }, rand);
                        foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                        r3 = ListFirstFoodSourceID[foosSourceIndex];
                    }


                    //Generating new solution using eq.5
                    //1- Obtaining the position of current particle
                    double[] currentParticlePositionArray = new double[ProblemDimension];

                    for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                    {
                        currentParticlePositionArray[arrayColumn] = populationArray[arrayLine, arrayColumn];
                    }

                    //2- Choosing a random dimension to update
                    int dimensionToUpdate = (int)Math.Floor(rand.NextDouble() * ProblemDimension);

                    //3- Apply new update equation to compute new solution
                    double phiValue = (rand.NextDouble() * 2) - 1;
                    currentParticlePositionArray[dimensionToUpdate] = populationArray[r1, dimensionToUpdate]
                                                                    + phiValue * (populationArray[r2, dimensionToUpdate] - populationArray[r3, dimensionToUpdate]);


                    //check if newly food sources within the serach space boundary
                    if (currentParticlePositionArray[dimensionToUpdate] > benchmarkFunction.SearchSpaceMaxValue[dimensionToUpdate])
                    {
                        currentParticlePositionArray[dimensionToUpdate] = benchmarkFunction.SearchSpaceMaxValue[dimensionToUpdate];
                    }
                    if (currentParticlePositionArray[dimensionToUpdate] < benchmarkFunction.SearchSpaceMinValue[dimensionToUpdate])
                    {
                        currentParticlePositionArray[dimensionToUpdate] = benchmarkFunction.SearchSpaceMinValue[dimensionToUpdate];
                    }



                    //Computing the cost of the current particle
                    double currentParticleValueObjectiveFunction = benchmarkFunction.ComputeValue(currentParticlePositionArray, ref currentNumberofunctionEvaluation, ShiftObjectiveFunctionOptimumValueToZero);

                    //Checking if the FunctionValueSigmaTolerance has been reached
                    if (FunctionValueSigmaTolerance > Math.Abs(currentParticleValueObjectiveFunction))
                    {
                        currentParticleValueObjectiveFunction = 0;
                    }

                    //Computing the fitness
                    double currentParticleFitness = currentParticleValueObjectiveFunction.ComputeFitness(optimizationType);


                    TotalMutationCount++;
                    ARABC_c++;

                    //Apply the greedy selection on newly generated food source
                    if (currentParticleFitness > fitnessArray[arrayLine])
                    {
                        //Newly generated food source is better
                        populationArray[arrayLine, dimensionToUpdate] = currentParticlePositionArray[dimensionToUpdate];
                        valueObjectiveFunctionArray[arrayLine] = currentParticleValueObjectiveFunction;
                        fitnessArray[arrayLine] = currentParticleFitness;

                        limitValueForFoodSourcesArray[arrayLine] = 0;
                        SuccessfullMutationCount++;
                        ARABC_sc++;

                        //Updating the GlobalBest Position
                        if (globalBestFitness < currentParticleFitness)
                        {
                            for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                            {
                                globalBestPosition[arrayColumn] = populationArray[arrayLine, arrayColumn];
                            }
                            globalBestFitness = currentParticleFitness;
                            globalBestValueObjectiveFunction = currentParticleValueObjectiveFunction;

                            //Check whether the optimization process should be
                            //stopped if the optimal value has been reached
                            if (StopOptimizationWhenOptimumIsReached == true && (Math.Abs(currentObjectiveFunctionOptimum - globalBestValueObjectiveFunction) < FunctionValueSigmaTolerance || Math.Abs(currentObjectiveFunctionOptimum - globalBestValueObjectiveFunction) < double.MinValue))
                            {
                                TotalMutationCountList.Add(TotalMutationCount);
                                SuccessfullMutationCountList.Add(SuccessfullMutationCount);

                                //collect current best function eval list for post data analysis 
                                bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                                PrepareResultData(resultsData, MHOptimizationResult.OptimalFunctionValue, globalBestValueObjectiveFunction);
                                PrepareResultData(resultsData, MHOptimizationResult.NumberOfFunctionEvaluation, currentNumberofunctionEvaluation);
                                PrepareResultData(resultsData, MHOptimizationResult.NumberOfTotalIteration, iterationNumber);
                                PrepareResultData(resultsData, MHOptimizationResult.OptimalPoint, globalBestPosition);
                                PrepareResultData(resultsData, MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds);
                                PrepareResultData(resultsData, MHOptimizationResult.OptimumFound, true);
                                PrepareResultData(resultsData, MHOptimizationResult.TotalMutationCountData, new List<int>());
                                PrepareResultData(resultsData, MHOptimizationResult.TotalSuccessfullMutationCountData, new List<int>());
                                PrepareResultData(resultsData, MHOptimizationResult.ObjectiveFunctionEvaluationData, bestObjectiveFunctionEvaluationData);
                                PrepareResultData(resultsData, MHOptimizationResult.ScoutBeesGeneratedCount, numerOfScoutBeesGenerated);

                                return;
                            }
                        }
                    }
                    else
                    {
                        //newly generated food source is not better
                        //increase the limit value counter for current food source
                        limitValueForFoodSourcesArray[arrayLine]++;
                    }

                    //collect current best function eval list for post data analysis 
                    if (currentNumberofunctionEvaluation % intervalObjFunEval == 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);


                    //Check to see whether we have depleted the number of allowed function evaluation
                    if (currentNumberofunctionEvaluation > maxFunctionEvaluationNumber)
                    {
                        TotalMutationCountList.Add(TotalMutationCount);
                        SuccessfullMutationCountList.Add(SuccessfullMutationCount);

                        //collect current best function eval list for post data analysis 
                        if (currentNumberofunctionEvaluation % intervalObjFunEval != 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                        PrepareResultData(resultsData, MHOptimizationResult.OptimalFunctionValue, globalBestValueObjectiveFunction);
                        PrepareResultData(resultsData, MHOptimizationResult.NumberOfFunctionEvaluation, currentNumberofunctionEvaluation);
                        PrepareResultData(resultsData, MHOptimizationResult.NumberOfTotalIteration, iterationNumber);
                        PrepareResultData(resultsData, MHOptimizationResult.OptimalPoint, globalBestPosition);
                        PrepareResultData(resultsData, MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds);
                        PrepareResultData(resultsData, MHOptimizationResult.OptimumFound, false);
                        PrepareResultData(resultsData, MHOptimizationResult.TotalMutationCountData, new List<int>());
                        PrepareResultData(resultsData, MHOptimizationResult.TotalSuccessfullMutationCountData, new List<int>());
                        PrepareResultData(resultsData, MHOptimizationResult.ObjectiveFunctionEvaluationData, bestObjectiveFunctionEvaluationData);
                        PrepareResultData(resultsData, MHOptimizationResult.ScoutBeesGeneratedCount, numerOfScoutBeesGenerated);

                        return;
                    }
                }
                #endregion



                #region Onlooker bee phase


                //Ranking solution from best to worst
                //1- Assign an indice to each solution
                for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                {
                    fitnessIndexArray[arrayLine] = arrayLine;
                }

                //2-Order the the employed food source based on their fitnesses
                Array.Sort(fitnessArray, fitnessIndexArray, new myReverserClass());

                //2.0-Reorder the arrays of the population and fitness
                tempPopulationArray = new double[nbrEmployed, ProblemDimension];
                i = 0;

                foreach (var indexOrdered in fitnessIndexArray)
                {
                    // tempFitnessArray[i] = fitnessArray[indexOrdered];

                    for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                    {
                        tempPopulationArray[i, arrayColumn] = populationArray[indexOrdered, arrayColumn];
                    }

                    i++;
                }

                populationArray = tempPopulationArray;

                //3- Compute the rank array
                rankFoodSourceArray = new double[nbrEmployed];

                for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                {
                    rankFoodSourceArray[arrayLine] = ((double)nbrEmployed - (arrayLine + 1.0)) + 1.0;
                }

                //4- Computing the probability of each food source o be selected
                probabilityArray = new double[nbrEmployed];

                for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                {
                    probabilityArray[arrayLine] = Math.Pow(rankFoodSourceArray[arrayLine] / (double)nbrEmployed, ARABC_alpha);
                }


                //Browse through all onlookers
                for (int arrayLine = 0; arrayLine < nbrOnlooker; arrayLine++)
                {
                    double tempRandomValue;

                    //1- choosing the  food source that the current onlooker will go to
                    ListFirstFoodSourceID.Clear();
                    for (int i1 = 0; i1 < nbrEmployed; i1++)
                    {
                        ListFirstFoodSourceID.Add(i1);
                    }

                    int foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                    int selectedFoodSourceID = ListFirstFoodSourceID[foosSourceIndex];
                    //int selectedFoodSourceID = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { }, rand);

                    tempRandomValue = rand.NextDouble();
                    while (tempRandomValue > probabilityArray[selectedFoodSourceID])
                    {
                        tempRandomValue = rand.NextDouble();
                        //selectedFoodSourceID = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { }, rand);
                        foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                        selectedFoodSourceID = ListFirstFoodSourceID[foosSourceIndex];
                    }

                    //Selecting r1,r2 and r3 using algorithm2 of the paper
                    int r1 = 0;
                    int r2 = 0;
                    int r3 = 0;

                    //r1
                    ListFirstFoodSourceID.Remove(selectedFoodSourceID);

                    foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                    r1 = ListFirstFoodSourceID[foosSourceIndex];
                    //r1 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { selectedFoodSourceID }, rand);

                    tempRandomValue = rand.NextDouble();
                    while (tempRandomValue > probabilityArray[r1])
                    {
                        tempRandomValue = rand.NextDouble();
                        //r1 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { selectedFoodSourceID }, rand);
                        foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                        r1 = ListFirstFoodSourceID[foosSourceIndex];
                    }


                    //r2
                    ListFirstFoodSourceID.Remove(r1);

                    foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                    r2 = ListFirstFoodSourceID[foosSourceIndex];
                    //r2 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { selectedFoodSourceID, r1 }, rand);

                    tempRandomValue = rand.NextDouble();
                    while (tempRandomValue > probabilityArray[r2])
                    {
                        tempRandomValue = rand.NextDouble();
                        //r2 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { selectedFoodSourceID, r1 }, rand);
                        foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                        r2 = ListFirstFoodSourceID[foosSourceIndex];
                    }


                    //r3
                    ListFirstFoodSourceID.Remove(r2);

                    foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                    r3 = ListFirstFoodSourceID[foosSourceIndex];

                    //r3 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { selectedFoodSourceID, r1, r2 }, rand);

                    tempRandomValue = rand.NextDouble();
                    while (tempRandomValue > probabilityArray[r3])
                    {
                        tempRandomValue = rand.NextDouble();
                        //r3 = FindZeroBasedRandomIndex(nbrEmployed - 1, new List<int>() { selectedFoodSourceID, r1, r2 }, rand);
                        foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                        r3 = ListFirstFoodSourceID[foosSourceIndex];
                    }





                    //Generating new solution using eq.5
                    //1- Obtaining the position of the selected food source
                    double[] currentParticlePositionArray = new double[ProblemDimension];

                    for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                    {
                        currentParticlePositionArray[arrayColumn] = populationArray[selectedFoodSourceID, arrayColumn];
                    }

                    //2- Choosing a random dimension to update
                    int dimensionToUpdate = (int)Math.Floor(rand.NextDouble() * ProblemDimension);

                    //3- Apply new update equation to compute new solution
                    double phiValue = (rand.NextDouble() * 2) - 1;
                    currentParticlePositionArray[dimensionToUpdate] = populationArray[r1, dimensionToUpdate]
                                                                    + phiValue * (populationArray[r2, dimensionToUpdate] - populationArray[r3, dimensionToUpdate]);


                    //check if newly food sources within the serach space boundary
                    if (currentParticlePositionArray[dimensionToUpdate] > benchmarkFunction.SearchSpaceMaxValue[dimensionToUpdate])
                    {
                        currentParticlePositionArray[dimensionToUpdate] = benchmarkFunction.SearchSpaceMaxValue[dimensionToUpdate];
                    }
                    if (currentParticlePositionArray[dimensionToUpdate] < benchmarkFunction.SearchSpaceMinValue[dimensionToUpdate])
                    {
                        currentParticlePositionArray[dimensionToUpdate] = benchmarkFunction.SearchSpaceMinValue[dimensionToUpdate];
                    }


                    //Computing the cost of the current particle
                    double currentParticleValueObjectiveFunction = benchmarkFunction.ComputeValue(currentParticlePositionArray, ref currentNumberofunctionEvaluation, ShiftObjectiveFunctionOptimumValueToZero);

                    //Checking if the FunctionValueSigmaTolerance has been reached
                    if (FunctionValueSigmaTolerance > Math.Abs(currentParticleValueObjectiveFunction))
                    {
                        currentParticleValueObjectiveFunction = 0;
                    }

                    //Computing the fitness
                    double currentParticleFitness = currentParticleValueObjectiveFunction.ComputeFitness(optimizationType);

                    TotalMutationCount++;
                    ARABC_c++;

                    //Apply the greedy selection on newly generated food source
                    if (currentParticleFitness > fitnessArray[selectedFoodSourceID])
                    {
                        //Newly generated food source is better
                        populationArray[selectedFoodSourceID, dimensionToUpdate] = currentParticlePositionArray[dimensionToUpdate];
                        valueObjectiveFunctionArray[selectedFoodSourceID] = currentParticleValueObjectiveFunction;
                        fitnessArray[selectedFoodSourceID] = currentParticleFitness;

                        limitValueForFoodSourcesArray[selectedFoodSourceID] = 0;
                        SuccessfullMutationCount++;
                        ARABC_sc++;

                        //Updating the GlobalBest Position
                        if (globalBestFitness < currentParticleFitness)
                        {
                            for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                            {
                                globalBestPosition[arrayColumn] = populationArray[selectedFoodSourceID, arrayColumn];
                            }
                            globalBestFitness = currentParticleFitness;
                            globalBestValueObjectiveFunction = currentParticleValueObjectiveFunction;

                            //Check whether the optimization process should be
                            //stopped if the optimal value has been reached
                            if (StopOptimizationWhenOptimumIsReached == true && (Math.Abs(currentObjectiveFunctionOptimum - globalBestValueObjectiveFunction) < FunctionValueSigmaTolerance || Math.Abs(currentObjectiveFunctionOptimum - globalBestValueObjectiveFunction) < double.MinValue))
                            {
                                TotalMutationCountList.Add(TotalMutationCount);
                                SuccessfullMutationCountList.Add(SuccessfullMutationCount);

                                //collect current best function eval list for post data analysis 
                                bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                                PrepareResultData(resultsData, MHOptimizationResult.OptimalFunctionValue, globalBestValueObjectiveFunction);
                                PrepareResultData(resultsData, MHOptimizationResult.NumberOfFunctionEvaluation, currentNumberofunctionEvaluation);
                                PrepareResultData(resultsData, MHOptimizationResult.NumberOfTotalIteration, iterationNumber);
                                PrepareResultData(resultsData, MHOptimizationResult.OptimalPoint, globalBestPosition);
                                PrepareResultData(resultsData, MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds);
                                PrepareResultData(resultsData, MHOptimizationResult.OptimumFound, true);
                                PrepareResultData(resultsData, MHOptimizationResult.TotalMutationCountData, new List<int>());
                                PrepareResultData(resultsData, MHOptimizationResult.TotalSuccessfullMutationCountData, new List<int>());
                                PrepareResultData(resultsData, MHOptimizationResult.ObjectiveFunctionEvaluationData, bestObjectiveFunctionEvaluationData);
                                PrepareResultData(resultsData, MHOptimizationResult.ScoutBeesGeneratedCount, numerOfScoutBeesGenerated);

                                return;
                            }
                        }
                    }
                    else
                    {
                        //newly generated food source is not better
                        //increase the limit value counter for current food source
                        limitValueForFoodSourcesArray[selectedFoodSourceID]++;
                    }

                    //collect current best function eval list for post data analysis 
                    if (currentNumberofunctionEvaluation % intervalObjFunEval == 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);


                    //Check to see whether we have depleted the number of allowed function evaluation
                    if (currentNumberofunctionEvaluation > maxFunctionEvaluationNumber)
                    {
                        TotalMutationCountList.Add(TotalMutationCount);
                        SuccessfullMutationCountList.Add(SuccessfullMutationCount);

                        //collect current best function eval list for post data analysis 
                        if (currentNumberofunctionEvaluation % intervalObjFunEval != 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                        PrepareResultData(resultsData, MHOptimizationResult.OptimalFunctionValue, globalBestValueObjectiveFunction);
                        PrepareResultData(resultsData, MHOptimizationResult.NumberOfFunctionEvaluation, currentNumberofunctionEvaluation);
                        PrepareResultData(resultsData, MHOptimizationResult.NumberOfTotalIteration, iterationNumber);
                        PrepareResultData(resultsData, MHOptimizationResult.OptimalPoint, globalBestPosition);
                        PrepareResultData(resultsData, MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds);
                        PrepareResultData(resultsData, MHOptimizationResult.OptimumFound, false);
                        PrepareResultData(resultsData, MHOptimizationResult.TotalMutationCountData, new List<int>());
                        PrepareResultData(resultsData, MHOptimizationResult.TotalSuccessfullMutationCountData, new List<int>());
                        PrepareResultData(resultsData, MHOptimizationResult.ObjectiveFunctionEvaluationData, bestObjectiveFunctionEvaluationData);
                        PrepareResultData(resultsData, MHOptimizationResult.ScoutBeesGeneratedCount, numerOfScoutBeesGenerated);

                        return;
                    }
                }
                #endregion


                #region Scout bee phase
                //Locating the food source with maximum limit value
                int maxLimitValueForFoodSources = 0;
                int maxLimitValueForFoodSourcesID = 0;

                for (int foodSourceID = 0; foodSourceID < nbrEmployed; foodSourceID++)
                {
                    if (limitValueForFoodSourcesArray[foodSourceID] > maxLimitValueForFoodSources)
                    {
                        maxLimitValueForFoodSources = limitValueForFoodSourcesArray[foodSourceID];
                        maxLimitValueForFoodSourcesID = foodSourceID;
                    }
                }

                //Check to see if the 'maxLimitValueForFoodSources' has exceeded the 'limit' parameter
                if (maxLimitValueForFoodSources > limitValue)
                {
                    numerOfScoutBeesGenerated++;

                    // Replace the 'maxLimitValueForFoodSourcesID' food source with a ramdomly generated one
                    switch (populationInitilizationScheme)
                    {
                        case PopulationInitilizationType.Random:
                            for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                            {
                                for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                                {
                                    populationArray[maxLimitValueForFoodSourcesID, arrayColumn] = searchSpaceMinValue[arrayColumn] + ((double)rand.NextDouble() * searchSpaceRangeValue[arrayColumn]);
                                    limitValueForFoodSourcesArray[maxLimitValueForFoodSourcesID] = 0;
                                }
                            }
                            break;
                        default:
                            for (int arrayLine = 0; arrayLine < populationSize; arrayLine++)
                            {
                                for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                                {
                                    populationArray[maxLimitValueForFoodSourcesID, arrayColumn] = searchSpaceMinValue[arrayColumn] + ((double)rand.NextDouble() * searchSpaceRangeValue[arrayColumn]);
                                    limitValueForFoodSourcesArray[maxLimitValueForFoodSourcesID] = 0;
                                }
                            }
                            break;
                    }

                    //Computing the cost of the newly generated  food sources
                    double[] currentParticlePositionArray = new double[ProblemDimension];

                    //Obtaining the position of current particle
                    for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                    {
                        currentParticlePositionArray[arrayColumn] = populationArray[maxLimitValueForFoodSourcesID, arrayColumn];
                    }

                    //Computing the cost of the current particle
                    valueObjectiveFunctionArray[maxLimitValueForFoodSourcesID] = benchmarkFunction.ComputeValue(currentParticlePositionArray, ref currentNumberofunctionEvaluation, ShiftObjectiveFunctionOptimumValueToZero);

                    //Checking if the FunctionValueSigmaTolerance has been reached
                    if (FunctionValueSigmaTolerance > Math.Abs(valueObjectiveFunctionArray[maxLimitValueForFoodSourcesID]))
                    {
                        valueObjectiveFunctionArray[maxLimitValueForFoodSourcesID] = 0;
                    }

                    //Computing the fitness
                    fitnessArray[maxLimitValueForFoodSourcesID] = valueObjectiveFunctionArray[maxLimitValueForFoodSourcesID].ComputeFitness(optimizationType);

                    //Updating the GlobalBest Position
                    if (globalBestFitness < fitnessArray[maxLimitValueForFoodSourcesID])
                    {
                        for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                        {
                            globalBestPosition[arrayColumn] = populationArray[maxLimitValueForFoodSourcesID, arrayColumn];
                        }
                        globalBestValueObjectiveFunction = valueObjectiveFunctionArray[maxLimitValueForFoodSourcesID];
                        globalBestFitness = fitnessArray[maxLimitValueForFoodSourcesID];

                        //Check whether the optimization process should be
                        //stopped if the optimal value has been reached
                        if (StopOptimizationWhenOptimumIsReached == true && (Math.Abs(currentObjectiveFunctionOptimum - globalBestValueObjectiveFunction) < FunctionValueSigmaTolerance || Math.Abs(currentObjectiveFunctionOptimum - globalBestValueObjectiveFunction) < double.MinValue))
                        {
                            TotalMutationCountList.Add(TotalMutationCount);
                            SuccessfullMutationCountList.Add(SuccessfullMutationCount);

                            //collect current best function eval list for post data analysis 
                            bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                            PrepareResultData(resultsData, MHOptimizationResult.OptimalFunctionValue, globalBestValueObjectiveFunction);
                            PrepareResultData(resultsData, MHOptimizationResult.NumberOfFunctionEvaluation, currentNumberofunctionEvaluation);
                            PrepareResultData(resultsData, MHOptimizationResult.NumberOfTotalIteration, iterationNumber);
                            PrepareResultData(resultsData, MHOptimizationResult.OptimalPoint, globalBestPosition);
                            PrepareResultData(resultsData, MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds);
                            PrepareResultData(resultsData, MHOptimizationResult.OptimumFound, true);
                            PrepareResultData(resultsData, MHOptimizationResult.TotalMutationCountData, new List<int>());
                            PrepareResultData(resultsData, MHOptimizationResult.TotalSuccessfullMutationCountData, new List<int>());
                            PrepareResultData(resultsData, MHOptimizationResult.ObjectiveFunctionEvaluationData, bestObjectiveFunctionEvaluationData);
                            PrepareResultData(resultsData, MHOptimizationResult.ScoutBeesGeneratedCount, numerOfScoutBeesGenerated);

                            return;
                        }
                    }

                    //collect current best function eval list for post data analysis 
                    if (currentNumberofunctionEvaluation % intervalObjFunEval == 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                    //Check to see whether we have depleted the number of allowed function evaluation
                    if (currentNumberofunctionEvaluation > maxFunctionEvaluationNumber)
                    {
                        TotalMutationCountList.Add(TotalMutationCount);
                        SuccessfullMutationCountList.Add(SuccessfullMutationCount);

                        //collect current best function eval list for post data analysis 
                        if (currentNumberofunctionEvaluation % intervalObjFunEval != 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                        PrepareResultData(resultsData, MHOptimizationResult.OptimalFunctionValue, globalBestValueObjectiveFunction);
                        PrepareResultData(resultsData, MHOptimizationResult.NumberOfFunctionEvaluation, currentNumberofunctionEvaluation);
                        PrepareResultData(resultsData, MHOptimizationResult.NumberOfTotalIteration, iterationNumber);
                        PrepareResultData(resultsData, MHOptimizationResult.OptimalPoint, globalBestPosition);
                        PrepareResultData(resultsData, MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds);
                        PrepareResultData(resultsData, MHOptimizationResult.OptimumFound, false);
                        PrepareResultData(resultsData, MHOptimizationResult.TotalMutationCountData, new List<int>());
                        PrepareResultData(resultsData, MHOptimizationResult.TotalSuccessfullMutationCountData, new List<int>());
                        PrepareResultData(resultsData, MHOptimizationResult.ObjectiveFunctionEvaluationData, bestObjectiveFunctionEvaluationData);
                        PrepareResultData(resultsData, MHOptimizationResult.ScoutBeesGeneratedCount, numerOfScoutBeesGenerated);

                        return;
                    }
                }

                #endregion




                #region Udating the ARABC parameters
                double ARABC_CurrentSuccessRate = ARABC_sc / ARABC_c;
                double ARABC_r = 0.8 + 0.1 * rand.NextDouble();

                ARABC_sc = 0;
                ARABC_c = 0;

                //Updating the alpha parameter
                if (ARABC_CurrentSuccessRate > ARABC_r * ARABC_PreviousSuccessRate)
                {
                    ARABC_alpha = Math.Min(ARABC_alpha + ARABC_delta * ARABC_CurrentSuccessRate, ARABC_alphaMax);
                }
                else
                {
                    ARABC_alpha = Math.Max(ARABC_alpha - ARABC_delta * (1 - ARABC_CurrentSuccessRate), ARABC_alphaMin);
                }

                if (ARABC_alpha > ARABC_alphaMax) ARABC_alpha = ARABC_alphaMax;
                if (ARABC_alpha < ARABC_alphaMin) ARABC_alpha = ARABC_alphaMin;


                ARABC_PreviousSuccessRate = ARABC_CurrentSuccessRate;
                #endregion



                TotalMutationCountList.Add(TotalMutationCount);
                SuccessfullMutationCountList.Add(SuccessfullMutationCount);
            }
            #endregion

            TotalMutationCountList.Add(TotalMutationCount);
            SuccessfullMutationCountList.Add(SuccessfullMutationCount);

            PrepareResultData(resultsData, MHOptimizationResult.OptimalFunctionValue, globalBestValueObjectiveFunction);
            PrepareResultData(resultsData, MHOptimizationResult.NumberOfFunctionEvaluation, currentNumberofunctionEvaluation);
            PrepareResultData(resultsData, MHOptimizationResult.NumberOfTotalIteration, maxItertaionNumber - 1);
            PrepareResultData(resultsData, MHOptimizationResult.OptimalPoint, globalBestPosition);
            PrepareResultData(resultsData, MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds);
            PrepareResultData(resultsData, MHOptimizationResult.OptimumFound, false);
            PrepareResultData(resultsData, MHOptimizationResult.TotalMutationCountData, new List<int>());
            PrepareResultData(resultsData, MHOptimizationResult.TotalSuccessfullMutationCountData, new List<int>());
            PrepareResultData(resultsData, MHOptimizationResult.ObjectiveFunctionEvaluationData, bestObjectiveFunctionEvaluationData);
            PrepareResultData(resultsData, MHOptimizationResult.ScoutBeesGeneratedCount, numerOfScoutBeesGenerated);

            return;
        }




        /// <summary>
        /// Call this method to prepare the data at the end of an optimization process
        /// </summary>
        public void PrepareResultData(List<OptimizationResultModel> data, MHOptimizationResult dataName, object dataValue)
        {
            if (data.Exists(x => x.Name == dataName) == true)
            {
                data.First(x => x.Name == dataName).Value = dataValue;
            }
            else
            {
                data.Add(new OptimizationResultModel(dataName, dataValue));
            }
        }



        /// <summary>
        /// Call this method to make hard personal copy of reference type 'optimizationConfiguration'
        /// </summary>
        /// <param name="optimizationConfiguration">the list of optimization parameters to apply to the current instance of metaheuristic optimization algorithm</param>
        /// <param name="description">the description of the current instance of metaheuristic optimization algorithm</param>
        /// <param name="randomIntValue"></param>
        public void MakePersonalOptimizationConfigurationListCopy(List<OptimizationParameter> optimizationConfiguration, string description, int randomIntValue)
        {
            //Making a distinct personal copy of the parameter list
            OptimizationConfiguration = new();
            OptimizationParameter parameter;

            foreach (var ParameterItem in optimizationConfiguration)
            {
                parameter = new OptimizationParameter(ParameterItem.Name, ParameterItem.Value, ParameterItem.IsEssentialInfo);
                OptimizationConfiguration.Add(parameter);
            }

            InstanceID = randomIntValue;
            Description = description;
        }


        /// <summary>
        /// Call this method to generate a random index from '0' to 'maxValue' while ignoring the index selected in 'indexToIgnore'
        /// </summary>
        /// <param name="maxValue">the maximal value the index could take</param>
        /// <param name="indexToIgnore">the list of indexes to ignore while computing the random one. the List could have a null value </param>
        /// <returns>a random zero based index</returns>
        private int FindZeroBasedRandomIndex(int maxValue, List<int> indexesToIgnoreList, Random rand)
        {
            List<int> indexList = new();

            for (int i = 0; i <= maxValue; i++)
            {
                if (indexesToIgnoreList != null && indexesToIgnoreList.Contains(i) == false)
                {
                    indexList.Add(i);
                }
            }

            int randomIndex = (int)Math.Floor(rand.NextDouble() * indexList.Count);

            return indexList[randomIndex];
        }


        private static void Sort<T>(T[][] data, int col)
        {
            Comparer<T> comparer = Comparer<T>.Default;
            Array.Sort<T[]>(data, (x, y) => comparer.Compare(x[col], y[col]));
        }

    }

    public class myReverserClass : IComparer
    {

        // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
        int IComparer.Compare(Object x, Object y)
        {
            double valX = (double)x;
            double valY = (double)y;

            if (valY > valX)
            {
                return 1;
            }
            else if (valX > valY)
            {
                return -1;
            }
            else
            {
                return 0;
            }


        }
    }


}
