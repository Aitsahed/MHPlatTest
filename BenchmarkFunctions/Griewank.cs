
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
    /// The Griewank function 
    /// A Note on the Griewank Test Function
    /// February 2003Journal of Global Optimization 25(2):169-174
    /// DOI: 10.1023/A:1021956306041
    /// </summary>
    internal class Griewank : IBenchmark
    {
        public Griewank()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Griewank";
        public string Description { get; set; } = "The Griewank function as . It has a single global minimum at the origin with value 0.";
        public int IDNumero { get; set; } = 4;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -600 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 600 };
        public short MinProblemDimension { get; set; } = 1;
        public short MaxProblemDimension { get; set; } = short.MaxValue;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {          // functionParameter.SetDataElementsToSigleValue(1);

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


            double tempResult = 0;
            double tempResultCos = 1;
            for (int i = 0; i < nbrProblemDimension; i++)
            {
                tempResult += functionParameter1[i] * functionParameter1[i];
                tempResultCos *= Math.Cos(functionParameter1[i] / Math.Sqrt((double)(i + 1)));
            }

            tempResult = tempResult / 4000;


            //tempResultCos = tempResultCos / nbrProblemDimension;


            tempResult = tempResult - tempResultCos + 1;

            //Shift the optimum if it is required
            if (ShiftOptimumToZero == true)
            {
                return tempResult - OptimalFunctionValue(functionParameter1.Length);
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
