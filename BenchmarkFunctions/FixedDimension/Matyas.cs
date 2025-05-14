
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
    /// The Matyas function is implemented as described on http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page2213.htm, last accessed April 12th, 2010.
    /// https://towardsdatascience.com/optimization-eye-pleasure-78-benchmark-test-functions-for-single-objective-optimization-92e7ed1d1f12
    /// </summary>
    internal class Matyas : IBenchmark
    {
        public Matyas()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Matyas Function";
        public string Description { get; set; } = "The Matyas function is implemented as described on http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page2213.htm, last accessed April 12th, 2010.";
        public int IDNumero { get; set; } = 6;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -10 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 10 };
        public short MinProblemDimension { get; set; } = 2;
        public short MaxProblemDimension { get; set; } = 2;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {
            //functionParameter.SetDataElementsToSigleValue(1);

            //Increase the current number of function evaluation by 1
            currentNumberofunctionEvaluation++;

            int nbrProblemDimension = functionParameter.Length;

            double[] functionParameter1 = new double[(int)nbrProblemDimension];
            double shiftDataValue = -1;
            for (int iShiftData = 0; iShiftData < nbrProblemDimension; iShiftData++)
            {
                functionParameter1[iShiftData] = shiftDataValue + functionParameter[iShiftData];
                if (functionParameter1[iShiftData] > SearchSpaceMaxValue[0])
                    functionParameter1[iShiftData] = SearchSpaceMaxValue[0];
            }


            //Shift the optimum if it is required
            if (ShiftOptimumToZero == true)
            {
                return 0.26 * (functionParameter1[0] * functionParameter1[0] + functionParameter1[1] * functionParameter1[1]) - 0.48 * functionParameter1[0] * functionParameter1[1] - OptimalFunctionValue(functionParameter1.Length);
            }

            return 0.26 * (functionParameter1[0] * functionParameter1[0] + functionParameter1[1] * functionParameter1[1]) - 0.48 * functionParameter1[0] * functionParameter1[1];
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
            double[] tempResult = new double[2] { 0, 0 };

            return new List<double[]> { tempResult };

        }
    }
}
