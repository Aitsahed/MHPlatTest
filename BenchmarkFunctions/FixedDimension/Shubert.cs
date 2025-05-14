
using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using MHPlatTest.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHPlatTest.BenchmarkFunctions.FixedDimension
{
    /// <summary>
    ///The Shubert function has several local minima and many global minima.
    /// https://www.sfu.ca/~ssurjano/shubert.html
    /// http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page1882.htm
    /// </summary>


    internal class Shubert : IBenchmark
    {
        public Shubert()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Shubert Function";
        public string Description { get; set; } = "The Shubert function has several local minima and many global minima";
        public int IDNumero { get; set; } = 1;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -10 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 10 };
        public short MinProblemDimension { get; set; } = 2;
        public short MaxProblemDimension { get; set; } = 2;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {
            //functionParameter.SetDataElementsToSigleValue(0);
            //Increase the current number of function evaluation by 1
            currentNumberofunctionEvaluation++;

            int nbrProblemDimension = functionParameter.Length;
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }

            double result = 0;
            double z1, z2;

            double sum1 = 0;
            double sum2 = 0;

            // // { new double[] { -1 * Math.PI, 12.275 }, new double[] { Math.PI, 2.275 }, new double[] { 9.42478, 2.475 };

            z1 = functionParameter[0];
            z2 = functionParameter[1];

            //z1 =  Math.PI;
            //z2 = 2.275;

            for (int i = 1; i < 6; i++)
            {
                sum1 += i * Math.Cos((i + 1) * z1 + i);
                sum2 += i * Math.Cos((i + 1) * z2 + i);
            }

            result = sum1 * sum2;

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

            double tempResult = -186.7309;

            return tempResult;
        }


        public List<double[]> OptimalPoint(int nbrProblemDimension)
        {
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }


            return new List<double[]>();// { new double[] { -1 * Math.PI, 12.275 }, new double[] { Math.PI, 2.275 }, new double[] { 9.42478, 2.475 } };
        }

    }
}
