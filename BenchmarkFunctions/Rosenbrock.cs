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
    /// The Rosenbrock function features a flat valley in which the global optimum is located.
    /// It is implemented as generalized Rosenbrock function as for example given in Shang, Y.-W. and Qiu, Y.-H. 2006. A Note on the Extended Rosenbrock Function. Evolutionary Computation 14, pp. 119-126, MIT Press.
    /// </summary>
    internal class Rosenbrock : IBenchmark
    {
        public Rosenbrock()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Rosenbrock";
        public string Description { get; set; } = "The Rosenbrock function features a flat valley in which the global optimum is located It is implemented as generalized Rosenbrock function as for example given in Shang, Y.-W. and Qiu, Y.-H. 2006. A Note on the Extended Rosenbrock Function. Evolutionary Computation 14, pp. 119-126, MIT Press.";
        public int IDNumero { get; set; } = 8;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -2.048 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 2.048 };
        public short MinProblemDimension { get; set; } = 1;
        public short MaxProblemDimension { get; set; } = short.MaxValue;
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
            double shiftDataValue = -0;
            for (int iShiftData = 0; iShiftData < nbrProblemDimension; iShiftData++)
            {
                functionParameter1[iShiftData] = shiftDataValue + functionParameter[iShiftData];
                if (functionParameter1[iShiftData] > SearchSpaceMaxValue[0])
                    functionParameter1[iShiftData] = SearchSpaceMaxValue[0];
            }


            double[] y = new double[nbrProblemDimension];
            for (int i = 0; i < nbrProblemDimension; i++)
            {
                y[i] = functionParameter1[i];
            }

            double result = 0;
            for (int i = 0; i < nbrProblemDimension - 1; i++)
            {
                result += 100 * (y[i] * y[i] - y[i + 1]) * (y[i] * y[i] - y[i + 1]);
                result += (y[i] - 1) * (y[i] - 1);
            }

            //Shift the optimum if it is required
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
                tempResult[i] = 1;
            }

            return new List<double[]> { tempResult };

        }

    }
}
