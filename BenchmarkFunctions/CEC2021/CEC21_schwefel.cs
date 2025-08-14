using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using MHPlatTest.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHPlatTest.BenchmarkFunctions.CEC2021
{
    /// <summary>
    /// WORKS ONLY WITH D=10 OR D=20 (MANUALLY MODIFY MinProblemDimension and MaxProblemDimension to use appropriate dimension)
    /// the problems are defined in the paper entitled
    /// Problem Definitions and Evaluation Criteria for the CEC 2021     Special Session and Competition on Single Objective Bound Constrained Numerical Optimization
    /// DOI: 10.13140/RG.2.2.36130.66245
    /// </summary>
    internal class CEC21_schwefel : IBenchmark
    {
        public CEC21_schwefel()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Schwefel Function CEC21";
        public string Description { get; set; } = "The Schwefel ";
        public int IDNumero { get; set; } = 9;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -100 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 100 };
        public short MinProblemDimension { get; set; } =  1;
        public short MaxProblemDimension { get; set; } =  short.MaxValue;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {
            //functionParameter.SetDataElementsToSigleValue(1);
            //Increase the current number of function evaluation by 1
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


            double[] z = new double[nbrProblemDimension];
            double f = 0;
            double result;
            double tmp;

            for (int i = 0; i < nbrProblemDimension; i++)
            {
                z[i] = functionParameter1[i] * 10;
            }

            for (int i = 0; i < nbrProblemDimension; i++)
            {
                z[i] += 4.209687462275036e+002;
                if (z[i] > 500.0)
                {
                    f -= (500.0 - (z[i] % 500)) * Math.Sin(Math.Pow(500.0 - (z[i] % 500.0), 0.5));
                    tmp = (z[i] - 500.0) / 100.0;
                    f += tmp * tmp / nbrProblemDimension;
                }
                else if (z[i] < -500)
                {
                    f -= (-500.0 + (Math.Abs(z[i]) % 500.0)) * Math.Sin(Math.Pow(500.0 - (Math.Abs(z[i]) % 500.0), 0.5));
                    tmp = (z[i] + 500.0) / 100;
                    f += tmp * tmp / nbrProblemDimension;
                }
                else
                    f -= z[i] * Math.Sin(Math.Pow(Math.Abs(z[i]), 0.5));
            }
            f += 4.189828872724338e+002 * nbrProblemDimension;

            result = f;

            //Shift the optimum if it is required
            if (ShiftOptimumToZero == true)
            {
                return result - OptimalFunctionValue(functionParameter1.Length);
            }

            return (result);
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
                tempResult[i] = 0;// 420.9687 / 5;
            }

            return new List<double[]> { tempResult };

        }

    }
}
