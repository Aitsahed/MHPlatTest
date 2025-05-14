












using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MHPlatTest.Utility
{
    internal static class CompareAlgoResults
    {

        internal static void CompareAlgoResultsToReference(List<List<Tuple<string, List<double>>>> newResults, string nameAlgoRemoveFromComparaison, string executionTimeStamp)
        {



            int totalBencmarFunctionCounter = 0;
            int newBestGlobalCounter = 0;
            int newBestCounter = 0;
            int newBestGlobalCounterIgnoringNameAlgo = 0;
            int newBestCounterIgnoringNameAlgo = 0;
            bool currentAlgoNameSavedInCSVFile = false;
            string currentAlgoDesignationName = "";
            List<string> newAlgosDesignationList = new List<string>();
            int tempInt;

            //Acquiring Reference results to compare to
            List<List<Tuple<string, List<double>>>> referenceResults = new();
            List<List<Tuple<string, List<double>>>> filteredNewResults = new();

            string pathReferenceResults = File.ReadAllText("G:\\Oussama\\Universite\\Recherche\\Habilitation\\Results\\20250415-1219\\ResByDimSerailized1.txt");
            referenceResults = JsonSerializer.Deserialize<List<List<Tuple<string, List<double>>>>>(pathReferenceResults);


            //Acquiring the current lines saved in the CSV file in disk
            var linesInCSVFile = File.ReadAllLines("G:\\Oussama\\Universite\\Recherche\\Habilitation\\Results\\ResultsAllAlgos.csv");


            //string pathneweResults = File.ReadAllText("G:\\Oussama\\Universite\\Recherche\\Habilitation\\Results\\20240124-1839\\ResByDimSerailized1.txt");
            //newResults = JsonSerializer.Deserialize<List<List<Tuple<string, List<double>>>>>(pathneweResults);

            if (newResults == null || newResults.Count == 0) { return; }
            if (referenceResults == null || referenceResults.Count == 0) { return; }



            //Retrieving the list of all new algos in 'newResults'
            foreach (var item in newResults[0])
            {
                if (item == null) { continue; }

                tempInt = item.Item1.IndexOf('-');

                currentAlgoDesignationName = item.Item1.Substring(tempInt + 1);
                if (newAlgosDesignationList.Contains(currentAlgoDesignationName) == false)
                {
                    newAlgosDesignationList.Add(currentAlgoDesignationName);
                }
            }


            //Browsing based on algos designations
            foreach (var currentAlgoDesignation in newAlgosDesignationList)
            {

                //Browse the list in newResults (To get results by Dimension & Fixed dimension then CEC)
                for (int indexGlobalList = 0; indexGlobalList < newResults.Count; indexGlobalList++)
                {
                    var listResult = newResults[indexGlobalList].Where(itemInList => itemInList != null && itemInList.Item1.Contains(currentAlgoDesignation)).ToList();
                    newBestCounter = 0;
                    newBestCounterIgnoringNameAlgo = 0;

                    //Browse the benchmark function name in the new results
                    for (int j = 0; j < listResult.Count; j++)
                    {
                        var listElement = listResult[j];
                        if (listElement == null) { continue; }
                        var splittedText = listElement.Item1.Split("-", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var valueResultOptimization = listElement.Item2[0];

                        //Counting the total number of bencghmark function tests
                        totalBencmarFunctionCounter++;

                        if (splittedText.Length > 1)
                        {
                            string benchmarkFunctionName = splittedText[0];

                            //Retrieve the list of result for all algos for the current benchmarkfuncrtion in 'benchmarkFunctionName' and the same case of Dimension & Fixed dimension then CEC
                            var listResultReferenceCurrentBenchmarkFunctionName = referenceResults[indexGlobalList].Where(itemInList => itemInList != null && itemInList.Item1.Contains(benchmarkFunctionName));

                            //Create a temporary list where the result for the same case and benchmark function are stored
                            List<Tuple<string, List<double>>> orderedAlgosByResults = new();
                            //orderedAlgosByResults.Add(listElement);
                            foreach (var item in listResultReferenceCurrentBenchmarkFunctionName)
                            {
                                orderedAlgosByResults.Add(item);
                            }

                            //Order the results in the temporary list based on the optimization results
                            orderedAlgosByResults = orderedAlgosByResults.OrderBy(itemInList => itemInList.Item2[0]).ToList();

                            if (orderedAlgosByResults.Count == 0) { continue; }
                            //Compare the first element in the ordered list to the new result to see if ts better
                            if (valueResultOptimization <= orderedAlgosByResults[0].Item2[0])
                            {
                                newBestCounter++;
                                newBestGlobalCounter++;
                                newBestCounterIgnoringNameAlgo++;
                                newBestGlobalCounterIgnoringNameAlgo++;
                            }
                            //Compare the new algorithm while ignoring the algo named 'nameAlgoRemoveFromComparaison'
                            else if (nameAlgoRemoveFromComparaison != "" || nameAlgoRemoveFromComparaison != null)
                            {
                                orderedAlgosByResults.RemoveAll(itemInList => itemInList.Item1.Contains(nameAlgoRemoveFromComparaison));
                                if (valueResultOptimization <= orderedAlgosByResults[0].Item2[0])
                                {
                                    newBestCounterIgnoringNameAlgo++;
                                    newBestGlobalCounterIgnoringNameAlgo++;
                                }
                            }
                        }
                    }
                    //All benchmark functions have been evaluated
                    if (currentAlgoNameSavedInCSVFile == false)
                    {
                        linesInCSVFile[0] += "," + currentAlgoDesignation + "," + currentAlgoDesignation + " (Without ABCV2)";
                        currentAlgoNameSavedInCSVFile = true;
                    }

                    //Saving the result in the lines array 
                    linesInCSVFile[indexGlobalList + 1] += "," + newBestCounter + "," + newBestCounterIgnoringNameAlgo;
                    Console.WriteLine(newBestCounter + "      " + newBestCounterIgnoringNameAlgo);

                    newBestCounter = 0;
                    newBestCounterIgnoringNameAlgo = 0;
                }

                //Saving the total results
                linesInCSVFile[newResults.Count + 1] += "," + newBestGlobalCounter + "," + newBestGlobalCounterIgnoringNameAlgo;
                linesInCSVFile[newResults.Count + 2] += "," + (100.0 * newBestGlobalCounter / (Double)totalBencmarFunctionCounter).ToString("0.##") + "," + (100.0 * newBestGlobalCounterIgnoringNameAlgo / totalBencmarFunctionCounter).ToString("0.##");
                linesInCSVFile[newResults.Count + 3] += ", " + executionTimeStamp + ", " + executionTimeStamp;


                Console.WriteLine(newBestGlobalCounter + "    " + newBestGlobalCounterIgnoringNameAlgo);
                Console.WriteLine((100.0 * newBestGlobalCounter / (Double)totalBencmarFunctionCounter).ToString("0.##") + "    " + (100.0 * newBestGlobalCounterIgnoringNameAlgo / totalBencmarFunctionCounter).ToString("0.##"));
                Console.WriteLine(executionTimeStamp + "    " + executionTimeStamp);



                currentAlgoNameSavedInCSVFile = false;
                totalBencmarFunctionCounter = 0;
                newBestGlobalCounter = 0;
                newBestGlobalCounterIgnoringNameAlgo = 0;

            }

            try
            {
            File.WriteAllLines("G:\\Oussama\\Universite\\Recherche\\Habilitation\\Results\\ResultsAllAlgos.csv", linesInCSVFile);

            }
            catch (Exception)
            {

            }

        }





    }
}







