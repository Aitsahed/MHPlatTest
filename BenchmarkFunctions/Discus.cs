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
    /// Discus Function
    /// the problems are defined in the paper entitled
    /// Problem Definitions and Evaluation Criteria for the CEC 2021     Special Session and Competition on Single Objective Bound Constrained Numerical Optimization
    /// DOI: 10.13140/RG.2.2.36130.66245
    /// </summary>
    internal class Discus : IBenchmark
    {
        public Discus()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Discus";
        public string Description { get; set; } = "The Discus Function as described in CEC2021 Report. It has a single global minimum at the origin with value 0.";
        public int IDNumero { get; set; } = 1;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -100 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 100 };
        public short MinProblemDimension { get; set; } = 1;
        public short MaxProblemDimension { get; set; } = short.MaxValue;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {          
            //functionParameter.SetDataElementsToSigleValue(1);

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


            double sumSquareElement = 0;
            double result = 0;
            double resultTemp = 0;


            for (int i = 1; i < nbrProblemDimension; i++)
            {
                sumSquareElement += functionParameter1[i] * functionParameter1[i];
            }



            result = 1000000 * functionParameter1[0] * functionParameter1[0] + sumSquareElement;



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


        /// <summary>
        /// </summary>
        /// <param name="nbrProblemDimension"></param>
        /// <returns></returns>
        public List<double[]> OptimalPoint(int nbrProblemDimension)
        {//THE OPTIMAL POINT OF THE FUNCTION ISNOT KNOWN
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

