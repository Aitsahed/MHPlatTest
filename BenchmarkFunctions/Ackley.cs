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
    /// The Ackley function as described in Eiben, A.E. and Smith, J.E. 2003. Introduction to Evolutionary Computation. Natural Computing Series, Springer-Verlag Berlin Heidelberg
    /// is highly multimodal. It has a single global minimum at the origin with value 0.
    /// </summary>
    internal class Ackley : IBenchmark
    {
        public Ackley()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }


        public string Name { get; set; } = "Ackley";
        public string Description { get; set; } = "The Ackley function as described in Eiben, A.E. and Smith, J.E. 2003. Introduction to Evolutionary Computation. Natural Computing Series, Springer-Verlag Berlin Heidelberg is highly multimodal. It has a single global minimum at the origin with value 0.";
        public int IDNumero { get; set; } = 1;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { -32.768 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 32.768 };
        public short MinProblemDimension { get; set; } = 1;
        public short MaxProblemDimension { get; set; } = short.MaxValue;
        public int ParentInstanceID { get; set; }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {

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


            double result;
            double val;

            double tempResult = 0;
            double tempResultCos = 0;
            for (int i = 0; i < nbrProblemDimension; i++)
            {
                tempResult += functionParameter1[i] * functionParameter1[i];
                tempResultCos += Math.Cos(2 * Math.PI * functionParameter1[i]);
            }

            tempResult = tempResult / nbrProblemDimension;
            tempResult = -0.2 * Math.Sqrt(tempResult);
            tempResult = 20 - 20 * Math.Exp(tempResult);

            tempResultCos = tempResultCos / nbrProblemDimension;
            tempResultCos = Math.Exp(tempResultCos);

            result = tempResult - tempResultCos + Math.E;




            //val = 0;
            //for (int iShiftData = 0; iShiftData < functionParameter.Length; iShiftData++)
            //    val += functionParameter[iShiftData] * functionParameter[iShiftData];
            //val /= (double)functionParameter.Length;
            //val = -0.2D * Math.Sqrt(val);
            //result = (double)(Math.Exp((double) val));
            ////if (result == 1)
            ////{
            ////    MathNet.Numerics.Complex32 numero1 = new MathNet.Numerics.Complex32((float)MathNet.Numerics.Constants.E, 0);
            ////    MathNet.Numerics.Complex32 numero2 = new MathNet.Numerics.Complex32((float) -1 * (float)val, 0);
            ////    MathNet.Numerics.Complex32 numero3 = MathNet.Numerics.Complex32.Pow(numero1, numero2);
            ////}
            //result = 1.0d / (Math.Pow(Math.E, -1.0d * val));
            //result = 20D - 20D * result;

            //val = 0;
            //for (int iShiftData = 0; iShiftData < functionParameter.Length; iShiftData++)
            //    val += Math.Cos(2 * Math.PI * functionParameter[iShiftData]);
            //val /= (double)functionParameter.Length;
            //result += Math.E - Math.Exp(val);

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
