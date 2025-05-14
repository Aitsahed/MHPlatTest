using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using MHPlatTest.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MHPlatTest.Models
{/// <summary>
 /// Standard parameter
 /// </summary>

    internal class OptimizationParameter
    {




        public OptimizationParameter()
        {

        }

        [JsonConstructor]
        public OptimizationParameter(MHAlgoParameters name, object value, bool isEssentialInfo = false)
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
            if (test.ValueKind == System.Text.Json.JsonValueKind.Number || test.ValueKind == System.Text.Json.JsonValueKind.True || test.ValueKind == System.Text.Json.JsonValueKind.False)
            {
                switch (Name)
                {
                    case MHAlgoParameters.PopulationSize:
                        value = test.GetInt32();
                        break;
                    case MHAlgoParameters.StoppingCriteriaType:
                        value = (StoppingCriteriaType)test.GetInt32();
                        break;
                    case MHAlgoParameters.MaxItertaionNumber:
                        value = test.GetInt32();
                        break;
                    case MHAlgoParameters.MaxFunctionEvaluationNumber:
                        value = test.GetInt32();
                        break;
                    case MHAlgoParameters.FunctionValueMinimumEnhancementThreshold:
                        value = test.GetDouble();
                        break;
                    case MHAlgoParameters.ProblemDimension:
                        value = test.GetInt32();
                        break;
                    case MHAlgoParameters.OptimizationType:
                        value = (OptimizationProblemType)test.GetInt32();
                        break;
                    case MHAlgoParameters.PopulationInitilization:
                        value = (PopulationInitilizationType)test.GetInt32();
                        break;
                    case MHAlgoParameters.ABC_LimitValue:
                        value = test.GetInt32();
                        break;
                    case MHAlgoParameters.PSO_C_Constant:
                        value = test.GetDouble();
                        break;
                    case MHAlgoParameters.PSO_X_Constant:
                        value = test.GetDouble();
                        break;
                    case MHAlgoParameters.FunctionValueSigmaTolerance:
                        value = test.GetDouble();
                        break;
                    case MHAlgoParameters.ShiftObjectiveFunctionOptimumValueToZero:
                        value = test.GetBoolean();
                        break;
                    case MHAlgoParameters.StopOptimizationWhenOptimumIsReached:
                        value = test.GetBoolean();
                        break;
                    case MHAlgoParameters.MABC_ModificationRate:
                        value = test.GetDouble();
                        break;
                    case MHAlgoParameters.MABC_UseScalingFactor:
                        value = test.GetBoolean();
                        break;
                    case MHAlgoParameters.MABC_LimitValue:
                        value = test.GetDouble();
                        break;
                    case MHAlgoParameters.AEEABC_TuneNumberOfDimensionUsingGBest:
                        value = test.GetBoolean();
                        break;
                    case MHAlgoParameters.AEEABC_NumberOfIterationsToTuneParameters:
                        value = test.GetInt32();
                        break;
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

        public MHAlgoParameters Name { get; set; }

        [JsonNumberHandling(JsonNumberHandling.Strict)]
        public object Value { get; set; }

        public bool IsEssentialInfo { get; set; } = false;


    }
}
