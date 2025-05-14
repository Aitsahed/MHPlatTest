using MHPlatTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
//using Newtonsoft.Json;
using MHPlatTest.Utility;
using System.Text.Json.Serialization;

namespace MHPlatTest.Models
{
    internal class GlobalBatchResultModel
    {
        public GlobalBatchResultModel()
        {
        }

        public GlobalBatchResultModel(IMHAlgorithm mHAlgorithm, IBenchmark benchmarkFunction, int repetitionID, List<OptimizationResultModel> optimizationResults)
        {
            MHAlgorithm = mHAlgorithm;
            BenchmarkFunction = benchmarkFunction;
            RepetitionID = repetitionID;
            OptimizationResults = optimizationResults;
        }

        /// <summary>
        /// the metaheuristic algorithm used within the optimization process
        /// </summary>
        public IMHAlgorithm MHAlgorithm { get; set; }




        /// <summary>
        /// The benchmark function object
        /// </summary>
        
        public IBenchmark BenchmarkFunction { get; set; }


        /// <summary>
        /// the number of current repetition
        /// </summary>
        public int RepetitionID { get; set; }

        /// <summary>
        /// All data compiled at the end of the optimization process
        /// </summary>
        public List<OptimizationResultModel> OptimizationResults { get; set; }
    }



    internal class GlobalBatchResultModelWithoutInterfaceProblem
    {
        public GlobalBatchResultModelWithoutInterfaceProblem()
        {
        }

        public GlobalBatchResultModelWithoutInterfaceProblem(GlobalBatchResultModel globalBatchResult)
        {
            //OptimizationResultModel optimizationResultModel;

            MHAlgorithm = globalBatchResult.MHAlgorithm.Name + " " + globalBatchResult.MHAlgorithm.InstanceID;
            BenchmarkFunction = globalBatchResult.BenchmarkFunction.Name + ":" + globalBatchResult.BenchmarkFunction.ParentInstanceID;
            RepetitionID = globalBatchResult.RepetitionID;

            //foreach (var item in globalBatchResult.OptimizationResults)
            //{
            //    optimizationResultModel = new(item.Name, item.Value, item.IsEssentialInfo);
            //}

            OptimizationResults = globalBatchResult.OptimizationResults;
            OptimizationConfiguration = globalBatchResult.MHAlgorithm.OptimizationConfiguration;
        }

        /// <summary>
        /// the metaheuristic algorithm used within the optimization process
        /// </summary>
        public string MHAlgorithm { get; set; }




        /// <summary>
        /// The benchmark function object
        /// </summary>
        public string BenchmarkFunction { get; set; }


        /// <summary>
        /// Contains the configuration to be applied with current optimization algorithm
        /// </summary>
        List<OptimizationParameter> OptimizationConfiguration { get; set; }



        /// <summary>
        /// the number of current repetition
        /// </summary>
        public int RepetitionID { get; set; }

        /// <summary>
        /// All data compiled at the end of the optimization process
        /// </summary>
        public List<OptimizationResultModel> OptimizationResults { get; set; }
    }
}
