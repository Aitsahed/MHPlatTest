using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHPlatTest.BenchmarkFunctions.FixedDimension
{
    /// <summary>
    /// The Beale function is defined for 2 dimensions with an optimum of 0 at (3, 0.5).
    /// It is implemented as described in Moré, J.J., Garbow, B., and Hillstrom, K. 1981. Testing unconstrained optimization software. ACM Transactions on Mathematical Software 7, pp. 136-140, ACM.
    /// https://towardsdatascience.com/optimization-eye-pleasure-78-benchmark-test-functions-for-single-objective-optimization-92e7ed1d1f12
    /// </summary>
    internal class Beale : IBenchmark
    {
        public Beale()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Beale Function";
        public string Description { get; set; } = " /// The Beale function is defined for 2 dimensions with an optimum of 0 at (3, 0.5). It is implemented as described in Moré, J.J., Garbow, B., and Hillstrom, K. 1981. Testing unconstrained optimization software. ACM Transactions on Mathematical Software 7, pp. 136-140, ACM.";
        public int IDNumero { get; set; } = 2;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -4.5 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 4.5 };
        public short MinProblemDimension { get; set; } = 2;
        public short MaxProblemDimension { get; set; } = 2;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {
            //Increase the current number of function evaluation by 1
            currentNumberofunctionEvaluation++;

            int nbrProblemDimension = functionParameter.Length;
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }

            double x1 = functionParameter[0];
            double x2 = functionParameter[1];

            double f1 = 1.5 - x1 * (1 - x2);
            double f2 = 2.25 - x1 * (1 - x2 * x2);
            double f3 = 2.625 - x1 * (1 - x2 * x2 * x2);

            //Shift the optimum if it is required
            if (ShiftOptimumToZero == true)
            {
                return (f1 * f1) + (f2 * f2) + (f3 * f3) - OptimalFunctionValue(functionParameter.Length);
            }

            return (f1 * f1) + (f2 * f2) + (f3 * f3);

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
            double[] tempResult = new double[2] { 3, 0.5 };

            return new List<double[]> { tempResult };

        }
    }
}
