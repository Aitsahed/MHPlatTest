using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHPlatTest.BenchmarkFunctions.FixedDimension
{
    /// <summary>
    ///   The Branin function is commonly used as a test function for metamodeling in computer experiments, especially in the context of optimization (Forrester et al., 2008; Picheny et al., 2012). /// </summary>
    /// A. Forrester, A. Sóbester, and A. Keane, Engineering Design via Surrogate Modelling: A Practical Guide, West Sussex: John Wiley & Sons Ltd., 2008, pp. 196-197. DOI:10.1002/9780470770801
    /// V. Picheny, T. Wagner, and D.Ginsbourger, “A benchmark of Kriging-based infill criteria for noisy optimization,” HAL, hal-00658212, 2012. URL
    /// the implemented function optimum has been shifted to zero
    /// https://uqworld.org/t/branin-function/53
    /// https://towardsdatascience.com/optimization-eye-pleasure-78-benchmark-test-functions-for-single-objective-optimization-92e7ed1d1f12
    /// </summary>


    internal class Branin : IBenchmark
    {
        public Branin()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.BenchmarkFunction;

        public string Name { get; set; } = "Branin Function";
        public string Description { get; set; } = "The Branin function is commonly used as a test function for metamodeling in computer experiments, especially in the context of optimization (Forrester et al., 2008; Picheny et al., 2012).";
        public int IDNumero { get; set; } = 1;
        public double[] SearchSpaceMinValue { get; set; } = new double[2] { -5, 0 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[2] { 0, 15 };
        public short MinProblemDimension { get; set; } = 2;
        public short MaxProblemDimension { get; set; } = 2;
        public int ParentInstanceID { get; set; }

        public double ComputeValue(double[] functionParameter, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {
            //Increase the current number of function evaluation by 1
            currentNumberofunctionEvaluation++;

            int nbrProblemDimension = functionParameter.Length;
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }

            //new double[] { -1 * Math.PI, 12.275 }, new double[] { Math.PI, 2.275 }, new double[] { 9.42478, 2.475 }

            //functionParameter[0] = -1 * Math.PI;
            //functionParameter[1] = 12.275;


            double result = 0;

            double a = 1;
            double b = 5.1 / (2 * Math.PI) / (2 * Math.PI);
            double c = 5 / Math.PI;
            double r = 6;
            double s = 10;
            double t = 1 / (8 * Math.PI);

            result = (functionParameter[1] - b * functionParameter[0] * functionParameter[0] + c * functionParameter[0] - r);
            result *= a * result;
            result += (s * (1.0 - t) * Math.Cos(functionParameter[0]) + s);
            //result -= 0.39788735772973816;

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


            //The optimum of the function has been shifted to zero
            double tempResult =  0.39788735772973816;

            return tempResult;
        }

        /// <summary>
        ///  x∗1=(−π,12.275)
        /// x∗2=(π,2.275)
        ///x∗3=(9.42478,2.475)
        /// </summary>
        /// <param name="nbrProblemDimension"></param>
        /// <returns></returns>
        public List<double[]> OptimalPoint(int nbrProblemDimension)
        {
            if (MinProblemDimension == MaxProblemDimension)
            {
                nbrProblemDimension = MinProblemDimension;
            }


            return new List<double[]>() { new double[] { -1 * Math.PI, 12.275 }, new double[] { Math.PI, 2.275 }, new double[] { 9.42478, 2.475 } };
        }

    }
}
