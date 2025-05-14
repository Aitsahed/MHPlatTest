
using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using MHPlatTest.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHPlatTest.BenchmarkFunctions
{
    /// <summary>
    /// The Exponential function is implemented as described in https://towardsdatascience.com/optimization-eye-pleasure-78-benchmark-test-functions-for-single-objective-optimization-92e7ed1d1f12
    /// and https://al-roomi.org/benchmarks/unconstrained/n-dimensions/168-exponential-function
    /// </summary>
    internal class Exponential : IBenchmark
    {
        public Exponential()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Exponential";
        public string Description { get; set; } = "The Exponential function is implemented as described in https://towardsdatascience.com/optimization-eye-pleasure-78-benchmark-test-functions-for-single-objective-optimization-92e7ed1d1f12";
        public int IDNumero { get; set; } = 12;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -1 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 1 };
        public short MinProblemDimension { get; set; } = 1;
        public short MaxProblemDimension { get; set; } = short.MaxValue;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {           // functionParameter.SetDataElementsToSigleValue(0.1);

            //Increase the current number of function evaluation by 1
            currentNumberofunctionEvaluation++;

            int nbrProblemDimension = functionParameter.Length;
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }



            double[] functionParameter1 = new double[(int)nbrProblemDimension];
            double shiftDataValue = -0.1;
            for (int iShiftData = 0; iShiftData < nbrProblemDimension; iShiftData++)
            {
                functionParameter1[iShiftData] = shiftDataValue + functionParameter[iShiftData];
                if (functionParameter1[iShiftData] > SearchSpaceMaxValue[0])
                    functionParameter1[iShiftData] = SearchSpaceMaxValue[0];
            }


            double sumElementSquare = 0;

            //for (int i = 0; i < nbrProblemDimension; i++)
            //{
            //    functionParameter[i] = 0;
            //}

            for (int i = 0; i < nbrProblemDimension; i++)
            {
                sumElementSquare += functionParameter1[i] * functionParameter1[i];
            }

            //decimal result1 = -Math.Exp(-0.5M * (decimal) sumElementSquare) + 1M; //T shift min to zero
            double result = -Math.Exp(-0.5 * sumElementSquare) + 1; //T shift min to zero



            if (ShiftOptimumToZero == true)
            {
                return result - OptimalFunctionValue(functionParameter1.Length);
            }
            else
            {
                return result;
            }
        }

        public double OptimalFunctionValue(int nbrProblemDimension)
        {
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }

            double tempResult = 0;// -1;

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
