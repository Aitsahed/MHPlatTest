
using MHPlatTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using MHPlatTest.Utility;
using MHPlatTest.Divers;

namespace MHPlatTest.BenchmarkFunctions
{
    /// <summary>
    /// the problems are defined in the paper entitled
    /// Problem Definitions and Evaluation Criteria for the CEC 2021     Special Session and Competition on Single Objective Bound Constrained Numerical Optimization
    /// DOI: 10.13140/RG.2.2.36130.66245
    /// </summary>
    internal class ExpandedScaffers : IBenchmark
    {
        public ExpandedScaffers()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Expanded";
        public string Description { get; set; } = "The Expanded Scaffers function as described in CEC2021 Report. It has a single global minimum at the origin with value 0.";
        public int IDNumero { get; set; } = 1;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -100 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 100 };
        public short MinProblemDimension { get; set; } = 1;
        public short MaxProblemDimension { get; set; } = short.MaxValue;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {           //functionParameter.SetDataElementsToSigleValue(1);

            currentNumberofunctionEvaluation++;

            int nbrProblemDimension = functionParameter.Length;

            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }



            double[] functionParameter1 = new double[(int)nbrProblemDimension];
            double shiftDataValue = -1;
            for (int iShiftData = 0; iShiftData < nbrProblemDimension; iShiftData++)
            {
                functionParameter1[iShiftData] = shiftDataValue + functionParameter[iShiftData];
                if (functionParameter1[iShiftData] > SearchSpaceMaxValue[0])
                    functionParameter1[iShiftData] = SearchSpaceMaxValue[0];
            }


            double result;
            double val = 0;
            int i;
            double temp1, temp2;
            double[] z = new double[nbrProblemDimension];

            for (int i1 = 1; i1 < nbrProblemDimension; i1++)
            {
                z[i1] = functionParameter1[i1];
            }


            result = 0.0;
            for (i = 0; i < nbrProblemDimension - 1; i++)
            {
                temp1 = Math.Sin(Math.Sqrt(z[i] * z[i] + z[i + 1] * z[i + 1]));
                temp1 = temp1 * temp1;
                temp2 = 1.0 + 0.001 * (z[i] * z[i] + z[i + 1] * z[i + 1]);
                result += 0.5 + (temp1 - 0.5) / (temp2 * temp2);
            }
            temp1 = Math.Sin(Math.Sqrt(z[nbrProblemDimension - 1] * z[nbrProblemDimension - 1] + z[0] * z[0]));
            temp1 = temp1 * temp1;
            temp2 = 1.0 + 0.001 * (z[nbrProblemDimension - 1] * z[nbrProblemDimension - 1] + z[0] * z[0]);
            result += 0.5 + (temp1 - 0.5) / (temp2 * temp2);









            if (ShiftOptimumToZero == true)
            {
                return result - OptimalFunctionValue(functionParameter1.Length);
            }

            return result;

        }

        public double OptimalFunctionValue(int nbrProblemDimension)
        {
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }

            double tempResult = 0;

            return tempResult;
        }


        public List<double[]> OptimalPoint(int nbrProblemDimension)
        {
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }

            double[] tempResult = new double[nbrProblemDimension];
            for (int i = 0; i < nbrProblemDimension; i++)
            {
                tempResult[i] = 0;
            }

            return new List<double[]> { tempResult };
        }

    }
}
