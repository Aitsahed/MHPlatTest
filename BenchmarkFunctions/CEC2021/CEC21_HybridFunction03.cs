
using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using MHPlatTest.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHPlatTest.BenchmarkFunctions.CEC2021
{
    /// <summary>
    /// WORKS ONLY WITH D=10 OR D=20 (MANUALLY MODIFY MinProblemDimension and MaxProblemDimension to use appropriate dimension)
    /// the problems are defined in the paper entitled
    /// Problem Definitions and Evaluation Criteria for the CEC 2021     Special Session and Competition on Single Objective Bound Constrained Numerical Optimization
    /// DOI: 10.13140/RG.2.2.36130.66245
    /// </summary>
    internal class CEC21_HybridFunction03 : IBenchmark
    {
        public CEC21_HybridFunction03()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Hybrid Function 3 CEC21";
        public string Description { get; set; } = "Hybrid Function 3  with 5 sub functions";
        public int IDNumero { get; set; } = 9;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -100 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 100 };
        public short MinProblemDimension { get; set; } = 10;
        public short MaxProblemDimension { get; set; } = 10;
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



            int i, tmp, cf_num = 5;
            double[] fit = new double[5];
            int[] startingSubProbDimension = new int[5];
            int[] subProbDimension = new int[5];
            double[] Gp = new double[5] { 0.1, 0.2, 0.2, 0.2, 0.3 };

            double[] y = new double[nbrProblemDimension];
            double[] z = new double[nbrProblemDimension];
            int[] Shuffle10 = new int[10] { 7, 8, 5, 10, 3, 6, 9, 4, 2, 1 };
            int[] Shuffle20 = new int[20] { 8, 7, 4, 15, 2, 17, 5, 11, 3, 1, 18, 6, 10, 9, 13, 19, 12, 16, 20, 14 };



            tmp = 0;
            for (i = 1; i < cf_num; i++)
            {
                subProbDimension[i] = (int)Math.Ceiling(Gp[i] * (double)nbrProblemDimension);
                tmp += subProbDimension[i];
            }
            //G_nx[cf_num-1]=nx-tmp;
            subProbDimension[0] = nbrProblemDimension - tmp;
            startingSubProbDimension[0] = 0;
            for (i = 1; i < cf_num; i++)
            {
                startingSubProbDimension[i] = startingSubProbDimension[i - 1] + subProbDimension[i - 1];
            }



            for (i = 0; i < nbrProblemDimension; i++)//shrink to the orginal search range
            {
                z[i] = functionParameter1[i];
            }

            for (i = 0; i < nbrProblemDimension; i++)
            {
                if (nbrProblemDimension == 10)
                {
                    y[i] = z[Shuffle10[i] - 1];
                }
                if (nbrProblemDimension == 20)
                {
                    y[i] = z[Shuffle20[i] - 1];
                }
            }


            ExpandedScaffers ExpandedScaffers_func = new ExpandedScaffers();
            HGBat HGBat_func = new HGBat();
            Rosenbrock Rosenbrock_func = new Rosenbrock();
            CEC21_schwefel CEC21_schwefel_func = new CEC21_schwefel();
            HighConditionedElliptic HighConditionedElliptic_func = new HighConditionedElliptic();
            double[] ExpandedScaffers_funcParameter = functionParameter1.Where((x, Index) => Index >= startingSubProbDimension[0] && Index < startingSubProbDimension[1]).ToArray();
            double[] HGBat_funcParameter = functionParameter1.Where((x, Index) => Index >= startingSubProbDimension[1] && Index < startingSubProbDimension[2]).ToArray();
            double[] Rosenbrock_funcParameter = functionParameter1.Where((x, Index) => Index >= startingSubProbDimension[2] && Index < startingSubProbDimension[3]).ToArray();
            double[] CEC21_schwefel_funcParameter = functionParameter1.Where((x, Index) => Index >= startingSubProbDimension[3] && Index < startingSubProbDimension[4]).ToArray();
            double[] HighConditionedElliptic_funcParameter = functionParameter1.Where((x, Index) => Index >= startingSubProbDimension[4]).ToArray();


            int tempValue = 0;
            double result = 0;


            for (int indexRastr = 0; indexRastr < ExpandedScaffers_funcParameter.Length; indexRastr++)
            {
                ExpandedScaffers_funcParameter[indexRastr] += 1.0;
            }
            result += ExpandedScaffers_func.ComputeValue(ExpandedScaffers_funcParameter, ref tempValue, ShiftOptimumToZero);

            //Shrink input value to accomodate boundary
            for (int indexRastr = 0; indexRastr < HGBat_funcParameter.Length; indexRastr++)
            {
                HGBat_funcParameter[indexRastr] = HGBat_funcParameter[indexRastr] * (5.0 / 100.0) - 1.0;
            }

            result += HGBat_func.ComputeValue(HGBat_funcParameter, ref tempValue, ShiftOptimumToZero);

            //Shrink input value to accomodate boundary
            for (int indexRastr = 0; indexRastr < Rosenbrock_funcParameter.Length; indexRastr++)
            {
                Rosenbrock_funcParameter[indexRastr] = (Rosenbrock_funcParameter[indexRastr] * 2.048 / 100.0) + 1;//Shift not necessary
            }
            result += Rosenbrock_func.ComputeValue(Rosenbrock_funcParameter, ref tempValue, ShiftOptimumToZero);



            for (int indexRastr = 0; indexRastr < CEC21_schwefel_funcParameter.Length; indexRastr++)
            {
                CEC21_schwefel_funcParameter[indexRastr] += 1;
            }
            result += CEC21_schwefel_func.ComputeValue(CEC21_schwefel_funcParameter, ref tempValue, ShiftOptimumToZero);


            for (int indexRastr = 0; indexRastr < HighConditionedElliptic_funcParameter.Length; indexRastr++)
            {
                HighConditionedElliptic_funcParameter[indexRastr] += 1;
            }
            result += HighConditionedElliptic_func.ComputeValue(HighConditionedElliptic_funcParameter, ref tempValue, ShiftOptimumToZero);



            //Shift the optimum if it is required
            if (ShiftOptimumToZero == true)
            {
                return result - OptimalFunctionValue(functionParameter1.Length);
            }

            return (result);
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

