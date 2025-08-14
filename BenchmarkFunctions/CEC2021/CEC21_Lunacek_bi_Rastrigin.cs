using MHPlatTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using MHPlatTest.Utility;
using MHPlatTest.Divers;

namespace MHPlatTest.BenchmarkFunctions.CEC2021
{
    /// <summary>
    /// WORKS ONLY WITH D=10 OR D=20 (MANUALLY MODIFY MinProblemDimension and MaxProblemDimension to use appropriate dimension)
    /// the problems are defined in the paper entitled
    /// Problem Definitions and Evaluation Criteria for the CEC 2021     Special Session and Competition on Single Objective Bound Constrained Numerical Optimization
    /// DOI: 10.13140/RG.2.2.36130.66245
    /// </summary>
    internal class CEC21_Lunacek_bi_Rastrigin : IBenchmark
    {
        public CEC21_Lunacek_bi_Rastrigin()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Lunacek_bi_Rastrigin Function CEC21";
        public string Description { get; set; } = "The Lunacek_bi_Rastrigin Function";
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


            double result = 0;
            int i;
            double mu0 = 2.5, d = 1.0, s, mu1, tmp, tmp1, tmp2;
            double[] tmpx = new double[nbrProblemDimension];
            double[] y = new double[nbrProblemDimension];
            double[] z = new double[nbrProblemDimension];



            s = 1.0 - 1.0 / (2.0 * Math.Pow(nbrProblemDimension + 20.0, 0.5) - 8.2);
            mu1 = -Math.Pow((mu0 * mu0 - d) / s, 0.5);

            for (i = 0; i < nbrProblemDimension; i++)//shrink to the orginal search range
            {
                y[i] = functionParameter1[i] / 10;
            }


            for (i = 0; i < nbrProblemDimension; i++)
            {
                tmpx[i] = 2 * y[i];
            }
            for (i = 0; i < nbrProblemDimension; i++)
            {
                z[i] = tmpx[i];
                tmpx[i] += mu0;
            }
            tmp1 = 0.0; tmp2 = 0.0;
            for (i = 0; i < nbrProblemDimension; i++)
            {
                tmp = tmpx[i] - mu0;
                tmp1 += tmp * tmp;
                tmp = tmpx[i] - mu1;
                tmp2 += tmp * tmp;
            }
            tmp2 *= s;
            tmp2 += d * nbrProblemDimension;
            tmp = 0.0;


            for (i = 0; i < nbrProblemDimension; i++)
            {
                tmp += Math.Cos(2.0 * Math.PI * z[i]);
            }
            if (tmp1 < tmp2)
                result = tmp1;
            else
                result = tmp2;
            result += 10.0 * (nbrProblemDimension - tmp);




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
                tempResult[i] = 0;// 2.5;  //-1 ou 0.5*mu0
            }

            return new List<double[]> { tempResult };
        }

    }
}

