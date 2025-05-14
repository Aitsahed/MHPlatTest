
using MathNet.Numerics.Financial;
using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using MHPlatTest.Models;
using MHPlatTest.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHPlatTest.Algorithms
{
    /// <summary> 
    /// Adaptve update equation divised into three parts. Each part could have its weight adapted
    /// Vh=Coff1*Xh+Coff2*PhiPar(Xh-Xk)+Coff3*KciPar(Xbest-Xh)
    /// KciPar is cmputed  KciBasedOnPhi: KciPar=1-Abs(PhiPar)
    ///Adaptive number of optimization parameters to be updated
    ///* Reference Papers*/
    ///The algorithm used with the AdaABC journl paper article
    ///* Parameters values*/
    ///*To be determined*/
    ///In V1 the food source used to compute distance is being changed for each updated dimension. Before the same food source was used to compute distance for all dimension
    /// </summary>
    class AdaABC : IMHAlgorithm
    {

        /// <summary>
        /// Create a new instance of the metaheuristic optimization algorithm
        /// </summary>
        /// <param name="optimizationConfiguration">the list of optimization configuration to be uapplied to the Algo</param>
        public AdaABC(List<OptimizationParameter> optimizationConfiguration, string description, int instanceID)
        {
            MakePersonalOptimizationConfigurationListCopy(optimizationConfiguration, description, instanceID);
        }

        public AdaABC()
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
        public string Name { get; set; } = "AdaABC";



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
                    maxItertaionNumber = (int)Math.Ceiling((double)maxFunctionEvaluationNumber / (double)populationSize);
                    FunctionValueMinimumEnhancementThreshold = 0;
                    break;
                case StoppingCriteriaType.FunctionValueTolerance:
                    maxItertaionNumber = 1000000;
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



            // Scout generationScheme
            ScoutGenerationType scoutGenerationScheme;

            try
            {
                scoutGenerationScheme = (ScoutGenerationType)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ScoutGeneration).First().Value;
            }
            catch (Exception ex)
            {
                throw new Exception("Please select the scout generation Scheme !");
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



            //AEEABC_TuneNumberOfDimensionUsingGBest Parameter value
            bool AEEABC_TuneNumberOfDimensionUsingGBest = false;

            try
            {
                AEEABC_TuneNumberOfDimensionUsingGBest = (bool)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneNumberOfDimensionUsingGBest).First().Value;
            }
            catch (Exception)
            {
            }


            //AEEABC_NumberOfIterationsToTuneParameters Parameter value
            int AEEABC_NumberOfIterationsToTuneParameters = 0;

            try
            {
                AEEABC_NumberOfIterationsToTuneParameters = (int)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.AEEABC_NumberOfIterationsToTuneParameters).First().Value;
            }
            catch (Exception)
            {
                throw new Exception("Please specify the Number Of Iterations required To Tune Parameters values !");
            }



            //ABC_ProbabilityEquationType Parameter value
            ABC_ProbabilityEquationType ABC_ProbabilityEquationType = ABC_ProbabilityEquationType.Original;

            try
            {
                ABC_ProbabilityEquationType = (ABC_ProbabilityEquationType)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ABC_ProbabilityEquationType).First().Value;
            }
            catch (Exception)
            {
                ABC_ProbabilityEquationType = ABC_ProbabilityEquationType.Original;
            }



            //AEEABC_TuneScoutGenerationTypeParameters Parameter value
            bool AEEABC_TuneScoutGenerationTypeParameters = false;

            try
            {
                AEEABC_TuneScoutGenerationTypeParameters = (bool)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneScoutGenerationType).First().Value;
            }
            catch (Exception)
            {
            }




            //AEEABC_TuneProbabilityEquationTypeParameter Parameter value
            bool AEEABC_TuneProbabilityEquationTypeParameter = false;

            try
            {
                AEEABC_TuneProbabilityEquationTypeParameter = (bool)localOptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneProbabilityEquationType).First().Value;
            }
            catch (Exception)
            {
            }



            #endregion




            #region Declataion of variables, objects and array
            int numerOfScoutBeesGenerated = 0;
            ExplorationVsExploitationType currentExplorationVsExploitationStatus = ExplorationVsExploitationType.DriveTowardExploration;
            int varIterNumberAfterLastTuningCycle = 1;
            int nbrEmployed = (int)Math.Floor(populationSize / 2.0d);
            int nbrOnlooker = (int)Math.Ceiling(populationSize / 2.0d);
            double[] searchSpaceMinValue, searchSpaceMaxValue, searchSpaceRangeValue;
            double[,] populationArray = new double[populationSize, ProblemDimension];
            double[] valueObjectiveFunctionArray = new double[populationSize];
            double[] fitnessArray = new double[populationSize];
            double[] wheightingProbabilityForScoutGenerationArray = new double[nbrEmployed];
            int[] limitValueForFoodSourcesArray = new int[populationSize];
            double globalBestFitness = 0;
            double globalBestValueObjectiveFunction;
            double[] globalBestPosition = new double[ProblemDimension];
            List<int> ListFirstFoodSourceID = new();
            int firstFoodSourceID;
            int currentNumberofunctionEvaluation = 0;
            List<double> bestObjectiveFunctionEvaluationData = new List<double>();


            //Variables used to store data about total/Successfull mutation count
            int TotalMutationCount = 0;
            int SuccessfullMutationCount = 0;
            List<int> TotalMutationCountList = new List<int>();
            List<int> SuccessfullMutationCountList = new List<int>();
            int[] foodSourceExploitationCountArray = new int[populationSize];
            int dimensionToUpdateItem = 0;

            int foosSourceIndex;
            double phiValue, distanceBetweenFoodSources, distancetoGlobalBestFoodSource;

            //Varales used to tune AEEABC parameters
            List<double> successfullMutationsRates = new List<double>();
            double currentCycleSuccessfullMutationRate = 0;
            double previousCycleSuccessfullMutationRate = 0;
            int AEEABC_NumberOfDimensionUsingGBest = 1;





            //List<int> nbreDimByIteratList = new List<int>();
            //List<int> nbreGBestDimByIteratList = new List<int>();



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

            for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
            {
                for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                {
                    populationArray[arrayLine, arrayColumn] = searchSpaceMinValue[arrayColumn] + ((double)rand.NextDouble() * searchSpaceRangeValue[arrayColumn]);
                }
                limitValueForFoodSourcesArray[arrayLine] = 0;
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

                        //collect current best function eval list for post data analysis 
                        bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

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

                //collect current best function eval list for post data analysis 
                if (currentNumberofunctionEvaluation % 250000 == 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                //Check to see whether we have depleted the number of allowed function evaluation
                if (currentNumberofunctionEvaluation > maxFunctionEvaluationNumber)
                {
                    TotalMutationCountList.Add(0);
                    SuccessfullMutationCountList.Add(0);

                    //collect current best function eval list for post data analysis 
                    if (currentNumberofunctionEvaluation % 250000 != 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

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
            int iterationNumber;
            //Choosing the all dimensions that need to be updated
            List<int> DimensionToUpdateUsingGbestList = new List<int>();
            //List<int> DimensionAlreadyTaken = new List<int>();


            for (iterationNumber = 0; iterationNumber < maxItertaionNumber; iterationNumber++)
            {

                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                ///

                #region Employed bee phase
                for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                {
                    //Obtaining the position of current particle
                    double[] currentParticlePositionArray = new double[ProblemDimension];

                    for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                    {
                        currentParticlePositionArray[arrayColumn] = populationArray[arrayLine, arrayColumn];
                    }

                    //Updating the food source exploitation count 
                    foodSourceExploitationCountArray[arrayLine]++;


                    //Choosing the randomly selected food source used to update the current food source
                    //Consider as if the currentfood source is not included in the selection 'nbrEmployed-1'
                    ListFirstFoodSourceID.Clear();
                    for (int i = 0; i < nbrEmployed; i++)
                    {
                        if (i != arrayLine)
                        {
                            ListFirstFoodSourceID.Add(i);
                        }
                    }


                    DimensionToUpdateUsingGbestList.Clear();
                    //Updating the desired dimensions
                    for (int i3 = 0; i3 < AEEABC_NumberOfDimensionUsingGBest; i3++)
                    {
                        //Selecting a random dimension to update
                        dimensionToUpdateItem = (int)Math.Floor(rand.NextDouble() * ProblemDimension);
                        if (AEEABC_NumberOfDimensionUsingGBest > 1)
                        {
                            while (DimensionToUpdateUsingGbestList.Contains(dimensionToUpdateItem) == true)
                            {
                                dimensionToUpdateItem = (int)Math.Floor(rand.NextDouble() * ProblemDimension);
                            }
                            DimensionToUpdateUsingGbestList.Add(dimensionToUpdateItem);
                        }

                        //Selecting firstfod source
                        foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                        firstFoodSourceID = ListFirstFoodSourceID[foosSourceIndex];

                        //Computing values for the update equation
                        phiValue = (rand.NextDouble() * 2) - 1;

                        distanceBetweenFoodSources = populationArray[arrayLine, dimensionToUpdateItem] - populationArray[firstFoodSourceID, dimensionToUpdateItem];
                        distancetoGlobalBestFoodSource = globalBestPosition[dimensionToUpdateItem] - populationArray[arrayLine, dimensionToUpdateItem];

                        // update the food source location
                        currentParticlePositionArray[dimensionToUpdateItem] = populationArray[arrayLine, dimensionToUpdateItem]
                                                                            + phiValue * distanceBetweenFoodSources
                                                                            + rand.NextDouble() * 1.5 * distancetoGlobalBestFoodSource;

                        //check if newly food sources within the serach space boundary
                        if (currentParticlePositionArray[dimensionToUpdateItem] > benchmarkFunction.SearchSpaceMaxValue[dimensionToUpdateItem])
                        {
                            currentParticlePositionArray[dimensionToUpdateItem] = benchmarkFunction.SearchSpaceMaxValue[dimensionToUpdateItem];
                        }
                        if (currentParticlePositionArray[dimensionToUpdateItem] < benchmarkFunction.SearchSpaceMinValue[dimensionToUpdateItem])
                        {
                            currentParticlePositionArray[dimensionToUpdateItem] = benchmarkFunction.SearchSpaceMinValue[dimensionToUpdateItem];
                        }
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

                    //Apply the greedy selection on newly generated food source
                    if (currentParticleFitness > fitnessArray[arrayLine])
                    {
                        //Newly generated food source is better
                        for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                        {
                            populationArray[arrayLine, arrayColumn] = currentParticlePositionArray[arrayColumn];
                        }
                        valueObjectiveFunctionArray[arrayLine] = currentParticleValueObjectiveFunction;
                        fitnessArray[arrayLine] = currentParticleFitness;

                        limitValueForFoodSourcesArray[arrayLine] = 0;
                        SuccessfullMutationCount++;

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
                                //if (globalBestValueObjectiveFunction > FunctionValueSigmaTolerance)
                                //{
                                //    int i = 1;
                                //}

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
                    if (currentNumberofunctionEvaluation % 250000 == 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                    //Check to see whether we have depleted the number of allowed function evaluation
                    if ((currentNumberofunctionEvaluation > maxFunctionEvaluationNumber))// || (StopOptimizationWhenOptimumIsReached == true && (Math.Abs(currentObjectiveFunctionOptimum - currentParticleValueObjectiveFunction) < FunctionValueSigmaTolerance || Math.Abs(currentObjectiveFunctionOptimum - currentParticleValueObjectiveFunction) < double.MinValue)))
                    {
                        TotalMutationCountList.Add(TotalMutationCount);
                        SuccessfullMutationCountList.Add(SuccessfullMutationCount);

                        //collect current best function eval list for post data analysis 
                        if (currentNumberofunctionEvaluation % 250000 != 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

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

                // Computing the probability of each food source to be selected by an onlooker

                double[] probabilityArray = new double[nbrEmployed];



                //Computing the total probability
                switch (ABC_ProbabilityEquationType)
                {
                    case ABC_ProbabilityEquationType.Original:

                        //Computing the probability array
                        double costArraySum = fitnessArray.Where((x, Index) => Index >= 0 && Index < nbrEmployed).Sum();

                        //Computing the first part of the new probability exploitation
                        probabilityArray[0] = fitnessArray[0] / costArraySum;

                        for (int arrayLine = 1; arrayLine < nbrEmployed; arrayLine++)
                        {
                            probabilityArray[arrayLine] = probabilityArray[arrayLine - 1] + fitnessArray[arrayLine] / costArraySum;
                        }

                        break;

                    case ABC_ProbabilityEquationType.ComplementOriginal:

                        //Computing the inverse of the probability equation
                        double inverseCostArraySum = 0;

                        for (int arrayLine = 0; arrayLine < nbrEmployed; arrayLine++)
                        {
                            inverseCostArraySum = inverseCostArraySum + (1 / fitnessArray[arrayLine]);
                        }

                        probabilityArray[0] = (1 / fitnessArray[0]) / inverseCostArraySum;

                        for (int arrayLine = 1; arrayLine < nbrEmployed; arrayLine++)
                        {
                            probabilityArray[arrayLine] = probabilityArray[arrayLine - 1] + ((1 / fitnessArray[arrayLine]) / inverseCostArraySum);
                        }

                        break;
                    default:
                        break;
                }

                probabilityArray[nbrEmployed - 1] = 1;

                //Browse through all onlookers
                for (int arrayLine = 0; arrayLine < nbrOnlooker; arrayLine++)
                {
                    //1- choosing the  food source that the current onlooker will go to
                    double randomValueSmallerThan1;
                    int selectedFoodSourceID = 0;
                    randomValueSmallerThan1 = rand.NextDouble();

                    for (int foodSourceID = 0; foodSourceID < nbrEmployed; foodSourceID++)
                    {
                        if (probabilityArray[foodSourceID] > randomValueSmallerThan1)
                        {
                            selectedFoodSourceID = foodSourceID;
                            break;
                        }
                    }

                    //2- Obtaining the position of the selected food source
                    double[] currentParticlePositionArray = new double[ProblemDimension];

                    for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                    {
                        currentParticlePositionArray[arrayColumn] = populationArray[selectedFoodSourceID, arrayColumn];
                    }

                    //Updating the food source exploitation count 
                    foodSourceExploitationCountArray[selectedFoodSourceID]++;




                    //Choosing the randomly selected food source used to update the current food source
                    //Consider as if the currentfood source is not included in the selection 'nbrEmployed-1'
                    ListFirstFoodSourceID.Clear();
                    for (int i = 0; i < nbrEmployed; i++)
                    {
                        if (i != arrayLine)
                        {
                            ListFirstFoodSourceID.Add(i);
                        }


                    }


                    DimensionToUpdateUsingGbestList.Clear();
                    //Updating the desired dimensions
                    for (int i3 = 0; i3 < AEEABC_NumberOfDimensionUsingGBest; i3++)
                    {
                        //Selecting a random dimension to update
                        dimensionToUpdateItem = (int)Math.Floor(rand.NextDouble() * ProblemDimension);
                        if (AEEABC_NumberOfDimensionUsingGBest > 1)
                        {
                            while (DimensionToUpdateUsingGbestList.Contains(dimensionToUpdateItem) == true)
                            {
                                dimensionToUpdateItem = (int)Math.Floor(rand.NextDouble() * ProblemDimension);
                            }
                            DimensionToUpdateUsingGbestList.Add(dimensionToUpdateItem);
                        }

                        //Selecting firstfood source
                        foosSourceIndex = (int)Math.Floor(rand.NextDouble() * ListFirstFoodSourceID.Count);
                        firstFoodSourceID = ListFirstFoodSourceID[foosSourceIndex];

                        //Computing values for the update equation
                        phiValue = (rand.NextDouble() * 2) - 1;

                        distanceBetweenFoodSources = populationArray[selectedFoodSourceID, dimensionToUpdateItem] - populationArray[firstFoodSourceID, dimensionToUpdateItem];
                        distancetoGlobalBestFoodSource = globalBestPosition[dimensionToUpdateItem] - populationArray[selectedFoodSourceID, dimensionToUpdateItem];

                        // update the food source location
                        currentParticlePositionArray[dimensionToUpdateItem] = populationArray[selectedFoodSourceID, dimensionToUpdateItem]
                                                                            + phiValue * distanceBetweenFoodSources
                                                                            + rand.NextDouble() * 1.5 * distancetoGlobalBestFoodSource;


                        //check if newly food sources within the serach space boundary
                        if (currentParticlePositionArray[dimensionToUpdateItem] > benchmarkFunction.SearchSpaceMaxValue[dimensionToUpdateItem])
                        {
                            currentParticlePositionArray[dimensionToUpdateItem] = benchmarkFunction.SearchSpaceMaxValue[dimensionToUpdateItem];
                        }
                        if (currentParticlePositionArray[dimensionToUpdateItem] < benchmarkFunction.SearchSpaceMinValue[dimensionToUpdateItem])
                        {
                            currentParticlePositionArray[dimensionToUpdateItem] = benchmarkFunction.SearchSpaceMinValue[dimensionToUpdateItem];
                        }
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


                    //Apply the greedy selection on newly generated food source
                    if (currentParticleFitness > fitnessArray[selectedFoodSourceID])
                    {
                        //Newly generated food source is better
                        for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                        {
                            populationArray[selectedFoodSourceID, arrayColumn] = currentParticlePositionArray[arrayColumn];
                        }

                        valueObjectiveFunctionArray[selectedFoodSourceID] = currentParticleValueObjectiveFunction;
                        fitnessArray[selectedFoodSourceID] = currentParticleFitness;

                        limitValueForFoodSourcesArray[selectedFoodSourceID] = 0;
                        SuccessfullMutationCount++;

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
                    if (currentNumberofunctionEvaluation % 250000 == 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                    //Check to see whether we have depleted the number of allowed function evaluation
                    if (currentNumberofunctionEvaluation > maxFunctionEvaluationNumber)
                    {
                        TotalMutationCountList.Add(TotalMutationCount);
                        SuccessfullMutationCountList.Add(SuccessfullMutationCount);

                        //collect current best function eval list for post data analysis 
                        if (currentNumberofunctionEvaluation % 250000 != 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

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
                int maxLimitValueForFoodSources = -1;
                int maxLimitValueForFoodSourcesID = -1;

                for (int foodSourceID = 0; foodSourceID < nbrEmployed; foodSourceID++)
                {
                    if (limitValueForFoodSourcesArray[foodSourceID] > limitValue)
                    {
                        maxLimitValueForFoodSources = limitValueForFoodSourcesArray[foodSourceID];
                        maxLimitValueForFoodSourcesID = foodSourceID;

                        break;
                    }
                }

                //Check to see if the 'maxLimitValueForFoodSources' has exceeded the 'limit' parameter
                if (maxLimitValueForFoodSourcesID != -1)
                {
                    numerOfScoutBeesGenerated++;
                    // Replace the 'maxLimitValueForFoodSourcesID' food source with a ramdomly generated one
                    switch (scoutGenerationScheme)
                    {
                        case ScoutGenerationType.Random:
                            for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                            {
                                populationArray[maxLimitValueForFoodSourcesID, arrayColumn] = searchSpaceMinValue[arrayColumn] + ((double)rand.NextDouble() * searchSpaceRangeValue[arrayColumn]);
                                limitValueForFoodSourcesArray[maxLimitValueForFoodSourcesID] = 0;
                            }
                            break;

                        //The new source (scout) will be genrated by computing the mean of all other vurrent solutions
                        case ScoutGenerationType.MeanExistingSolution:
                            for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                            {
                                populationArray[maxLimitValueForFoodSourcesID, arrayColumn] = 0;

                                for (int j_SolutionItem = 0; j_SolutionItem < nbrEmployed; j_SolutionItem++)
                                {
                                    if (maxLimitValueForFoodSourcesID != j_SolutionItem)
                                    {
                                        populationArray[maxLimitValueForFoodSourcesID, arrayColumn] += populationArray[j_SolutionItem, arrayColumn];
                                    }
                                }

                                populationArray[maxLimitValueForFoodSourcesID, arrayColumn] = populationArray[maxLimitValueForFoodSourcesID, arrayColumn] / ((double)nbrEmployed - 1.0);
                            }
                            limitValueForFoodSourcesArray[maxLimitValueForFoodSourcesID] = 0;

                            break;

                        default:
                            for (int arrayColumn = 0; arrayColumn < ProblemDimension; arrayColumn++)
                            {
                                populationArray[maxLimitValueForFoodSourcesID, arrayColumn] = searchSpaceMinValue[arrayColumn] + ((double)rand.NextDouble() * searchSpaceRangeValue[arrayColumn]);
                                limitValueForFoodSourcesArray[maxLimitValueForFoodSourcesID] = 0;
                            }

                            break;
                    }


                    //Resetting exploitation count data for replaced food source
                    foodSourceExploitationCountArray[maxLimitValueForFoodSourcesID] = 0;


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
                    if (currentNumberofunctionEvaluation % 250000 == 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

                    //Check to see whether we have depleted the number of allowed function evaluation
                    if (currentNumberofunctionEvaluation > maxFunctionEvaluationNumber)
                    {
                        TotalMutationCountList.Add(TotalMutationCount);
                        SuccessfullMutationCountList.Add(SuccessfullMutationCount);

                        //collect current best function eval list for post data analysis 
                        if (currentNumberofunctionEvaluation % 250000 != 0) bestObjectiveFunctionEvaluationData.Add(globalBestValueObjectiveFunction);

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



                TotalMutationCountList.Add(TotalMutationCount);
                SuccessfullMutationCountList.Add(SuccessfullMutationCount);






                #region Parameters Tuning Process
                varIterNumberAfterLastTuningCycle++;
                //Check to see if the tuning process should be executed

                if (varIterNumberAfterLastTuningCycle == AEEABC_NumberOfIterationsToTuneParameters)
                {
                    varIterNumberAfterLastTuningCycle = 1;

                    //nbreDimByIteratList.Add(AEEABC_NumberDimensionToUpdate);
                    //nbreGBestDimByIteratList.Add(AEEABC_NumberOfDimensionUsingGBest);


                    //Getting the mutation successfull rate for this tuning cycle
                    int totalMutationTuning = 0;
                    int totalSuccessfullMutationTuning = 0;


                    if (iterationNumber < AEEABC_NumberOfIterationsToTuneParameters)
                    {
                        totalMutationTuning = TotalMutationCountList[iterationNumber];
                        totalSuccessfullMutationTuning = SuccessfullMutationCountList[iterationNumber];
                    }
                    else
                    {
                        totalMutationTuning = TotalMutationCountList[iterationNumber] - TotalMutationCountList[iterationNumber - AEEABC_NumberOfIterationsToTuneParameters];
                        totalSuccessfullMutationTuning = SuccessfullMutationCountList[iterationNumber] - SuccessfullMutationCountList[iterationNumber - AEEABC_NumberOfIterationsToTuneParameters];
                    }

                    successfullMutationsRates.Add((double)totalSuccessfullMutationTuning / (double)totalMutationTuning);



                    //Getting the successfull mutation rate data for this cycle and the previous
                    currentCycleSuccessfullMutationRate = successfullMutationsRates.Last();

                    if (successfullMutationsRates.Count == 1)
                    {
                        previousCycleSuccessfullMutationRate = 0;
                    }
                    else
                    {
                        previousCycleSuccessfullMutationRate = successfullMutationsRates[successfullMutationsRates.Count - 2];
                    }



                    if (currentCycleSuccessfullMutationRate < previousCycleSuccessfullMutationRate)
                    {//current successfull mutatio rate is worst than the previous value
                     //Inverse the algorithm Exploration Vs Exploitation status
                        if (currentExplorationVsExploitationStatus == ExplorationVsExploitationType.DriveTowardExploration)
                        {
                            currentExplorationVsExploitationStatus = ExplorationVsExploitationType.DriveTowardExploitation;
                        }
                        else
                        {
                            currentExplorationVsExploitationStatus = ExplorationVsExploitationType.DriveTowardExploration;
                        }

                    }



                    if (currentExplorationVsExploitationStatus == ExplorationVsExploitationType.DriveTowardExploration)
                    {
                        //Drive the algorithm to do more exploration

                        if (AEEABC_TuneNumberOfDimensionUsingGBest == true)
                        {
                            //reduce the number of Gbest dimension to increase exploration
                            AEEABC_NumberOfDimensionUsingGBest--;

                            //Limit the lowest value to 1
                            if (AEEABC_NumberOfDimensionUsingGBest < 1) AEEABC_NumberOfDimensionUsingGBest = 1;
                        }


                        //Use the original probability equation to increase exploration capabilities of the algorithm
                        if (AEEABC_TuneProbabilityEquationTypeParameter == true)
                        {
                            ABC_ProbabilityEquationType = ABC_ProbabilityEquationType.ComplementOriginal;
                        }


                        //Use the original scout bee generation scheme to increase exploration capabilities of the algorithm
                        if (AEEABC_TuneScoutGenerationTypeParameters == true)
                        {
                            scoutGenerationScheme = ScoutGenerationType.Random;
                        }


                    }
                    else
                    {
                        //Drive the algorithm to do more exploitation

                        if (AEEABC_TuneNumberOfDimensionUsingGBest == true)
                        {
                            //reduce the number of Gbest dimension to increase exploitation
                            AEEABC_NumberOfDimensionUsingGBest++;

                            //Limit the lowest value to 1
                            if (AEEABC_NumberOfDimensionUsingGBest > ProblemDimension) AEEABC_NumberOfDimensionUsingGBest = ProblemDimension;
                        }


                        //Use the original probability equation to increase exploitation capabilities of the algorithm
                        if (AEEABC_TuneProbabilityEquationTypeParameter == true)
                        {
                            ABC_ProbabilityEquationType = ABC_ProbabilityEquationType.Original;
                        }


                        //Use the original scout bee generation scheme to increase exploitation capabilities of the algorithm
                        if (AEEABC_TuneScoutGenerationTypeParameters == true)
                        {
                            scoutGenerationScheme = ScoutGenerationType.MeanExistingSolution;
                        }

                    }


                }


                #endregion

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
        /// <param name="numberofValuesToGenerate">The number of generated values to be returned as array of ints </param>
        /// <returns>a random zero based list of indexes</returns>
        private List<int> FindZeroBasedRandomIndex(int maxValue, List<int> indexesToIgnoreList, Random rand, int numberofValuesToGenerate)
        {
            //List<int> indexList = new();
            List<int> indexList = new();
            List<int> resultList = new();

            for (int i = 0; i <= maxValue; i++)
            {
                if (indexesToIgnoreList != null && indexesToIgnoreList.Contains(i) == false)
                {
                    indexList.Add(i);
                }
            }

            int tempVal;

            for (int i = 0; i < numberofValuesToGenerate; i++)
            {
                tempVal = (int)Math.Floor(rand.NextDouble() * indexList.Count);
                resultList.Add(indexList[tempVal]);

                indexList.RemoveAt(tempVal);
            }

            return resultList;
        }



    }
}


