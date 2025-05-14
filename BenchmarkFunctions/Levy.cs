
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
    /// The Levy function is implemented as described on http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page2056.htm, last accessed April 12th, 2010.
    /// </summary>
    internal class Levy : IBenchmark
    {
        public Levy()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Levy";
        public string Description { get; set; } = " The Levy function is implemented as described on http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page2056.htm, last accessed April 12th, 2010.";
        public int IDNumero { get; set; } = 5;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -10 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 10 };
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
            double shiftDataValue = 0;
            for (int iShiftData = 0; iShiftData < nbrProblemDimension; iShiftData++)
            {
                functionParameter1[iShiftData] = shiftDataValue + functionParameter[iShiftData];
                if (functionParameter1[iShiftData] > SearchSpaceMaxValue[0])
                    functionParameter1[iShiftData] = SearchSpaceMaxValue[0];
            }


            double[] z = new double[nbrProblemDimension];
            double s;

            for (int i = 0; i < nbrProblemDimension; i++)
            {
                z[i] = 1 + (functionParameter1[i] - 1) / 4;
            }

            s = Math.Sin(Math.PI * z[0]);
            if (Math.Abs(s) < 1e-15) s = 0; // Math.Sin(Math.PI) == 0.00000000000000012246063538223773
            s *= s;

            for (int i = 0; i < nbrProblemDimension - 1; i++)
            {
                s += (z[i] - 1) * (z[i] - 1) * (1 + 10 * Math.Pow(Math.Sin(Math.PI * z[i] + 1), 2));
            }

            //Shift the optimum if it is required
            if (ShiftOptimumToZero == true)
            {
                return s + Math.Pow(z[nbrProblemDimension - 1] - 1, 2) * (1 + Math.Pow(Math.Sin(2 * Math.PI * z[nbrProblemDimension - 1]), 2)) - OptimalFunctionValue(functionParameter1.Length);
            }

            return s + Math.Pow(z[nbrProblemDimension - 1] - 1, 2) * (1 + Math.Pow(Math.Sin(2 * Math.PI * z[nbrProblemDimension - 1]), 2));

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
