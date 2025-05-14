
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
    /// The Booth function is implemented as described on http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page816.htm, last accessed April 12th, 2010.
    /// </summary>
    internal class Booth : IBenchmark
    {
        public Booth()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Booth Function";
        public string Description { get; set; } = "The Booth function is implemented as described on http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page816.htm, last accessed April 12th, 2010.";
        public int IDNumero { get; set; } = 3;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -10 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 10 };
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

            double tempResult = 0;
            double x1 = functionParameter[0];
            double x2 = functionParameter[1];

            tempResult = Math.Pow(x1 + 2 * x2 - 7, 2) + Math.Pow(2 * x1 + x2 - 5, 2);

            //Shift the optimum if it is required
            if (ShiftOptimumToZero == true)
            {
                return tempResult - OptimalFunctionValue(functionParameter.Length);
            }

            return tempResult;

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
            double[] tempResult = new double[2] { 1, 3 };

            return new List<double[]> { tempResult };

        }

    }
}
