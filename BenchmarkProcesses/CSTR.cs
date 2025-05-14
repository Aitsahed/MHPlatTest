using MHPlatTest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using MHPlatTest.Utility;
using MathNet.Numerics.OdeSolvers;
using MathNet.Numerics.LinearAlgebra;
using MHPlatTest.Divers;
using System.Reflection;

namespace MHPlatTest.BenchmarkProcesses
{
    /// <summary>
    /// The CSTR applicaytion as described in Ait sahed, O. , K. Kara and A. Benyoucef (2015). "Artificial bee colony-based predictive control for non-linear systems." Transactions of the Institute of Measurement and Control 37(6): 780-792.
    /// The process considered is the continuous stirred tank reactor within it a given product A will be converted into another product B via an exothermic chemical reaction. A coolant flow  qc (the control input) controls the reactor temperature, which controls in its turn the concentration of the resulted product  (The process output). 
    /// is highly multimodal. It has a single global minimum at the origin with value 0.
    /// </summary>
    internal class CSTR : IControlProcess, IBenchmark
    {
        public CSTR()
        {
            //Generate unique identifier for current instance
            Random random = new Random();
            ParentInstanceID = random.Next();
        }

        public string Name { get; set; } = "CSTR";
        public string Description { get; set; } = "The continuous stirred tank reactor is a highly nonlinear process";
        public int IDNumero { get; set; } = 1;
        public double[] SearchSpaceMinValue { get; set; } = new double[1] { 89 };
        public double[] SearchSpaceMaxValue { get; set; } = new double[1] { 111 };
        public short MinProblemDimension { get; set; } = 1;
        public short MaxProblemDimension { get; set; } = short.MaxValue;
        public int ParentInstanceID { get; set; }
        public BenchmarkOrProcessType BenchmarkOrProcess { get; set; } = BenchmarkOrProcessType.Process;
        double[] IControlProcess.InitialConditions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        /// <summary>
        /// the reference along the prediction horizon
        /// </summary>
        public double[] Reference { get; set; }

        /// <summary>
        /// The q value for the cost function
        /// </summary>
        private double Qvalue = 1;

        /// <summary>
        /// The R value for the cost function
        /// </summary>
        private double R = 0.0002;

        /// <summary>
        /// The initial conditons for the current process
        /// </summary>
        public double[] InitialConditions = new double[2] { 0.079890998, 443.339 };


        /// <summary>
        /// The number of the inputs for the current process
        /// </summary>
        private int NumberProcessInputs = 1;

        /// <summary>
        /// The sampling period adopted in minutes
        /// </summary>
        private double SamplingPeriod { get; set; } = 0.1;

        /// <summary>
        /// The number of the outputs for the current process
        /// </summary>
        private int NumberProcessOutputs = 1;

        /// <summary>
        /// To be used to identify the current process time
        /// </summary>
        public int CurrentSampleID { get; set; }

        /// <summary>
        /// To be used to identify the current process time
        /// </summary>
        public int ControlHorizonLength { get; set; }


        /// <summary>
        /// The current output and the two previous ones
        /// </summary>
        private double[] PreviousProcessOutputs { get; set; } = new double[3] { 0.079890998, 0.079890998, 0.079890998 };


        /// <summary>
        /// The current inputs and the two previous ones
        /// </summary>
        private double[] PreviousProcessinputs { get; set; } = new double[3] { 97.22687, 97.22687, 97.22687 };







        public void UpdatePreviousPreviousStatus(double newestNnput, double newestOutput)
        {
            //Updating the previous inputs
            PreviousProcessinputs[2] = PreviousProcessinputs[1];
            PreviousProcessinputs[1] = PreviousProcessinputs[0];
            PreviousProcessinputs[0] = newestNnput;
            //Updating the previous outputs
            PreviousProcessOutputs[2] = PreviousProcessOutputs[1];
            PreviousProcessOutputs[1] = PreviousProcessOutputs[0];
            PreviousProcessOutputs[0] = newestOutput;
        }


        /// <summary>
        /// compute the process output
        /// </summary>
        /// <param name="ProcessInputs"> Contains the inputs to the process</param>
        /// <param name="currentNumberofunctionEvaluation"></param>
        /// <param name="ShiftOptimumToZero"></param>
        /// <returns></returns>
        public double ComputeValue(double[] ProcessInputs, ref int currentNumberofunctionEvaluation, bool ShiftOptimumToZero)
        {
            //ProcessInputs[0] = 108.1;
            //ProcessInputs[1] = 108.1;


            //Updating number of function evaluation
            currentNumberofunctionEvaluation++;


            int predictionHorizonLength = 10;
            double[] dataForFuzzyOutputCalculator = new double[5];
            double[] predictedProcessOutputs = new double[predictionHorizonLength];
            double input0, input1;
            double output0, output1, output2;

            output0 = PreviousProcessOutputs[0];
            output1 = PreviousProcessOutputs[1];
            output2 = PreviousProcessOutputs[2];

            input0 = PreviousProcessinputs[0];
            input1 = PreviousProcessinputs[1];

            List<double> commincre = new();
            double commandIncrementCost = 0;
            for (int i = 0; i < predictionHorizonLength; i++)
            {
                input1 = input0;

                if (i < ControlHorizonLength)
                {
                    commandIncrementCost = commandIncrementCost + Math.Pow((ProcessInputs[i] - input0), 2) * R;
                    input0 = ProcessInputs[i];
                    commincre.Add((ProcessInputs[i] - input0));
                }
                else
                {
                    input0 = input1;
                }

                dataForFuzzyOutputCalculator[0] = output0;
                dataForFuzzyOutputCalculator[1] = output1;
                dataForFuzzyOutputCalculator[2] = output2;
                dataForFuzzyOutputCalculator[3] = input1;
                dataForFuzzyOutputCalculator[4] = 1;


                //Computing the fuzzy output
                predictedProcessOutputs[i] = Fuzzy_2MF_output_calculator(dataForFuzzyOutputCalculator);

                output2 = output1;
                output1 = output0;
                output0 = predictedProcessOutputs[i];
            }

            // Evaluating the cost function
            double costValue = 0;
            for (int i = 0; i < predictionHorizonLength; i++)
            {
                costValue += Math.Pow((predictedProcessOutputs[i] - Reference[i]), 2) * Qvalue;
            }
            costValue += commandIncrementCost;


            return costValue;

        }



        private double Fuzzy_2MF_output_calculator(double[] inputs)
        {
            double[] centre_input = { 89, 111 };
            double[] centre_output = { 0.06, 0.16 };
            double[,] clusters = { { 0.06, 0.16 }, { 0.06, 0.16 }, { 0.06, 0.16 }, { 89, 111 } };
            double[,] rule_matrice = { { 1, 1, 1, 1 }, { 2, 1, 1, 1 }, { 1, 2, 1, 1 }, { 2, 2, 1, 1 }, { 1, 1, 2, 1 }, { 2, 1, 2, 1 }, { 1, 2, 2, 1 }, { 2, 2, 2, 1 }, { 1, 1, 1, 2 }, { 2, 1, 1, 2 }, { 1, 2, 1, 2 }, { 2, 2, 1, 2 }, { 1, 1, 2, 2 }, { 2, 1, 2, 2 }, { 1, 2, 2, 2 }, { 2, 2, 2, 2 }, };
            int input_number = 4;
            double[] diff = new double[4] { 0.1000, 0.1000, 0.1000, 22.0000 };
            double[] all = new double[4];
            double[] a5 = new double[4];
            //Fuzzy model of the CSTR
            double[,] matrice_parametres_des_regles = { { -0.004932986679857, -0.016546864209191, 0.017283855951815, 0.000067026195457, -0.005111077336769 }, { 0.316505443455383, -0.539060110338276, 0.230099746453891, -0.000158396823069, 0.016453678744097 }, { 0.420947768290682, -0.887041492199589, 0.516792641028788, -0.000250303344090, 0.020085131796390 }, { -0.345907339401094, 0.903405220673699, -0.601986083763085, 0.000065181818140, 0.000523509825127 }, { 0.436921971233946, -0.695750969674051, 0.354229586449532, -0.000074116568378, -0.000622234124580 }, { 0.716512948470795, -1.356887890650746, 0.736382639132789, -0.000174397115663, 0.008483114292623 }, { -1.396304235710798, 2.509840182597425, -1.309761625773019, 0.000557045195836, -0.035421492605980 }, { 0.009956909619681, -0.040746217279321, 0.077678032270049, -0.000010961638949, -0.003227761298167 }, { 0.013478684046192, 0.015515527546560, -0.010634192814805, 0.000065187325330, -0.007711273394514 }, { -0.369703093617279, 0.685974440215715, -0.344867142856685, 0.000181557567583, -0.014974619869380 }, { 0.242005591061444, -0.414719813621765, 0.199196518422273, -0.000134599593820, 0.012421570603397 }, { 0.258663974227714, -0.366972761038504, 0.206516981128490, -0.000131318201777, 0.004162631146384 }, { -0.121190369293666, 0.255897961917324, -0.153662562344497, 0.000091582270594, -0.006377100964896 }, { 0.028007394142921, -0.082190702675796, -0.001174012508943, 0.000024503778250, 0.004761191703277 }, { 0.091917462473068, -0.150517890607018, 0.093935666388204, -0.000018201490082, -0.001119402728423 }, { 0.032332591254802, -0.051157791476931, 0.037032648593315, -0.000036484777094, 0.002731750255790 } };
            for (int i = 0; i < 5; i++)
            {
                for (int i1 = 0; i1 < 16; i1++)
                {
                    matrice_parametres_des_regles[i1, i] = matrice_parametres_des_regles[i1, i] * 100;
                }
            }

            double[] aa = new double[16];
            double[] bb = new double[16];



            for (int i = 0; i < 4; i++)
            {
                all[i] = (inputs[i] - clusters[i, 0]) / diff[i];
            }


            for (int j = 0; j < 16; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    aa[j] += matrice_parametres_des_regles[j, i] * inputs[i];
                }
            }


            for (int i = 0; i < 16; i++)
            {
                for (int jj = 0; jj < 4; jj++)
                {
                    if (rule_matrice[i, jj] == 1)
                    {
                        a5[jj] = 1 - all[jj];
                    }
                    else
                    {
                        a5[jj] = all[jj];
                    }
                }

                bb[i] = a5.Minimum();

            }

            double result = 0;
            for (int i = 0; i < 16; i++)
            {
                result += bb[i] * aa[i];
            }
            result = result / bb.Sum();

            return result;
        }





        /// <summary>
        /// compute the actual process output using the process' ODE
        /// </summary>
        /// <param name="functionParameter"> Contains the initial conditions values & the time period & the input to the process</param>
        /// <param name="currentNumberofunctionEvaluation"></param>
        /// <param name="ShiftOptimumToZero"></param>
        /// <returns></returns>
        public double ComputeActualProcessOutput(double[] ProcessInputs)
        {

            //Initial conditions
            Vector<double> initilConditions = Vector<double>.Build.Dense(new[] { InitialConditions[0], InitialConditions[1] });

            double computedOutput = 0;

            //Time period
            double startingTime = (CurrentSampleID - 1) * SamplingPeriod;



            #region Computing the process output

            //Declration
            Func<double, Vector<double>, Vector<double>> CSTR_ODE = CSTR_ProcessEquation(ProcessInputs[0]);
            int steps = 500;

            // Solve the ODE using Runge-Kutta 4th order method (similar to ode45)
            Vector<double>[] result = RungeKutta.FourthOrder(initilConditions, startingTime, startingTime + SamplingPeriod, steps, CSTR_ODE);


            //Updating initial condition for next sampling period
            InitialConditions = result[result.Length - 1].ToArray();


            //Retrieving the output
            computedOutput = InitialConditions[0];

            #endregion



            return computedOutput;

        }



        //Defining the differential equations
        private Func<double, Vector<double>, Vector<double>> CSTR_ProcessEquation(double input_qc)
        {

            return (t, Y) =>
            {
                double[] A = Y.ToArray();
                double y0 = A[0];
                double y1 = A[1];

                double q = 100;
                double v = 100;
                double k0 = 7.2e10;
                double E_R = 10000;
                double T0 = 350;
                double Tc0 = 350;
                double Cp = 1;
                double Cpc = Cp;
                double rho = 1000;
                double rhoc = rho;
                double ha = 700000;
                double Ca0 = 1;

                double k1 = -(-2e5 * k0) / (rho * Cp);
                double k2 = (rhoc * Cpc) / (rho * Cp * v);
                double k3 = ha / (rhoc * Cpc);


                return Vector<double>.Build.Dense(new[] { (q / v) * (Ca0 - y0) - k0 * y0 * Math.Exp(-E_R / y1), (q / v) * (T0 - y1) + k1 * y0 * Math.Exp(-E_R / y1) + k2 * input_qc * (1 - Math.Exp(-(k3 / input_qc))) * (Tc0 - y1) });

                ;


            };

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
