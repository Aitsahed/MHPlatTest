
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
    /// The Zakharov function is implemented as described in Hedar, A. & Fukushima, M. 2004. Heuristic pattern search and its hybridization with simulated annealing for nonlinear global optimization. Optimization Methods and Software 19, pp. 291-308, Taylor & Francis.
    /// </summary>
    internal class Zakharov : IBenchmark
    {
        public Zakharov()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Zakharov";
        public string Description { get; set; } = "The Zakharov function is implemented as described in Hedar, A. & Fukushima, M. 2004. Heuristic pattern search and its hybridization with simulated annealing for nonlinear global optimization. Optimization Methods and Software 19, pp. 291-308, Taylor & Francis.";
        public int IDNumero { get; set; } = 12;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -5 };
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
            double shiftDataValue = -1;
            for (int iShiftData = 0; iShiftData < nbrProblemDimension; iShiftData++)
            {
                functionParameter1[iShiftData] = shiftDataValue + functionParameter[iShiftData];
                if (functionParameter1[iShiftData] > SearchSpaceMaxValue[0])
                    functionParameter1[iShiftData] = SearchSpaceMaxValue[0];
            }


            double s1 = 0;
            double s2 = 0;

            //for (int i = 0; i < nbrProblemDimension; i++)
            //{
            //    functionParameter[i] = 0;
            //}

            for (int i = 0; i < nbrProblemDimension; i++)
            {
                s1 += functionParameter1[i] * functionParameter1[i];
                s2 += 0.5 * i * functionParameter1[i];
            }

            double result = s1 + (s2 * s2) + (s2 * s2 * s2 * s2);


            if (ShiftOptimumToZero == true)
            {
                return s1 + (s2 * s2) + (s2 * s2 * s2 * s2) - OptimalFunctionValue(functionParameter1.Length);
            }
            else
            {
                return result;
            }
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
