using MHPlatTest.Divers;
using MHPlatTest.Interfaces;
using MHPlatTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace MHPlatTest.Utility
{
    internal static class DiversExtendedProperties
    {


        //private readonly static object parametersListLock = new();

        private static int GetDecimalScale(Random r)
        {
            for (int i = 0; i <= 28; i++)
            {
                if (r.NextDouble() >= 0.1)
                    return i;
            }
            return 0;
        }




        /// <summary>
        /// Display all parameter details from a list of parameters as a single line text
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        internal static string MHConfig2String(this List<OptimizationParameter> parameters)
        {
            string result = "";


            foreach (var item in parameters)
            {
                if (result != "")
                {
                    result += "  |  " + item.Name.ToString().ShortenNameString();
                }
                else
                {
                    result += item.Name.ToString().ShortenNameString();
                }

                result += " : " + item.Value.ToString();

            }

            return result;
        }


        /// <summary>
        /// Display only essential parameter details from a list of parameters as a single line text
        /// </summary>
        /// <param name="parameters">The list of all IParameter objects containing the customized configuration</param>
        /// <returns></returns>
        internal static string MHEssentialConfig2String(this List<OptimizationParameter> parameters)
        {
            string result = "";



            foreach (var item in parameters.Where(x => x.IsEssentialInfo == true))
            {
                if (result != "")
                {
                    result += "  |  " + item.Name.ToString().ShortenNameString();
                }
                else
                {
                    result += item.Name.ToString().ShortenNameString();
                }

                result += " : " + item.Value.ToString();

            }


            return result;
        }


        /// <summary>
        /// Shorten a long name to a more compacte format
        /// </summary>
        /// <param name="LongName">the name to shorten</param>
        /// <returns></returns>
        public static string ShortenNameString(this string LongName)
        {
            List<string> nameParts = new List<string>();
            int startIndex;
            string result = "";

            startIndex = 0;

            //Decompose the name to distinc parts
            for (int charIndex = 1; charIndex < LongName.Length; charIndex++)
            {
                if (char.IsUpper(LongName[charIndex]) == true || char.IsWhiteSpace(LongName[charIndex]) == true)
                {
                    nameParts.Add(LongName.Substring(startIndex, charIndex - startIndex));
                    startIndex = charIndex;
                }
                if (charIndex == LongName.Length - 1)
                {
                    nameParts.Add(LongName.Substring(startIndex));

                }
            }

            //Short each name part and compose the full name
            foreach (var namePartItem in nameParts)
            {
                if (namePartItem.Length < 6)
                {
                    result += namePartItem;
                }
                else
                {
                    double varDouble;
                    if (double.TryParse(namePartItem, out varDouble))
                    {
                        result += namePartItem;
                    }
                    else
                    {
                        result += namePartItem.Substring(0, 6);
                    }
                }
            }


            return result;

        }

        internal static double ComputeFitness(this double valueObjectiveFunction, OptimizationProblemType optimizationProblemType)
        {
            double result = 0;

            switch (optimizationProblemType)
            {
                case OptimizationProblemType.Maximization:
                    result = valueObjectiveFunction;
                    break;
                case OptimizationProblemType.Minimization:
                    if (valueObjectiveFunction > 0)
                    {
                        // double result1 = double.Divide(1M, (1M + (double)valueObjectiveFunction));
                        result = 1 / (1 + valueObjectiveFunction);
                    }
                    else
                    {
                        result = 1 + Math.Abs(valueObjectiveFunction);
                    }
                    break;
                default:
                    if (valueObjectiveFunction > 0)
                    {
                        result = 1 / (valueObjectiveFunction + 1);
                    }
                    else
                    {
                        result = 1 + Math.Abs(valueObjectiveFunction);
                    }
                    break;
            }

            if (valueObjectiveFunction == 1)
            {
                valueObjectiveFunction = 0;
            }
            return result;
        }



        /// <summary>
        /// Compute stats based on the obtained results from a batch of optimization processes
        /// </summary>
        /// <param name="results">the batch results of the optimizations processes</param>
        /// <param name="fieldMHResultName">the name of field to compute the stats for (OptimalFunctionValue)</param>
        /// <param name="statsToCompute">the type fo stats to compute (Mean, Std,...etc)</param>
        /// <param name="includeStatsDescription">Whether to include a text description about the MH algorithm and the optimization parametrs that have generated these results</param>
        /// <param name="orderingType">whether to order the results or not and in which way (Ascending or Descending)</param>
        /// <returns>a List<Tuple<string, Double?>> that contains the stats for each optimization scenario</returns>
        internal static List<Tuple<string, Double?>> ComputeStat(this List<GlobalBatchResultModel> results, MHOptimizationResult fieldMHResultName, StatsToComputeType statsToCompute, bool includeStatsDescription, OrderingType orderingType = OrderingType.None)
        {
            List<Tuple<string, Double?>> result = new();
            List<Tuple<string, Double?>> tempResult = new();

            List<int> mHOptimizationAlgorithmList = new();
            List<int> benchmarkFunctionList = new();

            string textDescriptionResult = "Null";
            double? valueResult;

            //Order the list of results




            //Retrieve the different instance of optimization algorithms
            //the 'results' may contains optimization results of different optimization processes

            foreach (var resultItem in results)
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


            List<GlobalBatchResultModel> filteredGlobalBatchResults;
            //Browse the different optimization instances
            foreach (var benchmarkFunctionItem in benchmarkFunctionList)
            {
                foreach (var mhOptimizationAlgoinstanceItem in mHOptimizationAlgorithmList)
                {
                    filteredGlobalBatchResults = new();

                    filteredGlobalBatchResults = results.Where(x => x.MHAlgorithm.InstanceID == mhOptimizationAlgoinstanceItem && x.BenchmarkFunction.ParentInstanceID == benchmarkFunctionItem).ToList();

                    filteredGlobalBatchResults = filteredGlobalBatchResults.OrderBy(resultItem => resultItem.RepetitionID).ToList();

                    //Empty result list, go to next optimization algo instance
                    if (filteredGlobalBatchResults.Count == 0)
                    {
                        continue;
                    }

                    //Construct the returned response
                    if (includeStatsDescription == true)
                    {
                        textDescriptionResult = $" {filteredGlobalBatchResults.First().BenchmarkFunction.Name} -{filteredGlobalBatchResults.First().MHAlgorithm.Name} ({filteredGlobalBatchResults.First().MHAlgorithm.Description}) -{fieldMHResultName.ToString()}-{statsToCompute.ToString()})";
                    }

                    ////Check to see if every result instance has the required field name (for exemple the computed optimal value)
                    ////if not dont't compute the stats
                    //if (filteredGlobalBatchResults.All(x => x.OptimizationResults.Any(y => y.Name == fieldMHResultName)) == false)
                    //{//At least one optimization result does not contains the required field name
                    //    result += "//" + Environment.NewLine;
                    //    continue;
                    //}


                    //Preparing the list of numerical values 
                    List<double> numericalValuesList = new();
                    List<OptimizationResultModel> resultsDataSingleRun = new();

                    //Browse all batch optimization results
                    foreach (var OptimizationResultItem in filteredGlobalBatchResults)
                    {
                        resultsDataSingleRun = OptimizationResultItem.OptimizationResults;

                        //Adding the required numerical data to the array of numerical values based on the field name                      
                        switch (fieldMHResultName)
                        {
                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.OptimalFunctionValue:
                                double doubleValue = (double)resultsDataSingleRun.Where(item => item.Name == fieldMHResultName).First().Value;

                                numericalValuesList.Add(Convert.ToDouble(doubleValue));
                                break;
                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.NumberOfFunctionEvaluation || currentCase == MHOptimizationResult.NumberOfTotalIteration || currentCase == MHOptimizationResult.ExecutionTime:
                                object objectValue = resultsDataSingleRun.Where(item => item.Name == fieldMHResultName).First().Value;

                                numericalValuesList.Add(Convert.ToDouble(objectValue));
                                break;

                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.TotalSuccessfullMutationCountData || currentCase == MHOptimizationResult.TotalMutationCountData || currentCase == MHOptimizationResult.SuccessfullMutationRate:
                                int[] TotalMutationCountArray;
                                //Get the maximal itertaion number
                                int maxIteration = (int)resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.NumberOfTotalIteration).First().Value;

                                if (currentCase == MHOptimizationResult.TotalSuccessfullMutationCountData)
                                {//Compute the total count of successfull mutation
                                    TotalMutationCountArray = (int[])resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalSuccessfullMutationCountData).First().Value;
                                    numericalValuesList.Add(TotalMutationCountArray[maxIteration]);
                                }
                                else if (currentCase == MHOptimizationResult.TotalMutationCountData)
                                {//Compute the total count of mutation
                                    TotalMutationCountArray = (int[])resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalMutationCountData).First().Value;
                                    numericalValuesList.Add(TotalMutationCountArray[maxIteration]);
                                }
                                else if (currentCase == MHOptimizationResult.SuccessfullMutationRate)
                                {//Compute thesuccessfull mutation rate
                                    double totalMutation, totalSuccessfullMutation;
                                    TotalMutationCountArray = (int[])resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalSuccessfullMutationCountData).First().Value;
                                    totalSuccessfullMutation = TotalMutationCountArray[maxIteration];
                                    TotalMutationCountArray = (int[])resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalMutationCountData).First().Value;
                                    totalMutation = TotalMutationCountArray[maxIteration];

                                    numericalValuesList.Add(totalSuccessfullMutation / totalMutation);

                                }
                                break;

                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.OptimalPoint:
                                throw new Exception("Stats on the optimal point values are Not Implemented");
                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.OptimumFound:
                                numericalValuesList = filteredGlobalBatchResults.Select(x => Convert.ToDouble(x.OptimizationResults.Where(y => y.Name == fieldMHResultName).First().Value)).ToList();

                                break;

                            default:
                                throw new Exception("Required stats are not identifiable");
                        }
                    }


                    //Compute the required stats obn the array of numerical values
                    double computedResult = 0;
                    switch (statsToCompute)
                    {
                        case StatsToComputeType.Mean:
                            computedResult = numericalValuesList.Average();
                            break;
                        case StatsToComputeType.STD:
                            int count = numericalValuesList.Count;
                            double average = numericalValuesList.Average();
                            double sum = numericalValuesList.Sum(d => Math.Pow(d - average, 2));
                            computedResult = Math.Sqrt(sum / (count - 1));
                            computedResult = numericalValuesList.StandardDeviation();
                            break;
                        case StatsToComputeType.Max:
                            computedResult = numericalValuesList.Max();
                            break;
                        case StatsToComputeType.Min:
                            computedResult = numericalValuesList.Min();
                            break;
                        case StatsToComputeType.Median:
                            computedResult = numericalValuesList.Median();

                            break;
                        default:
                            throw new Exception("Required stats operation isnot identifiable");
                    }

                    valueResult = computedResult;
                    tempResult.Add(new Tuple<string, double?>(textDescriptionResult, valueResult));
                }
                if (orderingType == OrderingType.Ascending)
                {
                    tempResult = tempResult.OrderBy(resultItem => resultItem.Item2).ToList();
                }
                else if (orderingType == OrderingType.Descending)
                {
                    tempResult = tempResult.OrderByDescending(resultItem => resultItem.Item2).ToList();
                }

                result.AddRange(tempResult);
                tempResult = new();
            }



            return result;

        }


        /// <summary>
        /// Compute stats based on the obtained results from a batch of optimization processes
        /// </summary>
        /// <param name="results">the batch results of the optimizations processes</param>
        /// <param name="differentStatsToComputeList">the list of the different stats to perfomr</param>
        /// <param name="includeStatsDescription">Whether to include a text description about the MH algorithm and the optimization parametrs that have generated these results</param>
        /// <param name="orderingType">whether to order the results or not and in which way (Ascending or Descending)</param>
        /// <returns>a List<Tuple<string, List<double>>> that contains the stats for each optimization scenario</returns>
        internal static List<Tuple<string, List<double>>> ComputeStat(this List<GlobalBatchResultModel> results, List<Tuple<MHOptimizationResult, StatsToComputeType>> differentStatsToComputeList, bool includeStatsDescription, List<string> algoToIgnore, GroupByType groupByType, OrderingType orderingType = OrderingType.None)
        {
            List<int> iteratonsWhenToComputeStatsList = new List<int>() { 500, 1000, 1500, 2000 };


            List<Tuple<string, List<double>>> result = new();
            List<Tuple<string, List<double>>> tempResult = new();
            List<double> numericalResultForMHAlgoAndBenchmarkFunction = new();

            List<int> mHOptimizationAlgorithmList = new();
            List<int> benchmarkFunctionList = new();

            string textDescriptionResult = "Null";
            double valueResult;

            //Retrieve the different instance of optimization algorithms
            //the 'results' may contains optimization results of different optimization processes

            foreach (var resultItem in results)
            {
                if (mHOptimizationAlgorithmList.Contains(resultItem.MHAlgorithm.InstanceID) == false && algoToIgnore.Contains(resultItem.MHAlgorithm.Description) == false)
                {
                    mHOptimizationAlgorithmList.Add(resultItem.MHAlgorithm.InstanceID);
                }

                if (benchmarkFunctionList.Contains(resultItem.BenchmarkFunction.ParentInstanceID) == false)
                {
                    benchmarkFunctionList.Add(resultItem.BenchmarkFunction.ParentInstanceID);
                }
            }

            int numberOfBenchmarkFunction = benchmarkFunctionList.Count;
            int numberOfMHAlgos = mHOptimizationAlgorithmList.Count;

            List<GlobalBatchResultModel> filteredGlobalBatchResults;
            //Browse the different optimization instances

            //foreach (var iteratonsWhenToComputeStatsItem in iteratonsWhenToComputeStatsList)
            //{

            //groupByType
            for (int iCounter = 0; iCounter < numberOfMHAlgos * numberOfBenchmarkFunction; iCounter++)
            {
                int benchmarkFunctionItem;
                int benchmarkFunctionItemID = -1;
                int mhOptimizationAlgoinstanceItem;
                int mhOptimizationAlgoinstanceItemID = -1;

                if (groupByType == GroupByType.Algorithm)
                {
                    mhOptimizationAlgoinstanceItemID = (int)Math.Floor((double)iCounter / (double)numberOfBenchmarkFunction);
                    benchmarkFunctionItemID = iCounter - mhOptimizationAlgoinstanceItemID * numberOfBenchmarkFunction;
                }
                else if (groupByType == GroupByType.BenchmarkFunction)
                {
                    benchmarkFunctionItemID = (int)Math.Floor((double)iCounter / (double)numberOfMHAlgos);
                    mhOptimizationAlgoinstanceItemID = iCounter - benchmarkFunctionItemID * numberOfMHAlgos;
                }

                benchmarkFunctionItem = benchmarkFunctionList[benchmarkFunctionItemID];
                mhOptimizationAlgoinstanceItem = mHOptimizationAlgorithmList[mhOptimizationAlgoinstanceItemID];



                filteredGlobalBatchResults = new();

                filteredGlobalBatchResults = results.Where(x => x.MHAlgorithm.InstanceID == mhOptimizationAlgoinstanceItem && x.BenchmarkFunction.ParentInstanceID == benchmarkFunctionItem).ToList();

                filteredGlobalBatchResults = filteredGlobalBatchResults.OrderBy(resultItem => resultItem.RepetitionID).ToList();

                //Empty result list, go to next optimization algo instance
                if (filteredGlobalBatchResults.Count == 0)
                {
                    continue;
                }

                //Construct the returned response
                if (includeStatsDescription == true)
                {
                    textDescriptionResult = $" {filteredGlobalBatchResults.First().BenchmarkFunction.Name} -{filteredGlobalBatchResults.First().MHAlgorithm.Name} ({filteredGlobalBatchResults.First().MHAlgorithm.Description})";
                }

                ////Check to see if every result instance has the required field name (for exemple the computed optimal value)
                ////if not dont't compute the stats
                //if (filteredGlobalBatchResults.All(x => x.OptimizationResults.Any(y => y.Name == fieldMHResultName)) == false)
                //{//At least one optimization result does not contains the required field name
                //    result += "//" + Environment.NewLine;
                //    continue;
                //}

                foreach (var currentStatsToCompute in differentStatsToComputeList)
                {


                    //Preparing the list of numerical values 
                    List<double> numericalValuesList = new();
                    List<OptimizationResultModel> resultsDataSingleRun = new();
                    List<List<double>> bestObjectiveFunctionEvaluationData = new();
                    int largestbestObjectiveFunctionEvaluationListSize = 0;
                    //Browse all batch optimization results
                    foreach (var OptimizationResultItem in filteredGlobalBatchResults)
                    {
                        resultsDataSingleRun = OptimizationResultItem.OptimizationResults;

                        //Adding the required numerical data to the array of numerical values based on the field name                      
                        switch (currentStatsToCompute.Item1)
                        {
                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.OptimalFunctionValue || currentCase == MHOptimizationResult.ProcessMSE || currentCase == MHOptimizationResult.ProcessMCV:
                                double doubleValue = (double)resultsDataSingleRun.Where(item => item.Name == currentStatsToCompute.Item1).First().Value;

                                numericalValuesList.Add(Convert.ToDouble(doubleValue));
                                break;
                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.NumberOfFunctionEvaluation || currentCase == MHOptimizationResult.NumberOfTotalIteration || currentCase == MHOptimizationResult.ExecutionTime || currentCase == MHOptimizationResult.ScoutBeesGeneratedCount:
                                object objectValue = resultsDataSingleRun.Where(item => item.Name == currentStatsToCompute.Item1).First().Value;

                                numericalValuesList.Add(Convert.ToDouble(objectValue));
                                break;

                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.TotalSuccessfullMutationCountData || currentCase == MHOptimizationResult.TotalMutationCountData || currentCase == MHOptimizationResult.SuccessfullMutationRate:
                                List<int> TotalMutationCountArray;
                                //Get the maximal itertaion number
                                int maxIteration = (int)resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.NumberOfTotalIteration).First().Value;

                                if (currentCase == MHOptimizationResult.TotalSuccessfullMutationCountData)
                                {//Compute the total count of successfull mutation
                                    TotalMutationCountArray = (List<int>)resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalSuccessfullMutationCountData).First().Value;
                                    numericalValuesList.Add(TotalMutationCountArray.Last());
                                }
                                else if (currentCase == MHOptimizationResult.TotalMutationCountData)
                                {//Compute the total count of mutation
                                    TotalMutationCountArray = (List<int>)resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalMutationCountData).First().Value;
                                    numericalValuesList.Add(TotalMutationCountArray.Last());
                                }
                                else if (currentCase == MHOptimizationResult.SuccessfullMutationRate)
                                {//Compute thesuccessfull mutation rate
                                    double totalMutation, totalSuccessfullMutation;
                                    TotalMutationCountArray = (List<int>)resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalSuccessfullMutationCountData).First().Value;
                                    totalSuccessfullMutation = TotalMutationCountArray.Last();
                                    TotalMutationCountArray = (List<int>)resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalMutationCountData).First().Value;
                                    totalMutation = TotalMutationCountArray.Last();

                                    numericalValuesList.Add((double)totalSuccessfullMutation / (double)totalMutation);
                                }
                                break;

                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.OptimalPoint:
                                throw new Exception("Stats on the optimal point values are Not Implemented");
                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.OptimumFound:
                                // numericalValuesList = filteredGlobalBatchResults.Select(x => Convert.ToDouble(x.OptimizationResults.Where(y => y.Name == currentStatsToCompute.Item1).First().Value)).ToList();
                                numericalValuesList.Add(Convert.ToDouble(resultsDataSingleRun.Where(y => y.Name == currentStatsToCompute.Item1).First().Value));

                                break;

                            //aLL bEST OBJECTIVE FUNCTION DATA ARE SAVED IN EACH ITERATION
                            case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.ObjectiveFunctionEvaluationData:
                                List<double> tempDoubleList;

                                tempDoubleList = (List<double>)resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.ObjectiveFunctionEvaluationData).First().Value;
                                if (tempDoubleList != null)
                                {
                                    if (tempDoubleList.Count > largestbestObjectiveFunctionEvaluationListSize)
                                    {
                                        largestbestObjectiveFunctionEvaluationListSize = tempDoubleList.Count;
                                    }

                                    bestObjectiveFunctionEvaluationData.Add(tempDoubleList);
                                }

                                break;

                            default:
                                throw new Exception("Required stats are not identifiable");
                        }
                    }


                    if (bestObjectiveFunctionEvaluationData.Count == 0)
                    {//The stats 
                     //Compute the required stats obn the array of numerical values
                        double computedResult = 0;
                        switch (currentStatsToCompute.Item2)
                        {
                            case StatsToComputeType.Mean:
                                computedResult = numericalValuesList.Average();
                                break;
                            case StatsToComputeType.STD:
                                int count = numericalValuesList.Count;
                                double average = numericalValuesList.Average();
                                double sum = numericalValuesList.Sum(d => Math.Pow(d - average, 2));
                                computedResult = Math.Sqrt(sum / (count - 1));
                                computedResult = numericalValuesList.StandardDeviation();
                                break;
                            case StatsToComputeType.Max:
                                computedResult = numericalValuesList.Max();
                                break;
                            case StatsToComputeType.Min:
                                computedResult = numericalValuesList.Min();
                                break;
                            case StatsToComputeType.Median:
                                computedResult = numericalValuesList.Median();

                                break;
                            default:
                                throw new Exception("Required stats operation isnot identifiable");
                        }

                        valueResult = computedResult;
                        numericalResultForMHAlgoAndBenchmarkFunction.Add(valueResult);
                    }
                    else
                    {
                        //The stats re being computed on arrays and the results is an array
                        //Computing the mean best function evaluation at each iteration
                        List<double> tempDoubleresultList = new List<double>(largestbestObjectiveFunctionEvaluationListSize);

                        foreach (var listBestObjFunEvalByIter in bestObjectiveFunctionEvaluationData)
                        {
                            for (int i = 0; i < largestbestObjectiveFunctionEvaluationListSize; i++)
                            {
                                if (i < listBestObjFunEvalByIter.Count - 1)
                                {//Current list has
                                    tempDoubleresultList[i] += listBestObjFunEvalByIter[i];
                                }
                                else
                                {
                                    tempDoubleresultList[i] += listBestObjFunEvalByIter.Last();
                                }
                            }

                        }


                        for (int i = 0; i < largestbestObjectiveFunctionEvaluationListSize; i++)
                        {
                            //Compute the mean
                            tempDoubleresultList[i] = tempDoubleresultList[i] / bestObjectiveFunctionEvaluationData.Count();
                        }

                    }

                }

                tempResult.Add(new Tuple<string, List<double>>(textDescriptionResult, numericalResultForMHAlgoAndBenchmarkFunction));
                numericalResultForMHAlgoAndBenchmarkFunction = new();


                if ((groupByType == GroupByType.Algorithm && (iCounter + 1) % numberOfBenchmarkFunction == 0) || (groupByType == GroupByType.BenchmarkFunction && (iCounter + 1) % numberOfMHAlgos == 0))
                {
                    if (orderingType == OrderingType.Ascending)
                    {
                        tempResult = tempResult.OrderBy(resultItem => resultItem.Item2[0]).ToList();
                    }
                    else if (orderingType == OrderingType.Descending)
                    {
                        tempResult = tempResult.OrderByDescending(resultItem => resultItem.Item2[0]).ToList();
                    }

                    foreach (var tempResultitem in tempResult)
                    {
                        result.Add(tempResultitem);
                    }
                    //Add null value to mark the satr of a new benchmark function
                    result.Add(null);
                    tempResult = new();
                }



                //}
            }



            //foreach (var benchmarkFunctionItem in benchmarkFunctionList)
            //{
            //    foreach (var mhOptimizationAlgoinstanceItem in mHOptimizationAlgorithmList)
            //    {
            //        filteredGlobalBatchResults = new();

            //        filteredGlobalBatchResults = results.Where(x => x.MHAlgorithm.InstanceID == mhOptimizationAlgoinstanceItem && x.BenchmarkFunction.ParentInstanceID == benchmarkFunctionItem).ToList();

            //        filteredGlobalBatchResults = filteredGlobalBatchResults.OrderBy(resultItem => resultItem.RepetitionID).ToList();

            //        //Empty result list, go to next optimization algo instance
            //        if (filteredGlobalBatchResults.Count == 0)
            //        {
            //            continue;
            //        }

            //        //Construct the returned response
            //        if (includeStatsDescription == true)
            //        {
            //            textDescriptionResult = $" {filteredGlobalBatchResults.First().BenchmarkFunction.Name} -{filteredGlobalBatchResults.First().MHAlgorithm.Name} ({filteredGlobalBatchResults.First().MHAlgorithm.Description})";
            //        }

            //        ////Check to see if every result instance has the required field name (for exemple the computed optimal value)
            //        ////if not dont't compute the stats
            //        //if (filteredGlobalBatchResults.All(x => x.OptimizationResults.Any(y => y.Name == fieldMHResultName)) == false)
            //        //{//At least one optimization result does not contains the required field name
            //        //    result += "//" + Environment.NewLine;
            //        //    continue;
            //        //}

            //        foreach (var currentStatsToCompute in differentStatsToComputeList)
            //        {


            //            //Preparing the list of numerical values 
            //            List<double> numericalValuesList = new();
            //            List<OptimizationResultModel> resultsDataSingleRun = new();

            //            //Browse all batch optimization results
            //            foreach (var OptimizationResultItem in filteredGlobalBatchResults)
            //            {
            //                resultsDataSingleRun = OptimizationResultItem.OptimizationResults;

            //                //Adding the required numerical data to the array of numerical values based on the field name                      
            //                switch (currentStatsToCompute.Item1)
            //                {
            //                    case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.OptimalFunctionValue:
            //                        double doubleValue = (double)resultsDataSingleRun.Where(item => item.Name == currentStatsToCompute.Item1).First().Value;

            //                        numericalValuesList.Add(Convert.ToDouble(doubleValue));
            //                        break;
            //                    case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.NumberOfFunctionEvaluation || currentCase == MHOptimizationResult.NumberOfTotalIteration || currentCase == MHOptimizationResult.ExecutionTime:
            //                        object objectValue = resultsDataSingleRun.Where(item => item.Name == currentStatsToCompute.Item1).First().Value;

            //                        numericalValuesList.Add(Convert.ToDouble(objectValue));
            //                        break;

            //                    case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.TotalSuccessfullMutationCountData || currentCase == MHOptimizationResult.TotalMutationCountData || currentCase == MHOptimizationResult.SuccessfullMutationRate:
            //                        int[] TotalMutationCountArray;
            //                        //Get the maximal itertaion number
            //                        int maxIteration = (int)resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.NumberOfTotalIteration).First().Value;

            //                        if (currentCase == MHOptimizationResult.TotalSuccessfullMutationCountData)
            //                        {//Compute the total count of successfull mutation
            //                            TotalMutationCountArray = (int[])resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalSuccessfullMutationCountData).First().Value;
            //                            numericalValuesList.Add(TotalMutationCountArray[maxIteration]);
            //                        }
            //                        else if (currentCase == MHOptimizationResult.TotalMutationCountData)
            //                        {//Compute the total count of mutation
            //                            TotalMutationCountArray = (int[])resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalMutationCountData).First().Value;
            //                            numericalValuesList.Add(TotalMutationCountArray[maxIteration]);
            //                        }
            //                        else if (currentCase == MHOptimizationResult.SuccessfullMutationRate)
            //                        {//Compute thesuccessfull mutation rate
            //                            double totalMutation, totalSuccessfullMutation;
            //                            TotalMutationCountArray = (int[])resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalSuccessfullMutationCountData).First().Value;
            //                            totalSuccessfullMutation = TotalMutationCountArray[maxIteration];
            //                            TotalMutationCountArray = (int[])resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.TotalMutationCountData).First().Value;
            //                            totalMutation = TotalMutationCountArray[maxIteration];

            //                            numericalValuesList.Add(totalSuccessfullMutation / totalMutation);

            //                        }
            //                        break;

            //                    case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.OptimalPoint:
            //                        throw new Exception("Stats on the optimal point values are Not Implemented");
            //                    case MHOptimizationResult currentCase when currentCase == MHOptimizationResult.OptimumFound:
            //                        numericalValuesList = filteredGlobalBatchResults.Select(x => Convert.ToDouble(x.OptimizationResults.Where(y => y.Name == currentStatsToCompute.Item1).First().Value)).ToList();

            //                        break;

            //                    default:
            //                        throw new Exception("Required stats are not identifiable");
            //                }
            //            }


            //            //Compute the required stats obn the array of numerical values
            //            double computedResult = 0;
            //            switch (currentStatsToCompute.Item2)
            //            {
            //                case StatsToComputeType.Mean:
            //                    computedResult = numericalValuesList.Average();
            //                    break;
            //                case StatsToComputeType.STD:
            //                    int count = numericalValuesList.Count;
            //                    double average = numericalValuesList.Average();
            //                    double sum = numericalValuesList.Sum(d => Math.Pow(d - average, 2));
            //                    computedResult = Math.Sqrt(sum / (count - 1));
            //                    computedResult = numericalValuesList.StandardDeviation();
            //                    break;
            //                case StatsToComputeType.Max:
            //                    computedResult = numericalValuesList.Max();
            //                    break;
            //                case StatsToComputeType.Min:
            //                    computedResult = numericalValuesList.Min();
            //                    break;
            //                case StatsToComputeType.Median:
            //                    computedResult = numericalValuesList.Median();

            //                    break;
            //                default:
            //                    throw new Exception("Required stats operation isnot identifiable");
            //            }


            //            valueResult = computedResult;
            //            numericalResultForMHAlgoAndBenchmarkFunction.Add(valueResult);
            //        }

            //        tempResult.Add(new Tuple<string, List<double>>(textDescriptionResult, numericalResultForMHAlgoAndBenchmarkFunction));
            //        numericalResultForMHAlgoAndBenchmarkFunction = new();
            //    }


            //}



            return result;

        }


        internal static void ComputeMeanBestObjectiveFuncEvalByIteratStat(this List<GlobalBatchResultModel> results, string CSVPathFile)
        {
            // List<int> iteratonsWhenToComputeStatsList = new List<int>() { 500, 1000, 1500, 2000 };
            List<string> linesTofile = new List<string>();

            List<Tuple<string, List<double>>> result = new();
            List<Tuple<string, List<double>>> tempResult = new();
            List<double> numericalResultForMHAlgoAndBenchmarkFunction = new();

            List<int> mHOptimizationAlgorithmList = new();
            List<int> benchmarkFunctionList = new();

            string textDescriptionResult = "Null";
            double valueResult;
            int indexCounterCSVLinesList = 0;

            //Retrieve the different instance of optimization algorithms
            //the 'results' may contains optimization results of different optimization processes

            foreach (var resultItem in results)
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

            int numberOfBenchmarkFunction = benchmarkFunctionList.Count;
            int numberOfMHAlgos = mHOptimizationAlgorithmList.Count;

            List<GlobalBatchResultModel> filteredGlobalBatchResults;
            //Browse the different optimization instances

            //foreach (var iteratonsWhenToComputeStatsItem in iteratonsWhenToComputeStatsList)
            //{

            //groupByType
            for (int iCounter = 0; iCounter < numberOfMHAlgos * numberOfBenchmarkFunction; iCounter++)
            {
                int benchmarkFunctionItem;
                int benchmarkFunctionItemID = -1;
                int mhOptimizationAlgoinstanceItem;
                int mhOptimizationAlgoinstanceItemID = -1;

                mhOptimizationAlgoinstanceItemID = (int)Math.Floor((double)iCounter / (double)numberOfBenchmarkFunction);
                benchmarkFunctionItemID = iCounter - mhOptimizationAlgoinstanceItemID * numberOfBenchmarkFunction;


                //if (groupByType == GroupByType.Algorithm)
                //{
                //    mhOptimizationAlgoinstanceItemID = (int)Math.Floor((double)iCounter / (double)numberOfBenchmarkFunction);
                //    benchmarkFunctionItemID = iCounter - mhOptimizationAlgoinstanceItemID * numberOfBenchmarkFunction;
                //}
                //else if (groupByType == GroupByType.BenchmarkFunction)
                //{
                benchmarkFunctionItemID = (int)Math.Floor((double)iCounter / (double)numberOfMHAlgos);
                mhOptimizationAlgoinstanceItemID = iCounter - benchmarkFunctionItemID * numberOfMHAlgos;
                //}

                benchmarkFunctionItem = benchmarkFunctionList[benchmarkFunctionItemID];
                mhOptimizationAlgoinstanceItem = mHOptimizationAlgorithmList[mhOptimizationAlgoinstanceItemID];

                filteredGlobalBatchResults = new();
                filteredGlobalBatchResults = results.Where(x => x.MHAlgorithm.InstanceID == mhOptimizationAlgoinstanceItem && x.BenchmarkFunction.ParentInstanceID == benchmarkFunctionItem).ToList();
                filteredGlobalBatchResults = filteredGlobalBatchResults.OrderBy(resultItem => resultItem.RepetitionID).ToList();

                //Empty result list, go to next optimization algo instance
                if (filteredGlobalBatchResults.Count == 0)
                {
                    continue;
                }

                //Construct the returned response
                textDescriptionResult = $" {filteredGlobalBatchResults.First().BenchmarkFunction.Name} -{filteredGlobalBatchResults.First().MHAlgorithm.Name} ({filteredGlobalBatchResults.First().MHAlgorithm.Description})";
                linesTofile.Add(textDescriptionResult);
                indexCounterCSVLinesList++;


                //Preparing the list of numerical values 
                List<double> numericalValuesList = new();
                List<OptimizationResultModel> resultsDataSingleRun = new();
                List<List<double>> bestObjectiveFunctionEvaluationData = new();
                int largestbestObjectiveFunctionEvaluationListSize = 0;
                //Browse all batch optimization results
                foreach (var OptimizationResultItem in filteredGlobalBatchResults)
                {
                    resultsDataSingleRun = OptimizationResultItem.OptimizationResults;

                    //Adding the required numerical data to the array of numerical values based on the field name                      

                    List<double> tempDoubleList;

                    tempDoubleList = (List<double>)resultsDataSingleRun.Where(item => item.Name == MHOptimizationResult.ObjectiveFunctionEvaluationData).First().Value;
                    if (tempDoubleList != null)
                    {
                        if (tempDoubleList.Count > largestbestObjectiveFunctionEvaluationListSize)
                        {
                            largestbestObjectiveFunctionEvaluationListSize = tempDoubleList.Count;
                        }

                        bestObjectiveFunctionEvaluationData.Add(tempDoubleList);
                    }


                }



                //The stats re being computed on arrays and the results is an array
                //Computing the mean best function evaluation at each iteration
                List<double> tempDoubleresultList = new List<double>(new double[largestbestObjectiveFunctionEvaluationListSize]);

                foreach (var listBestObjFunEvalByIter in bestObjectiveFunctionEvaluationData)
                {
                    for (int i = 0; i < largestbestObjectiveFunctionEvaluationListSize; i++)
                    {
                        if (i <= listBestObjFunEvalByIter.Count - 1)
                        {//Current list has
                            tempDoubleresultList[i] += listBestObjFunEvalByIter[i];
                        }
                        else
                        {
                            tempDoubleresultList[i] += listBestObjFunEvalByIter.Last();
                        }
                    }
                }


                for (int i = 0; i < largestbestObjectiveFunctionEvaluationListSize; i++)
                {
                    //Compute the mean
                    tempDoubleresultList[i] = tempDoubleresultList[i] / bestObjectiveFunctionEvaluationData.Count();


                    linesTofile.Add((tempDoubleresultList[i].ToString()));


                }


                linesTofile.Add(null);
                linesTofile.Add(null);
                linesTofile.Add(null);

                //if ((iCounter + 1) % numberOfMHAlgos == 0)
                //{
                //    indexCounterCSVLinesList = indexCounterCSVLinesList + largestbestObjectiveFunctionEvaluationListSize;

                //    //if (orderingType == OrderingType.Ascending)
                //    //{
                //    //    tempResult = tempResult.OrderBy(resultItem => resultItem.Item2[0]).ToList();
                //    //}
                //    //else if (orderingType == OrderingType.Descending)
                //    //{
                //    //    tempResult = tempResult.OrderByDescending(resultItem => resultItem.Item2[0]).ToList();
                //    //}

                //    //foreach (var tempResultitem in tempResult)
                //    //{
                //    //    result.Add(tempResultitem);
                //    //}
                //    //Add null value to mark the satr of a new benchmark function
                //    linesTofile.Add(null);
                //    //tempResult = new();
                //}


            }








            string CSVFileContent = "";

            foreach (var line in linesTofile)
            {
                CSVFileContent += line + Environment.NewLine;
            }


            File.AppendAllText(CSVPathFile, CSVFileContent);


            //return result;

        }
        internal static string OptimizationProcessResults2String(List<OptimizationResultModel> optimizationResult)
        {
            string result = "";

            foreach (var item in optimizationResult)
            {
                if (result != "")
                {
                    result += "  |  " + item.Name;
                }
                else
                {
                    result += item.Name;
                }

                result += " : " + item.Value.ToString();

            }

            return result;
        }

        /// <summary>
        /// Format the string of the stats results to be displayed on screen each line contains a result description and the value
        /// </summary>
        /// <param name="statsResults">the stats to be formatted on screen</param>
        /// <returns></returns>
        internal static string FormatStatsResults(this List<Tuple<string, Double?>> statsResults)
        {
            string result = "";
            int maxTextLength = 0;


            //Retrieving the longest text
            foreach (var StatResultItem in statsResults)
            {
                if (StatResultItem.Item1.Length > maxTextLength)
                {
                    maxTextLength = StatResultItem.Item1.Length;
                }
            }


            string textValue;
            double? doubleValue;
            //Formatting the text
            foreach (var StatResultItem in statsResults)
            {


                textValue = StatResultItem.Item1;
                doubleValue = StatResultItem.Item2;

                if (textValue.Length < maxTextLength)
                {
                    textValue = textValue.PadRight(maxTextLength);
                }
                textValue += " : ";

                result += textValue + doubleValue.ToString() + Environment.NewLine;
            }


            return result;
        }



        /// <summary>
        /// Format the string of the stats results to be displayed on screen each line contains a result description and SEVERAL valueS
        /// </summary>
        /// <param name="statsResults">the stats to be formatted on screen</param>
        /// <returns></returns>
        internal static string FormatStatsResults(this List<Tuple<string, List<Double>>> statsResults, List<Tuple<MHOptimizationResult, StatsToComputeType>> differentStatsToComputeList)
        {

            string result = "";
            int numberOfNumericalValues = statsResults[0].Item2.Count();
            List<int> maxTextLength = new List<int>();


            for (int i = 0; i < numberOfNumericalValues + 1; i++)
            {
                maxTextLength.Add(0);
            }

            //Retrieving the longest text
            foreach (var StatResultItem in statsResults)
            {
                if (StatResultItem == null)
                {//Add new line between result of different benchmarkfunction
                    result += Environment.NewLine;
                    continue;
                }

                if (StatResultItem.Item1.Length > maxTextLength[0])
                {
                    maxTextLength[0] = StatResultItem.Item1.Length;
                }

                for (int i = 1; i <= numberOfNumericalValues; i++)
                {
                    string tempString = StatResultItem.Item2[i - 1].ToString();

                    if (tempString.Length > maxTextLength[i])
                    {
                        maxTextLength[i] = tempString.Length;
                    }
                }

            }


            result = "Description".PadRight(maxTextLength[0]) + "|";

            for (int i = 1; i <= numberOfNumericalValues; i++)
            {
                var tempString = differentStatsToComputeList[i - 1].Item1.ToString().ShortenNameString() + "/" + differentStatsToComputeList[i - 1].Item2.ToString().ShortenNameString();

                if (tempString.Length > maxTextLength[i])
                {
                    maxTextLength[i] = tempString.Length;
                }

                tempString = tempString.PadRight(maxTextLength[i]) + "|";

                result += tempString;
            }

            result += Environment.NewLine;


            string textValue;
            double? doubleValue;
            //Formatting the text
            foreach (var StatResultItem in statsResults)
            {
                if (StatResultItem == null)
                {//Add new line between result of different benchmarkfunction
                    result += Environment.NewLine;
                    continue;
                }

                textValue = StatResultItem.Item1;

                textValue = textValue.PadRight(maxTextLength[0]) + "|";

                for (int i = 1; i <= numberOfNumericalValues; i++)
                {
                    string tempString = StatResultItem.Item2[i - 1].ToString();

                    textValue += tempString.PadRight(maxTextLength[i]) + "|";
                }


                result += textValue + Environment.NewLine;
            }


            return result;
        }

        internal static string FramtStatsCSVFile(List<Tuple<string, List<double>>> statsResults, List<Tuple<MHOptimizationResult, StatsToComputeType>> differentStatsToComputeList)
        {
            string messageToWrite = "Function/Algo";

            foreach (var itemStats in differentStatsToComputeList)
            {
                var tempString = itemStats.Item1.ToString().ShortenNameString() + "/" + itemStats.Item2.ToString().ShortenNameString();

                messageToWrite += "," + tempString;

            }

            messageToWrite += Environment.NewLine;

            foreach (var StatResultItem in statsResults)
            {
                if (StatResultItem == null)
                {//Add new line between result of different benchmarkfunction
                    continue;
                }

                messageToWrite += StatResultItem.Item1;

                foreach (var itemValue in StatResultItem.Item2)
                {
                    messageToWrite += "," + itemValue;
                }
                messageToWrite += Environment.NewLine;

            }


            return messageToWrite;
        }

        public static List<Type> GetLoadableTypes(this Assembly[] assemblyList, Type desiredObjectType)
        {
            List<Type> result = new();


            foreach (var assemblyItem in assemblyList)
            {
                try
                {
                    var ss = assemblyItem.GetTypes().Where(x => x.IsAssignableFrom(desiredObjectType) && x.IsClass == true);
                }
                catch (ReflectionTypeLoadException e)
                {
                    result.AddRange(e.Types.Where(t => t != null).ToList()); ;
                }

            }

            return result;


        }



        public static void SetDataElementsToSigleValue(this double[] functionParameter, double value)
        {
            for (int iShiftData = 0; iShiftData < functionParameter.Length; iShiftData++)
            {
                functionParameter[iShiftData] = value;
            }
        }


    }
}
