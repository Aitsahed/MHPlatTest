using MHPlatTest.Divers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MHPlatTest.Models
{
    internal class OptimizationResultModel
    {
        public OptimizationResultModel()
        {

        }

        [JsonConstructor]
        public OptimizationResultModel(MHOptimizationResult name, object value, bool isEssentialInfo = false)
        {

            if (value.GetType().Equals(typeof(System.Text.Json.JsonElement)) == false)
            {
                Name = name;
                Value = value;
                IsEssentialInfo = isEssentialInfo;
                return;
            }

            System.Text.Json.JsonElement test;
            Name = name;
            IsEssentialInfo = isEssentialInfo;


            test = (System.Text.Json.JsonElement)value;
            if (test.ValueKind == System.Text.Json.JsonValueKind.Number || test.ValueKind == System.Text.Json.JsonValueKind.True || test.ValueKind == System.Text.Json.JsonValueKind.False || test.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                switch (name)
                {
                    case MHOptimizationResult.OptimalFunctionValue:
                        value = test.GetDouble();
                        break;
                    case MHOptimizationResult.OptimalPoint:
                        List<double> points = new List<double>();
                        for (int i = 0; i < test.GetArrayLength(); i++)
                        {
                            points.Add(test[i].GetDouble());
                        }
                        value = points;
                        break;
                    case MHOptimizationResult.NumberOfTotalIteration:
                        value = test.GetInt32();
                        break;
                    case MHOptimizationResult.NumberOfFunctionEvaluation:
                        value = test.GetInt32();
                        break;
                    case MHOptimizationResult.ExecutionTime:
                        value = test.GetInt32();
                        break;
                    case MHOptimizationResult.OptimumFound:
                        value = test.GetBoolean();
                        break;
                    case MHOptimizationResult.TotalMutationCountData:
                        points = new List<double>();
                        for (int i = 0; i < test.GetArrayLength(); i++)
                        {
                            points.Add(test[i].GetInt32());
                        }
                        value = points;
                        break;
                    case MHOptimizationResult.TotalSuccessfullMutationCountData:
                        points = new List<double>();
                        for (int i = 0; i < test.GetArrayLength(); i++)
                        {
                            points.Add(test[i].GetInt32());
                        }
                        value = points;
                        break;
                    case MHOptimizationResult.SuccessfullMutationRate:
                        value = test.GetDouble();
                        break;
                    //case MHOptimizationResult.ProcessStatus:
                    //    List<List<double>> dataPoints = new List<List<double>>();
                    //    for (int i = 0; i < test.GetArrayLength(); i++)
                    //    {
                    //        dataPoints.Add(test[i].);
                    //    }
                    //    value = dataPoints;
                    //    break;
                    default:
                        break;
                }
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            Value = value;
        }

        /// <summary>
        /// The name of the data object
        /// </summary>
        public MHOptimizationResult Name { get; set; }

        /// <summary>
        /// The value attached to the current data object
        /// </summary>
        public object Value { get; set; }


        /// <summary>
        /// Indicate whether the parameter is an important one.
        /// the important parameter details are displayed to indicate the progress of the optimization process especially in a batch (sequence) of optimization processes
        /// </summary>
        public bool IsEssentialInfo { get; set; } = false;
    }
}
