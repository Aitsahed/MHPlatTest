using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHPlatTest.Divers
{
    class EnumTypes
    {

    }

    /// <summary>
    /// The adopted stopping criteria used by the algorithm
    /// </summary>
    public enum StoppingCriteriaType : Int16
    {
        /// <summary>
        /// the algorithm will stop when the number of iteration reaches a predetermined value
        /// </summary>
        MaximalNumberOfIteration = 1,
        /// <summary>
        /// the algorithm will stop when the number of objective function evaluation reaches a predetermined value
        /// or the number of  MaximalNumberOfIteration reaches a million
        /// </summary>
        MaximalNumberOfFunctionEvaluation = 2,

        /// <summary>
        /// NOT YET IMPLEMENTED 
        /// The algorithm will stopp if the objective function evaluation value reaches a predetermined value
        /// or the number of  MaximalNumberOfIteration reaches a million
        /// NOT YET IMPLEMENTED
        /// </summary>
        FunctionValueTolerance = 3
    }


    /// <summary>
    /// The adopted stopping criteria used by the algorithm
    /// </summary>
    public enum OptimizationProblemType : Int16
    {
        /// <summary>
        /// the current optimization problem is a maximization problem
        /// </summary>
        Maximization = 1,
        /// <summary>
        /// the current optimization problem is a minimization problem
        /// </summary>
        Minimization = 2
    }


    /// <summary>
    /// The adopted population initialization scheme
    /// </summary>
    public enum PopulationInitilizationType : Int16
    {
        /// <summary>
        /// adopt a random population initialization
        /// </summary>
        Random = 1
    }


    /// <summary>
    /// The adopted scout generation scheme
    /// </summary>
    public enum ScoutGenerationType : Int16
    {
        /// <summary>
        /// adopt a random generated scout
        /// </summary>
        Random = 0,
        /// <summary>
        /// Generate scouts by computing the mean of all rxisting solution
        /// </summary>
        MeanExistingSolution = 1,
        /// <summary>
        /// Generate scouts by computing a randomly wieghted mean existing solution
        /// </summary>
        RandomlyWeighthedExistingSolution = 2,
    }


    /// <summary>
    /// The adopted probability equation type of the ABC algo
    /// </summary>
    public enum ABC_ProbabilityEquationType : Int16
    {
        /// <summary>
        /// the original probability equation found in the basic ABC
        /// </summary>
        Original = 0,
        /// <summary>
        /// a peobability equation that select the good and less exploited food sources for further exploitation
        /// </summary>
        EqualExploitation = 1,
        /// <summary>
        /// a probability equation  that selects only the less exploited food source for further exploitation
        /// </summary>
        LessExploitedOnly = 2,
        /// <summary>
        /// a probability equation  that is a complement (inverse) to the original probabilities
        /// </summary>
        ComplementOriginal = 3,
    }



    /// <summary>
    /// contains the different existing parameters that
    /// a user can change about any metaHeuristic Algrithms
    /// </summary>
    public enum MHAlgoParameters : Int16
    {
        /// <summary>
        /// define the population size
        /// </summary>
        PopulationSize = 1,
        /// <summary>
        /// define the Stopping Criteria.
        /// Based on this choice, other options concerning
        /// stopping criteria will be ignored
        /// </summary>
        StoppingCriteriaType = 2,
        /// <summary>
        /// define the maximum Itertaion Number (Stopping crteria)
        /// </summary>
        MaxItertaionNumber = 3,
        /// <summary>
        /// define the maximum Function Evaluation Number (Stopping crteria)
        /// </summary>
        MaxFunctionEvaluationNumber = 4,
        /// <summary>
        /// define the minimum value to which the objective function need to be enhanced in order to continu the optimization process
        /// if the function value enhancement is below this threshold, the optimization process will be stopped (Stopping criteria)
        /// </summary>
        FunctionValueMinimumEnhancementThreshold = 5,
        /// <summary>
        /// define the Problem Dimension
        /// </summary>
        ProblemDimension = 6,
        /// <summary>
        /// define the optimization Type
        /// </summary>
        OptimizationType = 7,
        /// <summary>
        /// define the population Initilization Scheme
        /// </summary>
        PopulationInitilization = 8,
        /// <summary>
        /// define the limit parameter value for the ABC algo
        /// </summary>
        ABC_LimitValue = 9,
        /// <summary>
        /// define the C_Constant parameter value for the PSO algo
        /// </summary>
        PSO_C_Constant = 10,
        /// <summary>
        /// define the X_Constant parameter value for the PSO algo
        /// </summary>
        PSO_X_Constant = 11,
        /// <summary>
        /// define the function Value below which the evaluation will be considered equals to '0'
        /// </summary>
        FunctionValueSigmaTolerance = 12,
        /// <summary>
        /// Force the optimum of the objective function to be '0' by modifying the method 'ComputeValue' 
        /// to substract the actual optimal value from the function objective
        /// </summary>
        ShiftObjectiveFunctionOptimumValueToZero = 13,
        /// <summary>
        /// define whether the optimization process should be stopped when the optimal value has been reached
        /// </summary>
        StopOptimizationWhenOptimumIsReached = 14,
        /// <summary>
        /// define the modification rate (MR) parameter value for the MABC algorithm
        /// </summary>
        MABC_ModificationRate = 15,
        /// <summary>
        /// define if the Scaling Factor (PhiRange) parameter value (Magnitude of the mutation) for the MABC algorithm will be used
        /// </summary>
        MABC_UseScalingFactor = 16,
        /// <summary>
        /// define the limit parameter value for the MABC algorithm 
        /// </summary>
        MABC_LimitValue = 17,
               /// <summary>
        /// Whether the algorithm will be tuning the number of dimension to be updated using the GBest solution and not a random food source
        /// </summary>
        AEEABC_TuneNumberOfDimensionUsingGBest = 23,
        /// <summary>
        /// define the required number of iterations that the algorithm will wait to tune the adjustable parameters
        /// </summary>
        AEEABC_NumberOfIterationsToTuneParameters = 24,
        /// <summary>
        /// Whether the algo will update all dimension being updated using the GBest equation
        /// (NumberOfDimensionUsingGBest=NumberOfDimensionToUpdate)
        /// </summary>
      
        /// <summary>
        /// Define how the scout will be generated
        /// </summary>
        ScoutGeneration = 45,
      
        /// <summary>
        /// Define how what type of probability equation to use with ABC algos
        /// </summary>
        ABC_ProbabilityEquationType = 47,
        /// <summary>
        /// Define whther the scout bee generation type will be tuned between (random, meanExisting)
        /// </summary>
        AEEABC_TuneScoutGenerationType = 48,
        /// <summary>
        /// Define whther the scout bee generation type will be tuned between (random, meanExisting)
        /// </summary>
        AEEABC_TuneProbabilityEquationType = 49,
    }

    /// <summary>
    /// Contain all data compiled at the end of an optimization process
    /// </summary>
    public enum MHOptimizationResult : Int16
    {
        /// <summary>
        /// contains the optimal objective function obtained at 
        /// the end of the optimization process
        /// </summary>
        OptimalFunctionValue = 1,
        /// <summary>
        /// contains the optimal point which correspond to the 
        /// optimal function value at the end of the optimization process
        /// </summary>
        OptimalPoint = 2,
        /// <summary>
        /// contains the last iteration number at which the optimization 
        /// process has been stopped
        /// </summary>
        NumberOfTotalIteration = 3,
        /// <summary>
        /// contains the number of function evaluation performed all along the optimization process 
        /// </summary>
        NumberOfFunctionEvaluation = 4,
        /// <summary>
        /// The elapsed time needed to complete the optimization process 
        /// </summary>
        ExecutionTime = 5,
        /// <summary>
        /// Indicates whether the optimization process was able to locate the optimum
        /// 'true' if the optimization process has located the optimum
        /// </summary>
        OptimumFound = 6,
        /// <summary>
        /// Contains the data about the total number of mutation at the end of each iteration
        /// </summary>
        TotalMutationCountData = 7,
        /// <summary>
        /// Contains the data about the total number of successfull mutation at the end of each iteration
        /// </summary>
        TotalSuccessfullMutationCountData = 8,
        /// <summary>
        /// Contains the successfullrate of mutation at the end of the optimzation process
        /// </summary>
        SuccessfullMutationRate = 9,
        /// <summary>
        /// Contains the data about the Bset objective function evaluation in each iteration
        /// </summary>
        ObjectiveFunctionEvaluationData = 10,
        /// <summary>
        /// Contains the number of trials in which the algorithm has converged to zero
        /// </summary>
        ConvergenceToZeroCount = 11,
        /// <summary>
        /// Contains the process details (current inputs, outputs and correspondant time)
        /// </summary>
        ProcessActualOutputsList = 12,
        /// <summary>
        /// Contains the process details (current inputs, outputs and correspondant time)
        /// </summary>
        ProcessCommandList = 13,
        /// <summary>
        /// Contains the process details (current inputs, outputs and correspondant time)
        /// </summary>
        ProcessMSE = 14,
        /// <summary>
        /// Contains the process details (current inputs, outputs and correspondant time)
        /// </summary>
        ProcessMCV = 15,
        /// <summary>
        /// Contains the scout that was generated when the optimization process was running
        /// </summary>
        ScoutBeesGeneratedCount = 16,
    }

    /// <summary>
    /// The different stats to be applied to the optimization results
    /// </summary>
    public enum StatsToComputeType : Int16
    {
        /// <summary>
        /// compute the mean of an array of numeric values
        /// </summary>
        Mean = 1,
        /// <summary>
        /// Compute the standard deviation of an array of numeric values
        /// </summary>
        STD = 2,
        /// <summary>
        /// retrieve the maximum value within an array of numeric values
        /// </summary>
        Max = 3,
        /// <summary>
        /// retrieve the minimum value within an array of numeric values
        /// </summary>
        Min = 4,
        /// <summary>
        /// retrieve the median value within an array of numeric values
        /// </summary>
        Median = 5,
    }


    /// <summary>
    /// The different ordering types
    /// </summary>
    public enum OrderingType : Int16
    {
        /// <summary>
        /// order in an ascending maner
        /// </summary>
        Ascending = 1,
        /// <summary>
        /// order in a descending maner
        /// </summary>
        Descending = 2,
        /// <summary>
        /// No norder, leave the list as is
        /// </summary>
        None = 3,

    }

    /// <summary>
    /// The different ordering types
    /// </summary>
    public enum GroupByType : Int16
    {
        /// <summary>
        /// Group by MH algos
        /// </summary>
        Algorithm = 1,
        /// <summary>
        /// Group by Functions
        /// </summary>
        BenchmarkFunction = 2,


    }

    /// <summary>
    /// The different AEEABC tunning parametres types
    /// </summary>
    public enum AEEABCtunningParametersNames : Int16
    {
        /// <summary>
        /// Number of dimension parameter
        /// </summary>
        NumberDimensionToUpdate = 1,
        /// <summary>
        ///Value of 'limit' parameter
        ///  </summary>
        LimitValue = 2,
        /// <summary>
        /// Scaling factor (magnitude of the perturbation)
        /// </summary>
        PhiRangeValue = 3,
        /// <summary>
        /// number of updated dimensions using the GBest solution
        /// </summary>
        NumberOfDimensionUsingGBest = 4,
        /// <summary>
        /// number of emplyed and onlookers at the curent iteration
        /// </summary>
        NumberEmployedOnlookers = 5,
        /// <summary>
        /// The alpha parameyter used for ABC V4 Equal Exploitation
        /// </summary>
        AlphaEqualExploitValue = 6,
        /// <summary>
        /// The beta parameyter used for the ABC update equation 
        /// </summary>
        Coeff1Value = 7,
        /// <summary>
        /// The beta parameyter used for the ABC update equation
        /// </summary>
        Coeff2Value = 8,
        /// <summary>
        /// The beta parameyter used for the ABC update equation
        /// </summary>
        Coeff3Value = 9,
        /// <summary>
        /// How to generate a new scout (randomly or MeanExisting)
        /// </summary>
        ScoutGenerationType = 10,

        /// <summary>
        /// Whjich probability equation to use for onlookers bees
        /// </summary>
        ProbabilityEquationType = 11,

    }




    /// <summary>
    /// The different AEEABC tunning for the employed bee
    /// Add, remove or Restore
    /// </summary>
    public enum AAEABCTuningEmployedBeeModificationType : Int16
    {
        /// <summary>
        /// W are adding a new employed bee to the current population
        /// </summary>
        AddNewEmployedBee = 1,
        /// <summary>
        ///we are removing the least fit employed bee from current population
        ///  </summary>
        RemoveExistingEmployedBee = 2,
        /// <summary>
        /// we are restoring a previously removed employed bee to the current popuolation
        /// </summary>
        //RestorePreviouslyRemovedEmployedBee = 3,
        /// <summary>
        /// DO nothing
        /// </summary>
        Nothing = 4,
    }

    /// <summary>
    /// The different variation the update equation
    /// </summary> 
    public enum AEEBCKciParType : Int16
    {
        /// <summary>
        /// the kci parameter isnot adapted
        /// </summary>
        KciGbestABCAlgo = 1,
        /// <summary>
        ///the Kci parameter will be randomly chosen from [0,1-Phi Parameter Value]
        ///  </summary>
        KciRandomBasedOnPhi = 2,
        /// <summary>
        /// chose the value as Kci=1-Phi value
        /// </summary>
        KciStaticBasedOnPhi = 3
    }


    /// <summary>
    /// The different ordering types
    /// </summary>
    public enum ExplorationVsExploitationType : Int16
    {
        /// <summary>
        /// Group by MH algos
        /// </summary>
        DriveTowardExploration = 1,
        /// <summary>
        /// Group by Functions
        /// </summary>
        DriveTowardExploitation = 2,


    }



    /// <summary>
    /// The adopted stopping criteria used by the algorithm
    /// </summary>
    public enum BenchmarkOrProcessType : Int16
    {
        /// <summary>
        /// the current optimization problem is a maximization problem
        /// </summary>
        BenchmarkFunction = 1,
        /// <summary>
        /// the current optimization problem is a minimization problem
        /// </summary>
        Process = 2
    }




    /// <summary>
    /// the type of optimization experience to run
    /// </summary>
    public enum OptimizationExperienceToRunType : Int16
    {
        /// <summary>
        /// Evaluate the optimization algorithms using the benchmark functions
        /// </summary>
        NumericalBenchmarkFunctionTests = 1,
        /// <summary>
        /// evaluate the conergence rate and speed of the optimization algorithms
        /// </summary>
        ConvergenceBenchmarkFunctionTest = 2,
        /// <summary>
        /// evaluate the optimization algorithm using a Nonlinear Model Predictive contorl problem
        /// </summary>
        ControlProcessTests = 3,
    }



}
