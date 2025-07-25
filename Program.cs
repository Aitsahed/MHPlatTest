// See https://aka.ms/new-console-template for more information


using MHPlatTest.Algorithms;
using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using MHPlatTest.Models;
using MHPlatTest.Utility;
using System.Reflection;
//using Newtonsoft.Json;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using MHPlatTest.BenchmarkFunctions;
using MHPlatTest.BenchmarkFunctions.FixedDimension;
using System.Diagnostics;
using MHPlatTest.BenchmarkProcesses;
using System.Diagnostics.Metrics;

namespace MHPlatTest
{

    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
    }

    internal class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
    }

    //...


    public class Program
    {




        static void Main()
        {

            //The first thing is to set which type of optimization the user want to run
            OptimizationExperienceToRunType optimizationExperienceToRun = OptimizationExperienceToRunType.NumericalBenchmarkFunctionTests;



            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            bool runOneProcessOnlyFlag = false;

            //In debug mode, always run in single process mode
#if DEBUG
            runOneProcessOnlyFlag = true;
#endif



            Random randomGenerator = new Random();
            List<OptimizationParameter> MHConf;
            Program mainProgram = new();



            //Constructing the differents configuration
            MHConf = mainProgram.DoOptimizationProcessConfiguration();



            string pathCSVFile;
            int counterCSVFile;
            string executionTimeStamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
            string pathFolderForResults = Environment.CurrentDirectory + "\\Results\\" + executionTimeStamp + "\\";
            //Creating folder if not existing
            if (Directory.Exists(pathFolderForResults) == false)
            {
                Directory.CreateDirectory(pathFolderForResults);
            }

            //the number of random repetitions to run for each scenario
            int numberTestRepetition = 100;  //todo Neber test repetition
            int populationSize;
            //Loading the desired benchmark functions
            List<IBenchmark> TempUsedBenchmarkFunctionList = new();
            List<IControlProcess> controlProcessesToBenchmarkList = new();
            List<List<IBenchmark>> usedBenchmarkFunctionList = new();
            List<string> benchmarkFunctionNameToIncludeList = new List<string>();
            List<string> benchmarkFunctionNameToIgnoreList = new List<string>() { "CEC21", "NotToBeUsed", "FixedDimension", "BenchmarkProcesses" };
            TempUsedBenchmarkFunctionList = LoadBenchmarkFunctions(benchmarkFunctionNameToIgnoreList, benchmarkFunctionNameToIncludeList);
            //Exemple how to include specified benchark function
            //TempUsedBenchmarkFunctionList = new() { new Rastrigin(), new Booth() };
            usedBenchmarkFunctionList.Add(TempUsedBenchmarkFunctionList);



            //Variables to store the results
            List<Tuple<string, List<double>>> StatsForCSV = new();
            List<List<Tuple<string, List<double>>>> ListStatsForCSV = new();
            List<List<Tuple<string, List<double>>>> StatsByMHAlgoAndBenchFunc = new();
            List<Tuple<MHOptimizationResult, StatsToComputeType>> StatsToComputeList = new();
            List<Tuple<MHOptimizationResult, StatsToComputeType>> csvFile_StatsToComputeList = new();
            string contentCSVFile = "";


            //Variables for choosing the optimization algos to run
            List<string> metaheuristicNameToIgnoreList = new List<string>();
            List<string> metaheuristicNameToIncludeList = new List<string>();
            List<string> configurationTextDetailsList = new List<string>();
            List<IMHAlgorithm> usedMetaheuristicAlgorithmList = new List<IMHAlgorithm>();
            List<List<GlobalBatchResultModel>?> GlobalResults = new();
            List<string> algoToIgnore = new List<string>();


            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //If the user is running numerical analysis on the numerical benchmark functions
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            if (optimizationExperienceToRun == OptimizationExperienceToRunType.NumericalBenchmarkFunctionTests)
            {

                //Preparing the stats to be gathered
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Max));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Min));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.STD));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Median));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfFunctionEvaluation, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfTotalIteration, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimumFound, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ScoutBeesGeneratedCount, StatsToComputeType.Mean));

                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Max));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Min));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.STD));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Median));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfFunctionEvaluation, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfTotalIteration, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimumFound, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ScoutBeesGeneratedCount, StatsToComputeType.Mean));




                //Creating file for stats in disk
                counterCSVFile = 1;
                pathCSVFile = pathFolderForResults + "ResultsByDim_" + executionTimeStamp + ".csv";




                int dimensionToUse = 0;
                populationSize = 40;

                //Working the enchmark functions with varying dimensionality
                //Choose the dimensions you want to test the optimization algorithms for
                for (int i = 0; i < 3; i++)
                {
                    usedMetaheuristicAlgorithmList = new List<IMHAlgorithm>();

                    switch (i)
                    {
                        case 0:
                            dimensionToUse = 10;
                            break;
                        case 1:
                            dimensionToUse = 30;
                            break;
                        case 2:
                            dimensionToUse = 50;
                            break;
                        case 3:
                            dimensionToUse = 100;
                            break;
                        case 4:
                            dimensionToUse = 250;
                            break;
                        default:
                            break;
                    }



                    MHConf.Where(x => x.Name == MHAlgoParameters.StopOptimizationWhenOptimumIsReached).First().Value = true;
                    //MHConf.Where(x => x.Name == MHAlgoParameters.MaxItertaionNumber).First().Value = 1500;
                    MHConf.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = 100;
                    MHConf.Where(x => x.Name == MHAlgoParameters.FunctionValueSigmaTolerance).First().Value = 1e-16;
                    MHConf.Where(x => x.Name == MHAlgoParameters.StoppingCriteriaType).First().Value = StoppingCriteriaType.MaximalNumberOfFunctionEvaluation;
                    MHConf.Where(x => x.Name == MHAlgoParameters.ProblemDimension).First().Value = dimensionToUse;
                    MHConf.Where(x => x.Name == MHAlgoParameters.MaxFunctionEvaluationNumber).First().Value = 100000;//todo number MaxFunctionEvaluationNumber 100000
                    MHConf.Where(x => x.Name == MHAlgoParameters.PopulationSize).First().Value = populationSize;


                    //Divers ABC algos
                    usedMetaheuristicAlgorithmList.Add(new DirectedABC(MHConf, "", randomGenerator.Next()));
                    usedMetaheuristicAlgorithmList.Add(new BasicABC(MHConf, "", randomGenerator.Next()));
                    usedMetaheuristicAlgorithmList.Add(new ImprovedABCAdaptiveMingZhao(MHConf, "", randomGenerator.Next()));
                    usedMetaheuristicAlgorithmList.Add(new GBestABC(MHConf, "", randomGenerator.Next()));


                    //MABC
                    MHConf.Where(x => x.Name == MHAlgoParameters.MABC_LimitValue).First().Value = 200;
                    MHConf.Where(x => x.Name == MHAlgoParameters.MABC_ModificationRate).First().Value = 0.4d;
                    MHConf.Where(x => x.Name == MHAlgoParameters.MABC_UseScalingFactor).First().Value = true;
                    usedMetaheuristicAlgorithmList.Add(new MABC(MHConf, "MABC Limit200 ScaFactor MR.4", randomGenerator.Next()));


                    //ARABC
                    MHConf.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = dimensionToUse * (int)MHConf.Where(x => x.Name == MHAlgoParameters.PopulationSize).First().Value;
                    usedMetaheuristicAlgorithmList.Add(new ARABC(MHConf, "", randomGenerator.Next()));
                    MHConf.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = 100;



                    //AdaABC
                    MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneNumberOfDimensionUsingGBest).First().Value = true;
                    MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneScoutGenerationType).First().Value = true;
                    MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneProbabilityEquationType).First().Value = true;
                    MHConf.Where(x => x.Name == MHAlgoParameters.ABC_ProbabilityEquationType).First().Value = ABC_ProbabilityEquationType.ComplementOriginal;
                    MHConf.Where(x => x.Name == MHAlgoParameters.ScoutGeneration).First().Value = ScoutGenerationType.Random;
                    MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_NumberOfIterationsToTuneParameters).First().Value = 20;
                    usedMetaheuristicAlgorithmList.Add(new AdaABC(MHConf, "Proposed Algo", randomGenerator.Next()));




                    GlobalResults.Add(mainProgram.StartOptimizationProcess(usedMetaheuristicAlgorithmList, usedBenchmarkFunctionList[0], numberTestRepetition, randomGenerator, runOneProcessOnlyFlag));
                    configurationTextDetailsList.Add(MHConf.MHConfig2String());
                    StatsByMHAlgoAndBenchFunc.Add(ComputeAndDisplayStats(GlobalResults[i], algoToIgnore, StatsToComputeList));




                    #region Stats Handling

                    //Saving stats as CSV File
                    StatsForCSV = DiversExtendedProperties.ComputeStat(GlobalResults[GlobalResults.Count - 1], csvFile_StatsToComputeList, true, algoToIgnore, GroupByType.Algorithm, OrderingType.None);
                    ListStatsForCSV.Add(StatsForCSV);

                    contentCSVFile += Environment.NewLine;

                    contentCSVFile += "Configuration" + Environment.NewLine;
                    contentCSVFile += "Nbre repetition" + numberTestRepetition + Environment.NewLine;
                    contentCSVFile += configurationTextDetailsList[GlobalResults.Count - 1] + Environment.NewLine + "Results" + Environment.NewLine + Environment.NewLine;
                    contentCSVFile += DiversExtendedProperties.FramtStatsCSVFile(StatsForCSV, csvFile_StatsToComputeList);

                    contentCSVFile += Environment.NewLine;
                    contentCSVFile += Environment.NewLine;
                    contentCSVFile += Environment.NewLine;
                    contentCSVFile += Environment.NewLine;
                    contentCSVFile += Environment.NewLine;



                    //Saving Data into disk


                    //Saving as CSV file
                    try
                    {
                        File.AppendAllText(pathCSVFile, contentCSVFile);
                    }
                    catch (Exception)
                    {
                        pathCSVFile = pathFolderForResults + "ResultsByDim_" + executionTimeStamp + counterCSVFile + ".csv";
                        while (File.Exists(pathCSVFile) == true)
                        {
                            pathCSVFile = pathFolderForResults + "ResultsByDim_" + executionTimeStamp + counterCSVFile++ + ".csv";
                        }
                        File.AppendAllText(pathCSVFile, contentCSVFile);
                    }

                    contentCSVFile = "";




                    #endregion

                }






                #region Fixed dimension benchmark functions
                dimensionToUse = 2;
                foreach (var item in usedMetaheuristicAlgorithmList)
                {
                    item.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ProblemDimension).First().Value = dimensionToUse;
                }

                foreach (var item in usedMetaheuristicAlgorithmList)
                {
                    item.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = 100;
                }

                //Working with the fixed dimension benchmark functions
                benchmarkFunctionNameToIgnoreList = new List<string>() { "CEC21", "NotToBeUsed" };
                benchmarkFunctionNameToIncludeList = new List<string>() { "FixedDimension" };
                TempUsedBenchmarkFunctionList = LoadBenchmarkFunctions(benchmarkFunctionNameToIgnoreList, benchmarkFunctionNameToIncludeList);
                usedBenchmarkFunctionList.Add(TempUsedBenchmarkFunctionList);

                GlobalResults.Add(mainProgram.StartOptimizationProcess(usedMetaheuristicAlgorithmList, usedBenchmarkFunctionList[usedBenchmarkFunctionList.Count - 1], numberTestRepetition, randomGenerator, runOneProcessOnlyFlag));
                configurationTextDetailsList.Add(MHConf.MHConfig2String());

                //Printing the used confguration
                Console.WriteLine("Configuration for Fixed dimension benchmark functions");
                Console.WriteLine(configurationTextDetailsList[configurationTextDetailsList.Count - 1]);
                Console.WriteLine("");
                Console.WriteLine("Results");
                StatsByMHAlgoAndBenchFunc.Add(ComputeAndDisplayStats(GlobalResults[GlobalResults.Count - 1], algoToIgnore, StatsToComputeList));


                //Saving stats as CSV File
                StatsForCSV = DiversExtendedProperties.ComputeStat(GlobalResults[GlobalResults.Count - 1], csvFile_StatsToComputeList, true, algoToIgnore, GroupByType.Algorithm, OrderingType.None);
                ListStatsForCSV.Add(StatsForCSV);

                contentCSVFile += "Configuration" + Environment.NewLine + configurationTextDetailsList[configurationTextDetailsList.Count - 1] + Environment.NewLine + "Results" + Environment.NewLine + Environment.NewLine;
                contentCSVFile += DiversExtendedProperties.FramtStatsCSVFile(StatsForCSV, csvFile_StatsToComputeList);

                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;


                try
                {
                    File.AppendAllText(pathCSVFile, contentCSVFile);
                }
                catch (Exception)
                {
                    pathCSVFile = pathFolderForResults + "ResultsByDim_" + executionTimeStamp + counterCSVFile + ".csv";
                    while (File.Exists(pathCSVFile) == true)
                    {
                        pathCSVFile = pathFolderForResults + "ResultsByDim_" + executionTimeStamp + counterCSVFile++ + ".csv";
                    }
                    File.AppendAllText(pathCSVFile, contentCSVFile);
                }

                contentCSVFile = "";


                #endregion







                //Saving result as seialized object
                string jsonString = JsonSerializer.Serialize(ListStatsForCSV);

                int counter = 1;
                string fileName = pathFolderForResults + "ResByDimSerailized_" + executionTimeStamp + counter + ".txt";
                while (File.Exists(fileName) == true)
                {
                    fileName = pathFolderForResults + "ResByDimSerailized_" + executionTimeStamp + counter++ + ".txt";
                }

                File.WriteAllText(fileName, jsonString);

            }



























            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //If the user is running convergence analysis on the numerical benchmark functions
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            if (optimizationExperienceToRun == OptimizationExperienceToRunType.ConvergenceBenchmarkFunctionTest)
            {

                //Preparing the stats to be gathered
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Max));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Min));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.STD));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Median));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfFunctionEvaluation, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfTotalIteration, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimumFound, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ScoutBeesGeneratedCount, StatsToComputeType.Mean));

                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Max));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Min));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.STD));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimalFunctionValue, StatsToComputeType.Median));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfFunctionEvaluation, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfTotalIteration, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.OptimumFound, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ScoutBeesGeneratedCount, StatsToComputeType.Mean));




                //Creating file for stats in disk
                counterCSVFile = 1;
                pathCSVFile = pathFolderForResults + "ConvergenceData_" + executionTimeStamp + ".csv";



                int dimensionToUse = 0;
                populationSize = 40; // Todo convergence population size 40

                //Working the enchmark functions with varying dimensionality
                //Choose the dimensions you want to test the optimization algorithms for
                for (int i = 0; i < 1; i++)
                {
                    usedMetaheuristicAlgorithmList = new List<IMHAlgorithm>();

                    switch (i)
                    {
                        case 0:
                            dimensionToUse = 10;
                            break;
                        case 1:
                            dimensionToUse = 30;
                            break;
                        case 2:
                            dimensionToUse = 50;
                            break;
                        case 3:
                            dimensionToUse = 100;
                            break;
                        case 4:
                            dimensionToUse = 250;
                            break;
                        default:
                            break;
                    }



                    MHConf.Where(x => x.Name == MHAlgoParameters.StopOptimizationWhenOptimumIsReached).First().Value = true;
                    //MHConf.Where(x => x.Name == MHAlgoParameters.MaxItertaionNumber).First().Value = 1500;
                    MHConf.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = 100;
                    MHConf.Where(x => x.Name == MHAlgoParameters.FunctionValueSigmaTolerance).First().Value = 1e-16;
                    MHConf.Where(x => x.Name == MHAlgoParameters.StoppingCriteriaType).First().Value = StoppingCriteriaType.MaximalNumberOfFunctionEvaluation;
                    MHConf.Where(x => x.Name == MHAlgoParameters.ProblemDimension).First().Value = dimensionToUse;
                    MHConf.Where(x => x.Name == MHAlgoParameters.MaxFunctionEvaluationNumber).First().Value = 500000; // todo convergence MaxFunctionEvaluationNumber
                    MHConf.Where(x => x.Name == MHAlgoParameters.PopulationSize).First().Value = populationSize;


                    //Divers ABC algos
                    usedMetaheuristicAlgorithmList.Add(new DirectedABC(MHConf, "", randomGenerator.Next()));
                    usedMetaheuristicAlgorithmList.Add(new BasicABC(MHConf, "", randomGenerator.Next()));
                    usedMetaheuristicAlgorithmList.Add(new ImprovedABCAdaptiveMingZhao(MHConf, "", randomGenerator.Next()));
                    usedMetaheuristicAlgorithmList.Add(new GBestABC(MHConf, "", randomGenerator.Next()));


                    //MABC
                    MHConf.Where(x => x.Name == MHAlgoParameters.MABC_LimitValue).First().Value = 200;
                    MHConf.Where(x => x.Name == MHAlgoParameters.MABC_ModificationRate).First().Value = 0.4d;
                    MHConf.Where(x => x.Name == MHAlgoParameters.MABC_UseScalingFactor).First().Value = true;
                    usedMetaheuristicAlgorithmList.Add(new MABC(MHConf, "MABC Limit200 ScaFactor MR.4", randomGenerator.Next()));


                    //ARABC
                    MHConf.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = dimensionToUse * (int)MHConf.Where(x => x.Name == MHAlgoParameters.PopulationSize).First().Value;
                    usedMetaheuristicAlgorithmList.Add(new ARABC(MHConf, "", randomGenerator.Next()));
                    MHConf.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = 100;



                    //AdaABC
                    MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneNumberOfDimensionUsingGBest).First().Value = true;
                    MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneScoutGenerationType).First().Value = true;
                    MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneProbabilityEquationType).First().Value = true;
                    MHConf.Where(x => x.Name == MHAlgoParameters.ABC_ProbabilityEquationType).First().Value = ABC_ProbabilityEquationType.ComplementOriginal;
                    MHConf.Where(x => x.Name == MHAlgoParameters.ScoutGeneration).First().Value = ScoutGenerationType.Random;
                    MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_NumberOfIterationsToTuneParameters).First().Value = 20;
                    usedMetaheuristicAlgorithmList.Add(new AdaABC(MHConf, "Proposed Algo", randomGenerator.Next()));




                    GlobalResults.Add(mainProgram.StartOptimizationProcess(usedMetaheuristicAlgorithmList, usedBenchmarkFunctionList[0], numberTestRepetition, randomGenerator, runOneProcessOnlyFlag));
                    configurationTextDetailsList.Add(MHConf.MHConfig2String());
                    StatsByMHAlgoAndBenchFunc.Add(ComputeAndDisplayStats(GlobalResults[i], algoToIgnore, StatsToComputeList));




                    #region Stats Handling

                    //Saving stats as CSV File
                    StatsForCSV = DiversExtendedProperties.ComputeStat(GlobalResults[GlobalResults.Count - 1], csvFile_StatsToComputeList, true, algoToIgnore, GroupByType.Algorithm, OrderingType.None);
                    ListStatsForCSV.Add(StatsForCSV);


                    contentCSVFile += "Configuration" + Environment.NewLine;
                    contentCSVFile += "Nbre repetition" + numberTestRepetition + Environment.NewLine;
                    contentCSVFile += configurationTextDetailsList[GlobalResults.Count - 1] + Environment.NewLine + "Results" + Environment.NewLine + Environment.NewLine;
                    contentCSVFile += DiversExtendedProperties.FramtStatsCSVFile(StatsForCSV, csvFile_StatsToComputeList);

                    contentCSVFile += Environment.NewLine;
                    contentCSVFile += Environment.NewLine;
                    contentCSVFile += Environment.NewLine;
                    contentCSVFile += Environment.NewLine;
                    contentCSVFile += Environment.NewLine;



                    //Saving Data into disk
                    try
                    {
                        File.AppendAllText(pathCSVFile, contentCSVFile);
                    }
                    catch (Exception)
                    {
                        pathCSVFile = pathFolderForResults + "ConvergenceData_" + executionTimeStamp + counterCSVFile + ".csv";
                        while (File.Exists(pathCSVFile) == true)
                        {
                            pathCSVFile = pathFolderForResults + "ConvergenceData_" + executionTimeStamp + counterCSVFile++ + ".csv";
                        }
                        File.AppendAllText(pathCSVFile, contentCSVFile);
                    }

                    contentCSVFile = "";

                    #endregion

                }






                #region Fixed dimension benchmark functions
                dimensionToUse = 2;
                foreach (var item in usedMetaheuristicAlgorithmList)
                {
                    item.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ProblemDimension).First().Value = dimensionToUse;
                }

                foreach (var item in usedMetaheuristicAlgorithmList)
                {
                    item.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = 100;
                }

                //Working with the fixed dimension benchmark functions
                benchmarkFunctionNameToIgnoreList = new List<string>() { "CEC21", "NotToBeUsed" };
                benchmarkFunctionNameToIncludeList = new List<string>() { "FixedDimension" };
                TempUsedBenchmarkFunctionList = LoadBenchmarkFunctions(benchmarkFunctionNameToIgnoreList, benchmarkFunctionNameToIncludeList);
                usedBenchmarkFunctionList.Add(TempUsedBenchmarkFunctionList);

                GlobalResults.Add(mainProgram.StartOptimizationProcess(usedMetaheuristicAlgorithmList, usedBenchmarkFunctionList[usedBenchmarkFunctionList.Count - 1], numberTestRepetition, randomGenerator, runOneProcessOnlyFlag));
                configurationTextDetailsList.Add(MHConf.MHConfig2String());

                //Printing the used confguration
                Console.WriteLine("Configuration for Fixed dimension benchmark functions");
                Console.WriteLine(configurationTextDetailsList[configurationTextDetailsList.Count - 1]);
                Console.WriteLine("");
                Console.WriteLine("Results");
                StatsByMHAlgoAndBenchFunc.Add(ComputeAndDisplayStats(GlobalResults[GlobalResults.Count - 1], algoToIgnore, StatsToComputeList));


                //Saving stats as CSV File
                StatsForCSV = DiversExtendedProperties.ComputeStat(GlobalResults[GlobalResults.Count - 1], csvFile_StatsToComputeList, true, algoToIgnore, GroupByType.Algorithm, OrderingType.None);
                ListStatsForCSV.Add(StatsForCSV);

                contentCSVFile += "Configuration" + Environment.NewLine + configurationTextDetailsList[configurationTextDetailsList.Count - 1] + Environment.NewLine + "Results" + Environment.NewLine + Environment.NewLine;
                contentCSVFile += DiversExtendedProperties.FramtStatsCSVFile(StatsForCSV, csvFile_StatsToComputeList);

                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;


                try
                {
                    File.AppendAllText(pathCSVFile, contentCSVFile);
                }
                catch (Exception)
                {
                    pathCSVFile = pathFolderForResults + "ConvergenceData_" + executionTimeStamp + counterCSVFile + ".csv";
                    while (File.Exists(pathCSVFile) == true)
                    {
                        pathCSVFile = pathFolderForResults + "ConvergenceData_" + executionTimeStamp + counterCSVFile++ + ".csv";
                    }
                    File.AppendAllText(pathCSVFile, contentCSVFile);
                }

                contentCSVFile = "";


                #endregion




                //Saving result as seialized object
                string jsonString = JsonSerializer.Serialize(ListStatsForCSV);

                int counter = 1;
                string fileName = pathFolderForResults + "ConvergenceData_" + counter + ".txt";
                while (File.Exists(fileName) == true)
                {
                    fileName = pathFolderForResults + "ConvergenceData_" + counter++ + ".txt";
                }

                File.WriteAllText(fileName, jsonString);
            }


























            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //If the user is running a control test to evaluate the optimization algos
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////
            if (optimizationExperienceToRun == OptimizationExperienceToRunType.ControlProcessTests)
            {
                usedMetaheuristicAlgorithmList = new List<IMHAlgorithm>();


                //Preparing the stats to be gathered
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ProcessMSE, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ProcessMCV, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ExecutionTime, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfTotalIteration, StatsToComputeType.Mean));
                StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ScoutBeesGeneratedCount, StatsToComputeType.Mean));


                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ProcessMSE, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ProcessMCV, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ExecutionTime, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.NumberOfTotalIteration, StatsToComputeType.Mean));
                csvFile_StatsToComputeList.Add(new Tuple<MHOptimizationResult, StatsToComputeType>(MHOptimizationResult.ScoutBeesGeneratedCount, StatsToComputeType.Mean));


                //Configuration for the NMPC controller
                int numberSamplesToEvaluate = 800; //Todo Process Number samples 800
                int numberRepetitionControlProcess = numberTestRepetition;//Todo Process Repetition 100
                int controlHorizonLength = 2;
                int predictionHorizonLength = 10;


                double[] referenceSignal = new double[numberSamplesToEvaluate];
                //Reference signal building
                for (int i = 0; i < numberSamplesToEvaluate; i++)
                {
                    switch (i)
                    {
                        case int n when (n <= 100):
                            referenceSignal[i] = 0.08;
                            break;
                        case int n when (100 < n && n <= 200):
                            referenceSignal[i] = 0.085;
                            break;
                        case int n when (200 < n && n <= 300):
                            referenceSignal[i] = 0.09;
                            break;
                        case int n when (300 < n && n <= 400):
                            referenceSignal[i] = 0.095;
                            break;
                        case int n when (400 < n && n <= 500):
                            referenceSignal[i] = 0.1;
                            break;
                        case int n when (500 < n && n <= 600):
                            referenceSignal[i] = 0.105;
                            break;
                        case int n when (600 < n && n <= 700):
                            referenceSignal[i] = 0.11;
                            break;
                        case int n when (700 < n && n <= 800):
                            referenceSignal[i] = 0.115;
                            break;
                        case int n when (800 < n && n <= 900):
                            referenceSignal[i] = 0.12;
                            break;
                        case int n when (900 < n && n <= 1000):
                            referenceSignal[i] = 0.125;
                            break;
                        case int n when (1000 < n && n <= 1100):
                            referenceSignal[i] = 0.13;
                            break;
                        case int n when (1100 < n && n <= 1200):
                            referenceSignal[i] = 0.135;
                            break;
                        case int n when (1200 < n && n <= 1300):
                            referenceSignal[i] = 0.14;
                            break;
                        default:
                            break;
                    }
                }







                //Defining the configuration
                MHConf.Where(x => x.Name == MHAlgoParameters.StopOptimizationWhenOptimumIsReached).First().Value = true;
                //MHConf.Where(x => x.Name == MHAlgoParameters.MaxItertaionNumber).First().Value = 1500;
                MHConf.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = 100;
                MHConf.Where(x => x.Name == MHAlgoParameters.FunctionValueSigmaTolerance).First().Value = 1e-16;
                MHConf.Where(x => x.Name == MHAlgoParameters.StoppingCriteriaType).First().Value = StoppingCriteriaType.MaximalNumberOfFunctionEvaluation;
                MHConf.Where(x => x.Name == MHAlgoParameters.ProblemDimension).First().Value = controlHorizonLength;
                MHConf.Where(x => x.Name == MHAlgoParameters.MaxFunctionEvaluationNumber).First().Value = 200;
                MHConf.Where(x => x.Name == MHAlgoParameters.PopulationSize).First().Value = 10;



                //ABC
                usedMetaheuristicAlgorithmList.Add(new DirectedABC(MHConf, "", randomGenerator.Next()));
                usedMetaheuristicAlgorithmList.Add(new BasicABC(MHConf, "", randomGenerator.Next()));
                usedMetaheuristicAlgorithmList.Add(new ImprovedABCAdaptiveMingZhao(MHConf, "", randomGenerator.Next()));
                usedMetaheuristicAlgorithmList.Add(new GBestABC(MHConf, "", randomGenerator.Next()));


                //MABC
                MHConf.Where(x => x.Name == MHAlgoParameters.MABC_LimitValue).First().Value = 200;
                MHConf.Where(x => x.Name == MHAlgoParameters.MABC_ModificationRate).First().Value = 0.4d;
                MHConf.Where(x => x.Name == MHAlgoParameters.MABC_UseScalingFactor).First().Value = true;
                usedMetaheuristicAlgorithmList.Add(new MABC(MHConf, "MABC Limit200 ScaFactor MR.4", randomGenerator.Next()));


                //ARABC
                MHConf.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = controlHorizonLength * (int)MHConf.Where(x => x.Name == MHAlgoParameters.PopulationSize).First().Value;
                usedMetaheuristicAlgorithmList.Add(new ARABC(MHConf, "", randomGenerator.Next()));
                MHConf.Where(x => x.Name == MHAlgoParameters.ABC_LimitValue).First().Value = 100;



                //AdaABC
                MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneNumberOfDimensionUsingGBest).First().Value = true;
                MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneScoutGenerationType).First().Value = true;
                MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_TuneProbabilityEquationType).First().Value = true;
                MHConf.Where(x => x.Name == MHAlgoParameters.ABC_ProbabilityEquationType).First().Value = ABC_ProbabilityEquationType.ComplementOriginal;
                MHConf.Where(x => x.Name == MHAlgoParameters.ScoutGeneration).First().Value = ScoutGenerationType.Random;
                MHConf.Where(x => x.Name == MHAlgoParameters.AEEABC_NumberOfIterationsToTuneParameters).First().Value = 5;
                usedMetaheuristicAlgorithmList.Add(new AdaABC(MHConf, "Proposed Algo", randomGenerator.Next()));





                //Adding the process
                controlProcessesToBenchmarkList.Add(new CSTR());


                //Starting the evaluation
                var tempresut = mainProgram.StartControlProcessEvaluation(usedMetaheuristicAlgorithmList, controlProcessesToBenchmarkList, referenceSignal, controlHorizonLength, predictionHorizonLength, numberRepetitionControlProcess, randomGenerator, runOneProcessOnlyFlag);
                GlobalResults.Add(tempresut);
                configurationTextDetailsList.Add(MHConf.MHConfig2String());






                //Saving datta into disk
                StatsForCSV = DiversExtendedProperties.ComputeStat(GlobalResults[GlobalResults.Count - 1], csvFile_StatsToComputeList, true, algoToIgnore, GroupByType.Algorithm, OrderingType.None);

                contentCSVFile += "Configuration" + Environment.NewLine + configurationTextDetailsList[0] + Environment.NewLine + "Results" + Environment.NewLine + Environment.NewLine;
                contentCSVFile += DiversExtendedProperties.FramtStatsCSVFile(StatsForCSV, csvFile_StatsToComputeList);

                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;


                SaveControlDataIntoDisk(tempresut, pathFolderForResults, configurationTextDetailsList, contentCSVFile, executionTimeStamp);





                string jsonString;
                string fileName;


                jsonString = JsonSerializer.Serialize(tempresut);

                int counter = 1;
                fileName = pathFolderForResults + "ResultProcessData" + counter + ".txt";
                while (File.Exists(fileName) == true)
                {
                    fileName = pathFolderForResults + "ResultProcessData" + counter++ + ".txt";
                }

                File.WriteAllText(fileName, jsonString);







            }





            Console.WriteLine("");
        }

        private static void SaveControlDataIntoDisk(List<GlobalBatchResultModel>? tempresut, string pathFolderForResults, List<string> configurationTextDetailsList, string contentCSVFile, string executionTimeStamp)
        {

            contentCSVFile += Environment.NewLine;
            contentCSVFile += Environment.NewLine;
            contentCSVFile += Environment.NewLine;
            contentCSVFile += Environment.NewLine;
            contentCSVFile += Environment.NewLine;
            contentCSVFile += "Configuration" + Environment.NewLine + configurationTextDetailsList[0] + Environment.NewLine + "Results" + Environment.NewLine + Environment.NewLine;

            contentCSVFile += Environment.NewLine;
            contentCSVFile += Environment.NewLine;
            contentCSVFile += Environment.NewLine;
            contentCSVFile += Environment.NewLine;
            contentCSVFile += Environment.NewLine;

            List<int> mHOptimizationAlgorithmList = new();
            List<int> benchmarkFunctionList = new();
            List<GlobalBatchResultModel> filteredGlobalBatchResults;


            //Retrieve the different instance of optimization algorithms
            //the 'results' may contains optimization results of different optimization processes

            foreach (var resultItem in tempresut)
            {
                if (mHOptimizationAlgorithmList.Contains(resultItem.MHAlgorithm.InstanceID) == false)
                {
                    mHOptimizationAlgorithmList.Add(resultItem.MHAlgorithm.InstanceID);
                }

                if (benchmarkFunctionList.Contains(resultItem.BenchmarkFunction.ParentInstanceID) == false)
                {
                    benchmarkFunctionList.Add(resultItem.BenchmarkFunction.ParentInstanceID);
                }
            }

            int numberOfMHAlgos = mHOptimizationAlgorithmList.Count;


            List<List<String>> tempList = new();
            List<String> tempSubList = new();

            //Browse through all MH Algos
            for (int iCounter = 0; iCounter < numberOfMHAlgos; iCounter++)
            {
                int MH_InstanceID = mHOptimizationAlgorithmList[iCounter];
                tempSubList = new();
                tempList.Add(tempSubList);

                filteredGlobalBatchResults = new();
                filteredGlobalBatchResults = tempresut.Where(x => x.MHAlgorithm.InstanceID == MH_InstanceID).ToList();
                filteredGlobalBatchResults = filteredGlobalBatchResults.OrderBy(resultItem => resultItem.RepetitionID).ToList();





                int intIndex = 0;
                List<String> constructedCSVFile = new();

                foreach (var item in filteredGlobalBatchResults)
                {
                    intIndex = 0;

                    List<double> commandList = ((List<double>)item.OptimizationResults.Where(x => x.Name == MHOptimizationResult.ProcessCommandList).First().Value);
                    List<double> ActualProcessOutputList = ((List<double>)item.OptimizationResults.Where(x => x.Name == MHOptimizationResult.ProcessActualOutputsList).First().Value);
                    string varString;
                    constructedCSVFile.Add("Repetition " + item.RepetitionID);

                    for (int i = 0; i < commandList.Count; i++)
                    {
                        if (i < tempSubList.Count)
                        {
                            varString = tempSubList[i];
                            varString += "," + commandList[i] + "," + ActualProcessOutputList[i] + ",,,";
                            tempSubList[i] = varString;
                        }
                        else
                        {
                            varString = "";
                            varString += "," + commandList[i] + "," + ActualProcessOutputList[i] + ",,,";
                            tempSubList.Add(varString);
                        }



                    }



                }


            }


            //Saving all data in single files
            int indexint = 0;

            foreach (var item in tempList)
            {
                contentCSVFile += tempresut.Where(x => x.MHAlgorithm.InstanceID == mHOptimizationAlgorithmList[indexint]).First().MHAlgorithm.Name + ": " + mHOptimizationAlgorithmList[indexint];
                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;



                foreach (var subitem in item)
                {
                    contentCSVFile += subitem + Environment.NewLine;
                }

                contentCSVFile += Environment.NewLine;
                contentCSVFile += Environment.NewLine;

                indexint++;
            }




            int counterCSVFile = 1;
            try
            {
                File.AppendAllText(pathFolderForResults + "ControlDataResult" + executionTimeStamp + ".csv", contentCSVFile);
            }
            catch (Exception)
            {
                pathFolderForResults = pathFolderForResults + "ControlDataResult" + executionTimeStamp + counterCSVFile + ".csv";
                while (File.Exists(pathFolderForResults) == true)
                {
                    pathFolderForResults = pathFolderForResults + "ControlDataResult" + executionTimeStamp + counterCSVFile++ + ".csv";
                }
                File.AppendAllText(pathFolderForResults, contentCSVFile);
            }



        }

        private static List<IMHAlgorithm> LoadMetaheuristicOptimizationAlgorithms(List<OptimizationParameter> mHConf, Random randomGenerator, List<string> algorithmNameToIgnoreList, List<string> algorithmNameToIncludeList)
        {
            Assembly ass = System.Reflection.Assembly.GetEntryAssembly();
            List<IMHAlgorithm> MetaHeuristicAlgorithmList = new();
            List<string> AvailableMetaheuristiqueAlgosNamesList = new List<string>();

            foreach (System.Reflection.TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IMHAlgorithm)))
                {
                    if (algorithmNameToIgnoreList.Any(x => ti.FullName.Contains(x) == false) == true)
                    {
                        if (algorithmNameToIncludeList.Count == 0 || algorithmNameToIncludeList.Any(x => ti.FullName.Contains(x) == true) == true)
                        {
                            AvailableMetaheuristiqueAlgosNamesList.Add(ti.FullName);
                        }
                    }
                }
            }

            AvailableMetaheuristiqueAlgosNamesList.Sort();


            foreach (string algorithmNameItem in AvailableMetaheuristiqueAlgosNamesList)
            {
                object? createdInstance = null;
                createdInstance = ass.CreateInstance(algorithmNameItem);

                if (createdInstance != null)
                {
                    MetaHeuristicAlgorithmList.Add((IMHAlgorithm)createdInstance);

                    MetaHeuristicAlgorithmList.Last().MakePersonalOptimizationConfigurationListCopy(mHConf, "", randomGenerator.Next());
                }
            }


            return MetaHeuristicAlgorithmList;
        }

        private static List<IBenchmark> LoadBenchmarkFunctions(List<string> benchmarkNameToIgnoreList, List<string> benchmarkFunctionNameToIncludeList)
        {
            List<IBenchmark> usedBenchmarkFunctionList = new();
            List<string> AvailableBenchmarkFunctionsNamesList = new List<string>();
            Assembly ass;

            ass = System.Reflection.Assembly.GetEntryAssembly();
            foreach (System.Reflection.TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IBenchmark)))
                {
                    if (benchmarkNameToIgnoreList.Where(x => ti.FullName.Contains(x) == true).Count() == 0)
                    {
                        if (benchmarkFunctionNameToIncludeList.Count == 0 || benchmarkFunctionNameToIncludeList.Any(x => ti.FullName.Contains(x) == true) == true)
                        {
                            AvailableBenchmarkFunctionsNamesList.Add(ti.FullName);
                        }
                    }
                }
            }

            AvailableBenchmarkFunctionsNamesList.Sort();



            foreach (string benchmarkFunctionItem in AvailableBenchmarkFunctionsNamesList)
            {
                object? createdInstance = null;
                createdInstance = ass.CreateInstance(benchmarkFunctionItem);

                if (createdInstance != null)
                {
                    usedBenchmarkFunctionList.Add((IBenchmark)createdInstance);
                }
            }

            return usedBenchmarkFunctionList;
        }

        private static List<Tuple<string, List<double>>> ComputeAndDisplayStats(List<GlobalBatchResultModel> GlobalResults, List<string> algoToIgnore, List<Tuple<MHOptimizationResult, StatsToComputeType>> StatsToComputeList)
        {
            List<Tuple<string, List<double>>> StatsByMHAlgoAndBenchFunc = new();



            StatsByMHAlgoAndBenchFunc = DiversExtendedProperties.ComputeStat(GlobalResults, StatsToComputeList, true, algoToIgnore, GroupByType.BenchmarkFunction, OrderingType.Ascending);


            Console.WriteLine(DiversExtendedProperties.FormatStatsResults(StatsByMHAlgoAndBenchFunc, StatsToComputeList));

            return StatsByMHAlgoAndBenchFunc;
        }

        private string ToString(List<OptimizationParameter> parameters)
        {

            return "";
        }

        private List<OptimizationParameter> DoOptimizationProcessConfiguration()
        {
            //Constructing the differents configuration
            List<OptimizationParameter> MHConf = new List<OptimizationParameter>();


            OptimizationParameter populationSizeParameter = new OptimizationParameter(Divers.MHAlgoParameters.PopulationSize, 40, true);
            OptimizationParameter stoppingCriteriaTypeParameter = new OptimizationParameter(Divers.MHAlgoParameters.StoppingCriteriaType, StoppingCriteriaType.MaximalNumberOfIteration);
            OptimizationParameter maxFunctionEvaluationNumberParameter = new OptimizationParameter(Divers.MHAlgoParameters.MaxFunctionEvaluationNumber, 200000);
            OptimizationParameter maxItertaionNumberParameter = new OptimizationParameter(Divers.MHAlgoParameters.MaxItertaionNumber, 1500, true);
            OptimizationParameter ProblemDimensionParameter = new OptimizationParameter(Divers.MHAlgoParameters.ProblemDimension, 30, true);
            OptimizationParameter OptimizationTypeParameter = new OptimizationParameter(Divers.MHAlgoParameters.OptimizationType, OptimizationProblemType.Minimization);
            OptimizationParameter PopulationInitilizationParameter = new OptimizationParameter(Divers.MHAlgoParameters.PopulationInitilization, PopulationInitilizationType.Random);
            OptimizationParameter ABC_LimitValueParameter = new OptimizationParameter(Divers.MHAlgoParameters.ABC_LimitValue, 100);
            OptimizationParameter MABC_ModificationRateParameter = new OptimizationParameter(Divers.MHAlgoParameters.MABC_ModificationRate, 0.4);
            OptimizationParameter MABC_UseScalingFactorParameter = new OptimizationParameter(Divers.MHAlgoParameters.MABC_UseScalingFactor, true);
            OptimizationParameter MABC_LimitValuerParameter = new OptimizationParameter(Divers.MHAlgoParameters.MABC_LimitValue, 200);
            OptimizationParameter FunctionValueSigmaToleranceParameter = new OptimizationParameter(Divers.MHAlgoParameters.FunctionValueSigmaTolerance, 1e-100);
            OptimizationParameter ShiftObjectiveFunctionOptimumValueToZeroParameter = new OptimizationParameter(Divers.MHAlgoParameters.ShiftObjectiveFunctionOptimumValueToZero, false);
            OptimizationParameter StopOptimizationWhenOptimumIsReachedParameter = new OptimizationParameter(Divers.MHAlgoParameters.StopOptimizationWhenOptimumIsReached, true);
            OptimizationParameter AEEABC_NumberOfIterationsToTuneParametersParameter = new OptimizationParameter(Divers.MHAlgoParameters.AEEABC_NumberOfIterationsToTuneParameters, 5);
            OptimizationParameter AEEABC_TuneNumberOfDimensionUsingGBestParameter = new OptimizationParameter(Divers.MHAlgoParameters.AEEABC_TuneNumberOfDimensionUsingGBest, false);
            OptimizationParameter scoutGenerationParameter = new OptimizationParameter(Divers.MHAlgoParameters.ScoutGeneration, ScoutGenerationType.Random);
            OptimizationParameter ABC_ProbabilityEquationTypeParameter = new OptimizationParameter(Divers.MHAlgoParameters.ABC_ProbabilityEquationType, ABC_ProbabilityEquationType.Original);
            OptimizationParameter AEEABC_TuneScoutGenerationTypeParameter = new OptimizationParameter(Divers.MHAlgoParameters.AEEABC_TuneScoutGenerationType, false);
            OptimizationParameter AEEABC_TuneProbabilityEquationTypeParameter = new OptimizationParameter(Divers.MHAlgoParameters.AEEABC_TuneProbabilityEquationType, false);




            MHConf.Add(populationSizeParameter);
            MHConf.Add(stoppingCriteriaTypeParameter);
            MHConf.Add(maxFunctionEvaluationNumberParameter);
            MHConf.Add(maxItertaionNumberParameter);
            MHConf.Add(ProblemDimensionParameter);
            MHConf.Add(OptimizationTypeParameter);
            MHConf.Add(PopulationInitilizationParameter);
            MHConf.Add(ABC_LimitValueParameter);
            MHConf.Add(MABC_ModificationRateParameter);
            MHConf.Add(MABC_UseScalingFactorParameter);
            MHConf.Add(MABC_LimitValuerParameter);
            MHConf.Add(FunctionValueSigmaToleranceParameter);
            MHConf.Add(ShiftObjectiveFunctionOptimumValueToZeroParameter);
            MHConf.Add(StopOptimizationWhenOptimumIsReachedParameter);
            MHConf.Add(AEEABC_NumberOfIterationsToTuneParametersParameter);
            MHConf.Add(AEEABC_TuneNumberOfDimensionUsingGBestParameter);
            MHConf.Add(scoutGenerationParameter);
            MHConf.Add(ABC_ProbabilityEquationTypeParameter);
            MHConf.Add(AEEABC_TuneScoutGenerationTypeParameter);
            MHConf.Add(AEEABC_TuneProbabilityEquationTypeParameter);


            return MHConf;
        }

        private List<GlobalBatchResultModel>? StartControlProcessEvaluation(List<IMHAlgorithm> metaheuristicAlgorithmList, List<IControlProcess> controlProcessesToBenchmarkList, double[] referenceSignal, int controlHorizonLength, int predictionHorizonLength, int numberRepetitionControlProcess, Random randomGenerator, bool RunOnlyOneProcess = false)
        {
            //NOT TESTED FOR MULTI THREADING FUNCTIONS
            RunOnlyOneProcess = true;

            List<GlobalBatchResultModel>? GlobalResults = new List<GlobalBatchResultModel>();
            List<int> randomGeneratedIntegerList;
            Assembly ass = System.Reflection.Assembly.GetEntryAssembly();
            object resultLock = new object();
            int numberOfCompltedOptimizationProcesses = 0;
            bool processStoppedPrematurly = false;
            string processStoppedPrematurlyErrorMessage = "";
            GlobalBatchResultModel currentResul = new GlobalBatchResultModel();
            int intCounter = 0;

            //COmputing the number of optimization processes to run
            int numberTotalOptimizationProcessesToRun = metaheuristicAlgorithmList.Count * controlProcessesToBenchmarkList.Count * numberRepetitionControlProcess;
            var watch = new System.Diagnostics.Stopwatch();

            foreach (var MHAlgoItem in metaheuristicAlgorithmList)
            {//Loop for all MH algorithm to be tested
                foreach (var ControlProcessItem in controlProcessesToBenchmarkList)
                {//Loop through all control processes to benchmark
                    string? controlProcessTypeName = ControlProcessItem.GetType().FullName;

                    //Call this method to prevent computerfrom sleeping during execution
                    NativeMethods.SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);

                    ParallelOptions parallelOptions = new ParallelOptions();
                    if (RunOnlyOneProcess == true)
                    {
                        parallelOptions.MaxDegreeOfParallelism = 1;
                    }
                    randomGenerator = new Random();

                    //Generating random numbers to be used as seed for the optimization algorithms
                    int randomIntValue;
                    randomGeneratedIntegerList = new List<int>();
                    for (int i = 0; i < numberTotalOptimizationProcessesToRun * referenceSignal.Length; i++)
                    {
                        randomGeneratedIntegerList.Add(randomGenerator.Next());
                    }

                    List<OptimizationResultModel> resultsData = new();
                    List<OptimizationResultModel> resultProcessData = new();


                    ParallelLoopResult resultLoopStatus = Parallel.For(0, numberRepetitionControlProcess, parallelOptions, (RepetitionID, state) =>
                    {
                        resultsData = new();
                        IControlProcess localControlProcess;
                        object? createdInstance;
                        int tempInt;

                        try
                        {
                            createdInstance = ass.CreateInstance(controlProcessTypeName);
                        }
                        catch (Exception)
                        {
                            createdInstance = null;
                        }

                        if (createdInstance != null)
                        {
                            localControlProcess = (IControlProcess)createdInstance;
                            localControlProcess.ParentInstanceID = ControlProcessItem.ParentInstanceID;
                            localControlProcess.MaxProblemDimension = (short)controlHorizonLength;
                            localControlProcess.MinProblemDimension = (short)controlHorizonLength;
                            localControlProcess.ControlHorizonLength = (short)controlHorizonLength;


                            //List<List<double>> processStatus = new();
                            List<double> processSingleDataStatus = new();
                            List<double> appliedCommandList = new();
                            List<double> actualProcessOutputList = new();
                            double[,]? initialSolutions = null;
                            ;
                            double MSE_Error = 0;
                            double MCV_Criterion = 0;
                            int numberOfGeneratedScoutBees = 0;
                            int numberOfiterationsPefromed = 0;

                            watch.Restart();

                            //Browsing through the samples
                            for (int controlSampleID = 0; controlSampleID < referenceSignal.Length - predictionHorizonLength; controlSampleID++)
                            {
                                Interlocked.Add(ref intCounter, 1);

                                lock (resultLock)
                                {
                                    randomIntValue = randomGeneratedIntegerList[intCounter];
                                }


                                //Repeat the same scenario 
                                resultsData = new();
                                processSingleDataStatus = new();


                                //Setting the reference signal for the prediction horizon
                                localControlProcess.Reference = referenceSignal.Where((number, index) => controlSampleID <= index && index < (controlSampleID + predictionHorizonLength)).ToArray();

                                //Setting the current sample ID
                                localControlProcess.CurrentSampleID = controlSampleID + 1;


                                try
                                {
                                    MHAlgoItem.ComputeOptimum((IBenchmark)localControlProcess, resultsData, randomIntValue, initialSolutions);
                                }
                                catch (Exception ex)
                                {
                                    processStoppedPrematurly = true;
                                    processStoppedPrematurlyErrorMessage = ex.ToString();
                                    state.Stop();
                                }

                                //Retrieving the optimal command found by the optimization algorithm
                                var tempOptimalCommand = resultsData.Where(res => res.Name == MHOptimizationResult.OptimalPoint).First().Value;

                                //Retrieving the cost of the optimal command found by the optimization algorithm
                                var CostOptimalCommand = resultsData.Where(res => res.Name == MHOptimizationResult.OptimalFunctionValue).First().Value;

                                //Retrieving the number of scout bees generated through the optimization process for all samples
                                numberOfGeneratedScoutBees += (int)resultsData.Where(res => res.Name == MHOptimizationResult.ScoutBeesGeneratedCount).First().Value;

                                //Retrieving the number of optimization itertaions required through the optimization process for all samples
                                numberOfiterationsPefromed += (int)resultsData.Where(res => res.Name == MHOptimizationResult.NumberOfTotalIteration).First().Value;


                                double[] optimalCommand = (double[])tempOptimalCommand;

                                watch.Stop();

                                //Applying the optimal command found to the process and computing the actual output using the process ODE
                                //List<double> TESTFIXEDCOMMAND = new() { 108.1 };
                                double actualProcessOutput = localControlProcess.ComputeActualProcessOutput(optimalCommand);
                                //double actualProcessOutput = localControlProcess.ComputeActualProcessOutput(TESTFIXEDCOMMAND.ToArray());

                                watch.Start();

                                //Saving process status data
                                appliedCommandList.Add(optimalCommand[0]);
                                actualProcessOutputList.Add(actualProcessOutput);
                                localControlProcess.UpdatePreviousPreviousStatus(optimalCommand[0], actualProcessOutput);

                                //adding current optimal solution in the population for the next sampling period                               
                                initialSolutions = new double[1, 2] { { optimalCommand[0], optimalCommand[1] } };

                                //Computing MSE   & MCU                                                            
                                MSE_Error += Math.Pow(actualProcessOutput - referenceSignal[controlSampleID], 2);
                                MCV_Criterion += (double)CostOptimalCommand;

                                Console.WriteLine(intCounter + "/" + (numberTotalOptimizationProcessesToRun * (referenceSignal.Length - predictionHorizonLength)).ToString() + "  || Echantillon " + controlSampleID + "/" + (referenceSignal.Length - predictionHorizonLength).ToString());
                            }


                            //Computing the MSE
                            MSE_Error = MSE_Error / (referenceSignal.Length - predictionHorizonLength);
                            MCV_Criterion = MCV_Criterion / (referenceSignal.Length - predictionHorizonLength);

                            ///////////////////////////////////////////////////
                            //////OptimalFunctionValue=> contains the MSE//////////////////////////
                            //////SuccessfullMutationRate=> contains the MCU//////////////////////////
                            ///////////////////////////////////////////////////
                            resultProcessData.Add(new OptimizationResultModel(MHOptimizationResult.ProcessMSE, MSE_Error));
                            resultProcessData.Add(new OptimizationResultModel(MHOptimizationResult.ProcessMCV, MCV_Criterion));
                            resultProcessData.Add(new OptimizationResultModel(MHOptimizationResult.ExecutionTime, watch.ElapsedMilliseconds));
                            resultProcessData.Add(new OptimizationResultModel(MHOptimizationResult.ProcessCommandList, appliedCommandList));
                            resultProcessData.Add(new OptimizationResultModel(MHOptimizationResult.ProcessActualOutputsList, actualProcessOutputList));
                            resultProcessData.Add(new OptimizationResultModel(MHOptimizationResult.ScoutBeesGeneratedCount, numberOfGeneratedScoutBees));
                            resultProcessData.Add(new OptimizationResultModel(MHOptimizationResult.NumberOfTotalIteration, numberOfiterationsPefromed));


                            currentResul = new GlobalBatchResultModel(MHAlgoItem, (IBenchmark)ControlProcessItem, RepetitionID, resultProcessData);
                            resultProcessData = new();

                            Interlocked.Add(ref numberOfCompltedOptimizationProcesses, 1);


                            if (processStoppedPrematurly == false)
                                lock (resultLock)
                                {
                                    #region Preparing status to be printed on screen
                                    string currentStatusText = "";
                                    StoppingCriteriaType stoppingCriteriaType = (StoppingCriteriaType)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.StoppingCriteriaType).First().Value;
                                    int currentUsedDimension;
                                    int benchmarkFunctionMaxDimension = ControlProcessItem.MaxProblemDimension;
                                    int benchmarkFunctionMinDimension = ControlProcessItem.MinProblemDimension;
                                    int currentRequiredDimension = (int)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ProblemDimension).First().Value;

                                    if (currentRequiredDimension > benchmarkFunctionMaxDimension)
                                    {
                                        currentUsedDimension = benchmarkFunctionMaxDimension;
                                    }
                                    else if (currentRequiredDimension < benchmarkFunctionMinDimension)
                                    {
                                        currentUsedDimension = benchmarkFunctionMinDimension;
                                    }
                                    else
                                    {
                                        currentUsedDimension = currentRequiredDimension;
                                    }

                                    currentStatusText += MHAlgoItem.Name.ShortenNameString();
                                    currentStatusText += $"({MHAlgoItem.Description.ShortenNameString()})||";
                                    currentStatusText += ControlProcessItem.Name.ShortenNameString() + "||";
                                    //currentStatusText += "Pop : " + (int)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.PopulationSize).First().Value;
                                    //if (stoppingCriteriaType == StoppingCriteriaType.MaximalNumberOfIteration)
                                    //{
                                    //    currentStatusText += "||MaxIter : " + (int)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.MaxItertaionNumber).First().Value;
                                    //}
                                    //else if (stoppingCriteriaType == StoppingCriteriaType.MaximalNumberOfFunctionEvaluation)
                                    //{
                                    //    currentStatusText += "||MaxFuncEval : " + (int)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.MaxFunctionEvaluationNumber).First().Value;
                                    //}
                                    currentStatusText += "||Dim : " + currentUsedDimension.ToString();

                                    currentStatusText += "||" + (RepetitionID + 1).ToString("D" + numberRepetitionControlProcess.ToString().Length) + "/" + numberRepetitionControlProcess.ToString("D" + numberRepetitionControlProcess.ToString().Length);
                                    currentStatusText += "||FinalIterNum : " + resultsData.First(x => x.Name == MHOptimizationResult.NumberOfTotalIteration).Value;
                                    currentStatusText += "||FinalNbreFuncEval : " + resultsData.First(x => x.Name == MHOptimizationResult.NumberOfFunctionEvaluation).Value;
                                    currentStatusText += "||Time : " + resultsData.First(x => x.Name == MHOptimizationResult.ExecutionTime).Value;
                                    currentStatusText += "||OptValue : " + resultsData.First(x => x.Name == MHOptimizationResult.OptimalFunctionValue).Value;
                                    currentStatusText += "||Progress: " + numberOfCompltedOptimizationProcesses.ToString("D" + numberTotalOptimizationProcessesToRun.ToString().Length) + "/" + numberTotalOptimizationProcessesToRun.ToString("D" + numberTotalOptimizationProcessesToRun.ToString().Length);
                                    #endregion

                                    GlobalResults.Add(currentResul);
                                    Console.WriteLine(currentStatusText);
                                }
                        }






                    });


                    if (processStoppedPrematurly == true)
                    {
                        break;
                        // GlobalResults.Clear();
                        Console.WriteLine("A problem has been detected." + Environment.NewLine + "Optimization process has been halted. Please correct the issue first");
                        Console.WriteLine(processStoppedPrematurlyErrorMessage);
                        return null;
                    }
                }
                if (processStoppedPrematurly == true)
                {
                    break;
                    // GlobalResults.Clear();
                    Console.WriteLine("A problem has been detected." + Environment.NewLine + "Optimization process has been halted. Please correct the issue first");
                    Console.WriteLine(processStoppedPrematurlyErrorMessage);
                    return null;
                }
            }


            if (processStoppedPrematurly == true)
            {
                //GlobalResults.Clear();
                Console.WriteLine("A problem has been detected." + Environment.NewLine + "Optimization process has been halted. Please correct the issue first");
                Console.WriteLine(processStoppedPrematurlyErrorMessage);
                return null;
            }

            return GlobalResults;
        }

        private List<GlobalBatchResultModel>? StartOptimizationProcess(List<IMHAlgorithm> metaheuristicAlgorithmList, List<IBenchmark> usedBenchmarkFunctionList, int NumberRepetition, Random randomGenerator, bool RunOnlyOneProcess = false)
        {
            List<GlobalBatchResultModel>? GlobalResults = new List<GlobalBatchResultModel>();
            List<int> randomGeneratedIntegerList;
            Assembly ass = System.Reflection.Assembly.GetEntryAssembly();
            object resultLock = new object();
            int numberOfCompltedOptimizationProcesses = 0;
            bool processStoppedPrematurly = false;
            string processStoppedPrematurlyErrorMessage = "";
            GlobalBatchResultModel currentResul = new GlobalBatchResultModel();

            //COmputing the number of optimization processes to run
            int numberTotalOptimizationProcessesToRun = metaheuristicAlgorithmList.Count * usedBenchmarkFunctionList.Count * NumberRepetition;

            foreach (var MHAlgoItem in metaheuristicAlgorithmList)
            {//Loop for all MH algorithm to be tested
                foreach (var BenchmarkFuntionItem in usedBenchmarkFunctionList)
                {//Loop through all benchmark functions
                    string? benchmarkFunctionTypeName = BenchmarkFuntionItem.GetType().FullName;

                    //Call this method to prevent computerfrom sleeping during execution
                    NativeMethods.SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);

                    ParallelOptions parallelOptions = new ParallelOptions();
                    if (RunOnlyOneProcess == true)
                    {
                        parallelOptions.MaxDegreeOfParallelism = 1;
                    }
                    randomGenerator = new Random();

                    //Generating random numbers to be used as seed for the optimization algorithms
                    int randomIntValue;
                    randomGeneratedIntegerList = new List<int>();
                    for (int i = 0; i < NumberRepetition; i++)
                    {
                        randomGeneratedIntegerList.Add(randomGenerator.Next());
                    }

                    ParallelLoopResult resultLoopStatus = Parallel.For(0, NumberRepetition, parallelOptions, (RepetitionID, state) =>
                    {

                        //Repeat the same scenario 
                        List<OptimizationResultModel> resultsData = new();

                        IBenchmark localBenchmarkFunction;
                        object? createdInstance;
                        int tempInt;

                        try
                        {
                            createdInstance = ass.CreateInstance(benchmarkFunctionTypeName);
                        }
                        catch (Exception)
                        {
                            createdInstance = null;
                        }

                        if (createdInstance != null)
                        {
                            localBenchmarkFunction = (IBenchmark)createdInstance;
                            localBenchmarkFunction.ParentInstanceID = BenchmarkFuntionItem.ParentInstanceID;

                            lock (resultLock)
                            {
                                randomIntValue = randomGeneratedIntegerList[RepetitionID];
                            }

                            if (numberOfCompltedOptimizationProcesses == 1600)
                            {
                                tempInt = 2;
                            }

                            try
                            {
                                MHAlgoItem.ComputeOptimum(localBenchmarkFunction, resultsData, randomIntValue);
                            }
                            catch (Exception ex)
                            {
                                processStoppedPrematurly = true;
                                processStoppedPrematurlyErrorMessage = ex.ToString();
                                state.Stop();
                            }
                            Interlocked.Add(ref numberOfCompltedOptimizationProcesses, 1);
                            currentResul = new GlobalBatchResultModel(MHAlgoItem, localBenchmarkFunction, RepetitionID, resultsData);


                            if (processStoppedPrematurly == false)
                                lock (resultLock)
                                {
                                    #region Preparing status to be printed on screen
                                    string currentStatusText = "";
                                    StoppingCriteriaType stoppingCriteriaType = (StoppingCriteriaType)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.StoppingCriteriaType).First().Value;
                                    int currentUsedDimension;
                                    int benchmarkFunctionMaxDimension = localBenchmarkFunction.MaxProblemDimension;
                                    int benchmarkFunctionMinDimension = localBenchmarkFunction.MinProblemDimension;
                                    int currentRequiredDimension = (int)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.ProblemDimension).First().Value;

                                    if (currentRequiredDimension > benchmarkFunctionMaxDimension)
                                    {
                                        currentUsedDimension = benchmarkFunctionMaxDimension;
                                    }
                                    else if (currentRequiredDimension < benchmarkFunctionMinDimension)
                                    {
                                        currentUsedDimension = benchmarkFunctionMinDimension;
                                    }
                                    else
                                    {
                                        currentUsedDimension = currentRequiredDimension;
                                    }

                                    currentStatusText += MHAlgoItem.Name.ShortenNameString();
                                    currentStatusText += $"({MHAlgoItem.Description.ShortenNameString()})||";
                                    currentStatusText += localBenchmarkFunction.Name.ShortenNameString() + "||";
                                    //currentStatusText += "Pop : " + (int)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.PopulationSize).First().Value;
                                    //if (stoppingCriteriaType == StoppingCriteriaType.MaximalNumberOfIteration)
                                    //{
                                    //    currentStatusText += "||MaxIter : " + (int)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.MaxItertaionNumber).First().Value;
                                    //}
                                    //else if (stoppingCriteriaType == StoppingCriteriaType.MaximalNumberOfFunctionEvaluation)
                                    //{
                                    //    currentStatusText += "||MaxFuncEval : " + (int)MHAlgoItem.OptimizationConfiguration.Where(x => x.Name == MHAlgoParameters.MaxFunctionEvaluationNumber).First().Value;
                                    //}
                                    currentStatusText += "||Dim : " + currentUsedDimension.ToString();

                                    currentStatusText += "||" + (RepetitionID + 1).ToString("D" + NumberRepetition.ToString().Length) + "/" + NumberRepetition.ToString("D" + NumberRepetition.ToString().Length);
                                    currentStatusText += "||FinalIterNum : " + resultsData.First(x => x.Name == MHOptimizationResult.NumberOfTotalIteration).Value;
                                    currentStatusText += "||FinalNbreFuncEval : " + resultsData.First(x => x.Name == MHOptimizationResult.NumberOfFunctionEvaluation).Value;
                                    currentStatusText += "||Time : " + resultsData.First(x => x.Name == MHOptimizationResult.ExecutionTime).Value;
                                    currentStatusText += "||OptValue : " + resultsData.First(x => x.Name == MHOptimizationResult.OptimalFunctionValue).Value;
                                    currentStatusText += "||Progress: " + numberOfCompltedOptimizationProcesses.ToString("D" + numberTotalOptimizationProcessesToRun.ToString().Length) + "/" + numberTotalOptimizationProcessesToRun.ToString("D" + numberTotalOptimizationProcessesToRun.ToString().Length);
                                    #endregion

                                    GlobalResults.Add(currentResul);
                                    Console.WriteLine(currentStatusText);
                                }
                        }

                    });

                    if (processStoppedPrematurly == true)
                    {
                        break;
                        // GlobalResults.Clear();
                        Console.WriteLine("A problem has been detected." + Environment.NewLine + "Optimization process has been halted. Please correct the issue first");
                        Console.WriteLine(processStoppedPrematurlyErrorMessage);
                        return null;
                    }
                }
                if (processStoppedPrematurly == true)
                {
                    break;
                    // GlobalResults.Clear();
                    Console.WriteLine("A problem has been detected." + Environment.NewLine + "Optimization process has been halted. Please correct the issue first");
                    Console.WriteLine(processStoppedPrematurlyErrorMessage);
                    return null;
                }
            }


            if (processStoppedPrematurly == true)
            {
                //GlobalResults.Clear();
                Console.WriteLine("A problem has been detected." + Environment.NewLine + "Optimization process has been halted. Please correct the issue first");
                Console.WriteLine(processStoppedPrematurlyErrorMessage);
                return null;
            }

            return GlobalResults;
        }

        private static void FormateResultsForPaper(List<List<Tuple<string, List<double>>>>? statsByMHAlgoAndBenchFuncDESERIALIZE, string dateForFolderName)
        {
            string formattedCSVOutputFileMeanSTD = "";
            string formattedCSVOutputFileMean = "";
            string formattedCSVOutputFileFENMean = "";
            string line1 = "", line2 = "", line3 = "";
            string line10 = "", line20 = "", line30 = "";
            List<Tuple<string, List<double>>> batchResultLIST = new(70);
            int index = 0;


            //Loop for the benchmark with stadard function (dimension varable)
            string[] linesMeanSTD = new string[72];
            string[] linesMean = new string[72];
            string[] NumberFunctionEvaluationMean = new string[72];
            index = 0;
            int indexMemory = 0;
            int indexMemoryFixedDim = 0;

            var batchResults1 = statsByMHAlgoAndBenchFuncDESERIALIZE[0];
            var batchResults2 = statsByMHAlgoAndBenchFuncDESERIALIZE[1];
            var batchResults3 = statsByMHAlgoAndBenchFuncDESERIALIZE[2];

            for (int j = 0; j < batchResults1.Count; j++)
            {
                var singleResult1 = batchResults1[j];
                var singleResult2 = batchResults2[j];
                var singleResult3 = batchResults3[j];

                if (singleResult1 == null)
                {
                    index = 0;
                }
                else
                {
                    linesMeanSTD[index * 3] += singleResult1.Item2[0] + "," + singleResult1.Item2[3] + ",";
                    linesMeanSTD[index * 3 + 1] += singleResult2.Item2[0] + "," + singleResult2.Item2[3] + ",";
                    linesMeanSTD[index * 3 + 2] += singleResult3.Item2[0] + "," + singleResult3.Item2[3] + ",";

                    linesMean[index * 3] += singleResult1.Item2[0] + ",";
                    linesMean[index * 3 + 1] += singleResult2.Item2[0] + ",";
                    linesMean[index * 3 + 2] += singleResult3.Item2[0] + ",";

                    //NumberFunctionEvaluationMean[index * 3] += "TBA,";
                    //NumberFunctionEvaluationMean[index * 3 + 1] += singleResult1.Item2[0] + ",";
                    //NumberFunctionEvaluationMean[index * 3 + 2] += singleResult2.Item2[0] + ",";

                    //NumberFunctionEvaluationMean[index * 3] += singleResult1.Item2[5] + ",";
                    //NumberFunctionEvaluationMean[index * 3 + 1] += singleResult2.Item2[5] + ",";
                    //NumberFunctionEvaluationMean[index * 3 + 2] += singleResult3.Item2[5] + ",";

                    index++;
                    if (indexMemory < index)
                    {
                        indexMemory = index;
                    }
                }

            }


            indexMemory = (indexMemory - 1) * 3 + 2;

            //Fixed dimension benchmark
            batchResults1 = statsByMHAlgoAndBenchFuncDESERIALIZE[statsByMHAlgoAndBenchFuncDESERIALIZE.Count - 2];
            index = indexMemory;

            for (int j = 0; j < batchResults1.Count; j++)
            {
                var singleResult1 = batchResults1[j];

                if (singleResult1 == null)
                {
                    index = indexMemory;
                }
                else
                {
                    linesMeanSTD[index + 1] += singleResult1.Item2[0] + "," + singleResult1.Item2[3] + ",";
                    linesMean[index + 1] += singleResult1.Item2[0] + ",";
                    NumberFunctionEvaluationMean[index + 1] += singleResult1.Item2[0] + ",";

                    index++;
                    if (indexMemoryFixedDim < index)
                    {
                        indexMemoryFixedDim = index;
                    }
                }

            }



            //CEC21 benchmark
            batchResults1 = statsByMHAlgoAndBenchFuncDESERIALIZE[statsByMHAlgoAndBenchFuncDESERIALIZE.Count - 1];
            index = indexMemoryFixedDim;


            for (int j = 0; j < batchResults1.Count; j++)
            {
                var singleResult1 = batchResults1[j];

                if (singleResult1 == null)
                {
                    index = indexMemoryFixedDim;
                }
                else
                {
                    linesMeanSTD[index + 1] += singleResult1.Item2[0] + "," + singleResult1.Item2[3] + ",";
                    linesMean[index + 1] += singleResult1.Item2[0] + ",";
                    NumberFunctionEvaluationMean[index + 1] += singleResult1.Item2[0] + ",";

                    index++;
                }

            }




            formattedCSVOutputFileMeanSTD = "";
            formattedCSVOutputFileMean = "";
            formattedCSVOutputFileFENMean = "";

            foreach (var item in linesMeanSTD)
            {
                formattedCSVOutputFileMeanSTD += item + Environment.NewLine;
            }

            foreach (var item in linesMean)
            {
                formattedCSVOutputFileMean += item + Environment.NewLine;
            }

            foreach (var item in NumberFunctionEvaluationMean)
            {
                formattedCSVOutputFileFENMean += item + Environment.NewLine;
            }








            string fileName;
            int counter = 1;
            fileName = dateForFolderName + "resultsMeanSTD" + counter + ".csv";
            while (File.Exists(fileName) == true)
            {
                fileName = dateForFolderName + "resultsMeanSTD" + counter++ + ".csv";
            }

            try
            {
                File.AppendAllText(fileName, formattedCSVOutputFileMeanSTD);
            }
            catch (Exception)
            {

            }


            fileName = "";
            counter = 1;
            fileName = dateForFolderName + "resultsMean" + counter + ".csv";
            while (File.Exists(fileName) == true)
            {
                fileName = dateForFolderName + "resultsMean" + counter++ + ".csv";
            }

            try
            {
                File.AppendAllText(fileName, formattedCSVOutputFileMean);
            }
            catch (Exception)
            {

            }


            fileName = "";
            counter = 1;
            fileName = dateForFolderName + "resultsFENumberMean" + counter + ".csv";
            while (File.Exists(fileName) == true)
            {
                fileName = dateForFolderName + "resultsFENumberMean" + counter++ + ".csv";
            }

            try
            {
                File.AppendAllText(fileName, formattedCSVOutputFileFENMean);
            }
            catch (Exception)
            {

            }
            return;




            List<List<Tuple<string, List<double>>>> StatsByMHAlgoAndBenchFunc = new();
            string path = "G:\\Oussama\\Universite\\Recherche\\Habilitation\\Article\\SelfAdaptive\\Journal\\AllResultsMeanSuccMutRateSingleLine.txt";
            string[] fileConten;

            fileConten = File.ReadAllLines(path);
            List<double> valuesAcquired = new();
            double doubleValue;

            foreach (string s in fileConten)
            {
                if (s != "")
                {
                    doubleValue = Convert.ToDouble(s);
                    valuesAcquired.Add(doubleValue);
                }
            }


            List<double> DirectedABCValuesMutation = new();
            List<double> BasicABCValuesMutation = new();
            List<double> MingABCValuesMutation = new();
            List<double> GBestABCValuesMutation = new();
            List<double> MABCValuesMutation = new();
            List<double> ARABCValuesMutation = new();
            List<double> ABC2ValuesMutation = new();
            List<double> referenceValuesMutation = new();


            DirectedABCValuesMutation = valuesAcquired.Where((a, inde) => inde < 106).ToList();
            BasicABCValuesMutation = valuesAcquired.Where((a, inde) => (inde >= 106 * 1) & (inde < 106 * 2)).ToList();
            MingABCValuesMutation = valuesAcquired.Where((a, inde) => (inde >= 106 * 2) & (inde < 106 * 3)).ToList();
            GBestABCValuesMutation = valuesAcquired.Where((a, inde) => (inde >= 106 * 3) & (inde < 106 * 4)).ToList();
            MABCValuesMutation = valuesAcquired.Where((a, inde) => (inde >= 106 * 4) & (inde < 106 * 5)).ToList();
            ARABCValuesMutation = valuesAcquired.Where((a, inde) => (inde >= 106 * 5) & (inde < 106 * 6)).ToList();
            ABC2ValuesMutation = valuesAcquired.Where((a, inde) => (inde >= 106 * 6) & (inde < 106 * 7)).ToList();
            referenceValuesMutation = valuesAcquired.Where((a, inde) => (inde >= 106 * 7) & (inde < 106 * 8)).ToList();





            path = "G:\\Oussama\\Universite\\Recherche\\Habilitation\\Article\\SelfAdaptive\\Journal\\AllResultsMeanFunctionSingleLine.txt";


            fileConten = File.ReadAllLines(path);
            valuesAcquired = new();
            doubleValue = 0;

            foreach (string s in fileConten)
            {
                if (s != "")
                {
                    doubleValue = Convert.ToDouble(s);
                    valuesAcquired.Add(doubleValue);
                }
            }


            List<double> DirectedABCValues = new();
            List<double> BasicABCValues = new();
            List<double> MingABCValues = new();
            List<double> GBestABCValues = new();
            List<double> MABCValues = new();
            List<double> ARABCValues = new();
            List<double> ABC2Values = new();
            List<double> referenceValues = new();


            DirectedABCValues = valuesAcquired.Where((a, inde) => inde < 106).ToList();
            BasicABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 1) & (inde < 106 * 2)).ToList();
            MingABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 2) & (inde < 106 * 3)).ToList();
            GBestABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 3) & (inde < 106 * 4)).ToList();
            MABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 4) & (inde < 106 * 5)).ToList();
            ARABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 5) & (inde < 106 * 6)).ToList();
            ABC2Values = valuesAcquired.Where((a, inde) => (inde >= 106 * 6) & (inde < 106 * 7)).ToList();
            referenceValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 7) & (inde < 106 * 8)).ToList();

            string constructedLine = "";

            for (int i = 0; i < 18; i++)
            {

                constructedLine += DirectedABCValues[i] + "," + DirectedABCValuesMutation[i] + ",";
                constructedLine += BasicABCValues[i] + "," + BasicABCValuesMutation[i] + ",";
                constructedLine += MingABCValues[i] + "," + MingABCValuesMutation[i] + ",";
                constructedLine += GBestABCValues[i] + "," + GBestABCValuesMutation[i] + ",";
                constructedLine += MABCValues[i] + "," + MABCValuesMutation[i] + ",";
                constructedLine += ARABCValues[i] + "," + ARABCValuesMutation[i] + ",";
                constructedLine += referenceValues[i] + "," + referenceValuesMutation[i] + ",";
                constructedLine += Environment.NewLine;

                constructedLine += DirectedABCValues[i + (18 * 3)] + "," + DirectedABCValuesMutation[i + (18 * 3)] + ",";
                constructedLine += BasicABCValues[i + (18 * 3)] + "," + BasicABCValuesMutation[i + (18 * 3)] + ",";
                constructedLine += MingABCValues[i + (18 * 3)] + "," + MingABCValuesMutation[i + (18 * 3)] + ",";
                constructedLine += GBestABCValues[i + (18 * 3)] + "," + GBestABCValuesMutation[i + (18 * 3)] + ",";
                constructedLine += MABCValues[i + (18 * 3)] + "," + MABCValuesMutation[i + (18 * 3)] + ",";
                constructedLine += ARABCValues[i + (18 * 3)] + "," + ARABCValuesMutation[i + (18 * 3)] + ",";
                constructedLine += referenceValues[i + (18 * 3)] + "," + referenceValuesMutation[i + (18 * 3)] + ",";
                constructedLine += Environment.NewLine;

                constructedLine += DirectedABCValues[i + (18 * 4)] + "," + DirectedABCValuesMutation[i + (18 * 4)] + ",";
                constructedLine += BasicABCValues[i + (18 * 4)] + "," + BasicABCValuesMutation[i + (18 * 4)] + ",";
                constructedLine += MingABCValues[i + (18 * 4)] + "," + MingABCValuesMutation[i + (18 * 4)] + ",";
                constructedLine += GBestABCValues[i + (18 * 4)] + "," + GBestABCValuesMutation[i + (18 * 4)] + ",";
                constructedLine += MABCValues[i + (18 * 4)] + "," + MABCValuesMutation[i + (18 * 4)] + ",";
                constructedLine += ARABCValues[i + (18 * 4)] + "," + ARABCValuesMutation[i + (18 * 4)] + ",";
                constructedLine += referenceValues[i + (18 * 4)] + "," + referenceValuesMutation[i + (18 * 4)] + ",";
                constructedLine += Environment.NewLine;
            }


            for (int i = 90; i < 106; i++)
            {

                constructedLine += DirectedABCValues[i] + "," + DirectedABCValuesMutation[i] + ",";
                constructedLine += BasicABCValues[i] + "," + BasicABCValuesMutation[i] + ",";
                constructedLine += MingABCValues[i] + "," + MingABCValuesMutation[i] + ",";
                constructedLine += GBestABCValues[i] + "," + GBestABCValuesMutation[i] + ",";
                constructedLine += MABCValues[i] + "," + MABCValuesMutation[i] + ",";
                constructedLine += ARABCValues[i] + "," + ARABCValuesMutation[i] + ",";
                constructedLine += referenceValues[i] + "," + referenceValuesMutation[i] + ",";
                constructedLine += Environment.NewLine;

            }

            path = "G:\\Oussama\\Universite\\Recherche\\Habilitation\\Article\\SelfAdaptive\\Journal\\ResltFormatedArticle.csv";


            File.AppendAllText(path, constructedLine);

        }


        private static void ComputeWilcoxonTest()
        {

            List<List<Tuple<string, List<double>>>> StatsByMHAlgoAndBenchFunc = new();
            string path = "G:\\Oussama\\Universite\\Recherche\\Habilitation\\Article\\SelfAdaptive\\Journal\\AllResultsSingleLine.txt";
            string[] fileConten;

            fileConten = File.ReadAllLines(path);
            List<double> valuesAcquired = new();
            double doubleValue;

            foreach (string s in fileConten)
            {
                if (s != "")
                {
                    doubleValue = Convert.ToDouble(s);
                    valuesAcquired.Add(doubleValue);
                }
            }


            List<double> DirectedABCValues = new();
            List<double> BasicABCValues = new();
            List<double> MingABCValues = new();
            List<double> GBestABCValues = new();
            List<double> MABCValues = new();
            List<double> ARABCValues = new();
            List<double> ABC2Values = new();
            List<double> referenceValues = new();
            double[] diffValues = new double[106];


            DirectedABCValues = valuesAcquired.Where((a, inde) => inde < 106).ToList();
            BasicABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 1) & (inde < 106 * 2)).ToList();
            MingABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 2) & (inde < 106 * 3)).ToList();
            GBestABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 3) & (inde < 106 * 4)).ToList();
            MABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 4) & (inde < 106 * 5)).ToList();
            ARABCValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 5) & (inde < 106 * 6)).ToList();
            ABC2Values = valuesAcquired.Where((a, inde) => (inde >= 106 * 6) & (inde < 106 * 7)).ToList();
            referenceValues = valuesAcquired.Where((a, inde) => (inde >= 106 * 7) & (inde < 106 * 8)).ToList();


            double bothTail, leftTail, RightTail;
            string result = "";


            ////Ming ABC
            //for (int i = 0; i < referenceValues.Count(); i++)
            //{
            //    diffValues[i] = referenceValues[i] - MingABCValues[i];
            //}

            //bothTail = 0;
            //alglib.wilcoxonsignedranktest(diffValues, 18, 0.0d, out bothTail, out leftTail, out RightTail);
            //result += "Vs Ming ABC," + bothTail + "," + leftTail + "," + RightTail + Environment.NewLine;


            ////DABC
            //for (int i = 0; i < referenceValues.Count(); i++)
            //{
            //    diffValues[i] = referenceValues[i] - DirectedABCValues[i];
            //}

            //bothTail = 0;
            //alglib.wilcoxonsignedranktest(diffValues, 18, 0.0d, out bothTail, out leftTail, out RightTail);
            //result += "Vs DABC," + bothTail + "," + leftTail + "," + RightTail + Environment.NewLine;



            ////GBestABCValues ABC
            //for (int i = 0; i < referenceValues.Count(); i++)
            //{
            //    diffValues[i] = referenceValues[i] - GBestABCValues[i];
            //}

            //bothTail = 0;
            //alglib.wilcoxonsignedranktest(diffValues, 18, 0.0d, out bothTail, out leftTail, out RightTail);
            //result += "Vs GBestABC," + bothTail + "," + leftTail + "," + RightTail + Environment.NewLine;




            ////Basic ABC
            //for (int i = 0; i < referenceValues.Count(); i++)
            //{
            //    diffValues[i] = referenceValues[i] - BasicABCValues[i];
            //}

            //bothTail = 0;
            //alglib.wilcoxonsignedranktest(diffValues, 18, 0.0d, out bothTail, out leftTail, out RightTail);
            //result += "Vs Basic ABC," + bothTail + "," + leftTail + "," + RightTail + Environment.NewLine;








            ////MABCValues ABC
            //for (int i = 0; i < referenceValues.Count(); i++)
            //{
            //    diffValues[i] = referenceValues[i] - MABCValues[i];
            //}

            //bothTail = 0;
            //alglib.wilcoxonsignedranktest(diffValues, 18, 0.0d, out bothTail, out leftTail, out RightTail);
            //result += "Vs MABC," + bothTail + "," + leftTail + "," + RightTail + Environment.NewLine;



            ////ARABCValues ABC
            //for (int i = 0; i < referenceValues.Count(); i++)
            //{
            //    diffValues[i] = referenceValues[i] - ARABCValues[i];
            //}

            //bothTail = 0;
            //alglib.wilcoxonsignedranktest(diffValues, 18, 0.0d, out bothTail, out leftTail, out RightTail);
            //result += "Vs ARABC," + bothTail + "," + leftTail + "," + RightTail + Environment.NewLine;


            File.AppendAllText("G:\\Oussama\\Universite\\Recherche\\Habilitation\\Article\\SelfAdaptive\\Journal\\AllAlgoWilcoxonTestResults1.txt", result);
        }


        private static List<List<GlobalBatchResultModelWithoutInterfaceProblem>?> ConvertResultWithoutInterfaceProblem(List<List<GlobalBatchResultModel>?> GlobalResults)
        {
            GlobalBatchResultModelWithoutInterfaceProblem resultWithoutInterfaceProblem;
            List<List<GlobalBatchResultModelWithoutInterfaceProblem>?> resultCorrected = new();
            List<GlobalBatchResultModelWithoutInterfaceProblem> resultCorrectedBatch;


            foreach (List<GlobalBatchResultModel>? resultBatch in GlobalResults)
            {
                resultCorrectedBatch = new();

                if (resultBatch == null) { continue; }
                foreach (GlobalBatchResultModel Result in resultBatch)
                {
                    resultWithoutInterfaceProblem = new GlobalBatchResultModelWithoutInterfaceProblem(Result);
                    resultCorrectedBatch.Add(resultWithoutInterfaceProblem);
                }

                resultCorrected.Add(resultCorrectedBatch);
            }


            return resultCorrected;
        }


    }
}


