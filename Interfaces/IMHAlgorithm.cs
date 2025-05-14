using MHPlatTest.Algorithms;
using MHPlatTest.BenchmarkFunctions;
using MHPlatTest.Divers;
using MHPlatTest.Models;
using MHPlatTest.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace MHPlatTest.Interfaces
{
    /// <summary>
    /// /Use to present a metaheuristic algorithm
    /// </summary>
    //[JsonInterfaceConverter(typeof(InterfaceConverter<IMHAlgorithm>))]+
    [JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(AdaABC), typeDiscriminator: "AdaABC")]
    internal interface IMHAlgorithm
    {
        /// <summary>
        /// The name of the metaheuristic algorithm
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Contains the configuration to be applied with current optimization algorithm
        /// </summary>
        List<OptimizationParameter> OptimizationConfiguration { get; set; }

        /// <summary>
        /// description of the current optimization algorithm
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Contains an unique idenifier used to differentiate 
        /// between created instances especially when they are of the same type
        /// </summary>
        int InstanceID { get; set; }



        //double ComputeOptimum(short populationSize, short maxItertaionNumber, short maxFunctionEvaluationNumber = 0, IBenchmark benchmarkFunction = null, short nbrProblemDimension = 0, bool ComputeMin = true);
        /// <summary>
        /// Start an optimization process with a customized configuration for a given Benchmark function
        /// </summary>
        /// <param name="threadSafeMethodArgs">all the args need to be included in a single object to thread safe the method</param>
        /// 
        /// <returns></returns>
        void ComputeOptimum(IBenchmark benchmarkFunction, List<OptimizationResultModel> resultsData, int randomSeed, double[,]? initialSolutions = null);

        void PrepareResultData(List<OptimizationResultModel> data, MHOptimizationResult dataName, object dataValue);

        /// <summary>
        /// Call this method to make hard personal copy of reference type 'optimizationConfiguration'
        /// </summary>
        /// <param name="optimizationConfiguration">the list of optimization parameters to apply to the current instance of metaheuristic optimization algorithm</param>
        /// <param name="description">the description of the current instance of metaheuristic optimization algorithm</param>
        /// <param name="randomIntValue"></param>
        void MakePersonalOptimizationConfigurationListCopy(List<OptimizationParameter> optimizationConfiguration, string description, int randomIntValue);

    }
}
