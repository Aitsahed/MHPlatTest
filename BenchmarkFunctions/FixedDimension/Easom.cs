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
    ///The Easom function has several local minima.It is unimodal, and the global minimum has a small area relative to the search space.
    /// https://www.sfu.ca/~ssurjano/easom.html
    /// Global Optimization Test Problems. Retrieved June 2013, from http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO.htm.
    /// the implemented function optimum has been shifted to zero
    /// https://towardsdatascience.com/optimization-eye-pleasure-78-benchmark-test-functions-for-single-objective-optimization-92e7ed1d1f12
    /// </summary>


    internal class Easom : IBenchmark
    {
        public Easom()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Easom Function";
        public string Description { get; set; } = "The Easom function has several local minima.It is unimodal, and the global minimum has a small area relative to the search space. https://www.sfu.ca/~ssurjano/easom.html Global Optimization Test Problems. Retrieved June 2013, from http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO.htm.";
        public int IDNumero { get; set; } = 1;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -100 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 100 };
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

            double result = 0;

            double fact1 = -1 * Math.Cos(functionParameter[0]) * Math.Cos(functionParameter[1]);
            double fact2 = Math.Exp((-1 * (functionParameter[0] - Math.PI) * (functionParameter[0] - Math.PI)) - ((functionParameter[1] - Math.PI) * (functionParameter[1] - Math.PI)));

            result = fact1 * fact2;
           // result += 1;

            //Shift the optimum if it is required
            if (ShiftOptimumToZero == true)
            {
                return result - OptimalFunctionValue(functionParameter.Length);
            }

            return result;

        }

        public double OptimalFunctionValue(int nbrProblemDimension)
        {
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }
            //Shift the optimum if it is required
            double tempResult =  -1;

            return tempResult;
        }


        public List<double[]> OptimalPoint(int nbrProblemDimension)
        {
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }

            return new List<double[]>() { new double[] { Math.PI, Math.PI } };
        }

    }
}
