using MHPlatTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using MHPlatTest.Utility;
using MHPlatTest.Divers;

namespace MHPlatTest.BenchmarkFunctions
{
    /// <summary>
    /// Weierstrass Function
    /// the problems are defined in the paper entitled
    /// Problem Definitions and Evaluation Criteria for the CEC 2021     Special Session and Competition on Single Objective Bound Constrained Numerical Optimization
    /// DOI: 10.13140/RG.2.2.36130.66245
    /// </summary>
    internal class Weierstrass : IBenchmark
    {
        public Weierstrass()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Weierstrass";
        public string Description { get; set; } = "The Weierstrass Function as described in CEC2021 Report. It has a single global minimum at the origin with value 0.";
        public int IDNumero { get; set; } = 1;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -100 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 100 };
        public short MinProblemDimension { get; set; } = 1;
        public short MaxProblemDimension { get; set; } = short.MaxValue;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {
           // functionParameter.SetDataElementsToSigleValue(1);
            currentNumberofunctionEvaluation++;

            double nbrProblemDimension = (double)functionParameter.Length;

            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = (double)MinProblemDimension;
            }

            double[] functionParameter1 = new double[(int)nbrProblemDimension];
            double shiftDataValue = -1;
            for (int iShiftData = 0; iShiftData < nbrProblemDimension; iShiftData++)
            {
                functionParameter1[iShiftData] = shiftDataValue + functionParameter[iShiftData];
                if (functionParameter1[iShiftData] > SearchSpaceMaxValue[0])
                    functionParameter1[iShiftData] = SearchSpaceMaxValue[0];
            }


            double result = 0;
            double a = 0.5;
            double b = 3;
            double kMax = 20;
            double sumCos1 = 0;
            double sumCos2 = 0;
            double tempValue1;

            for (int i = 0; i < nbrProblemDimension; i++)
            {
                for (int k = 0; k <= kMax; k++)
                {
                    tempValue1 = 2 * Math.PI * Math.Pow(b, k) * (functionParameter1[i] + 0.5);
                    sumCos1 += Math.Pow(a, k) * Math.Cos(tempValue1);
                }
            }

            for (int k = 0; k <= kMax; k++)
            {
                tempValue1 = 2 * Math.PI * Math.Pow(b, k) * 0.5;
                sumCos2 += Math.Pow(a, k) * Math.Cos(tempValue1);
            }



            result = sumCos1 - nbrProblemDimension * sumCos2;

            if (result == 0)
            {
                int i = 222;
            }

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
                tempResult[i] = 0;   //Used to be -100 investigate why
            }

            return new List<double[]> { tempResult };
        }

    }
}
