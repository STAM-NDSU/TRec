using BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace ConsoleApp
{
    public class BusinessLogic
    {
        public void CheckoutAndParseforJava()
        {
            //Commits for Recomendation
            var commitForRecomendation = MasterObject.Commits.FirstOrDefault(x => x.CommitId == MasterObject.CommitShaForRecomendation);
            if (commitForRecomendation != null)
            {
                MasterObject.ValidCommitsForAnalysis.Add(commitForRecomendation);
            }
            foreach (var theCommit in MasterObject.ValidCommitsForAnalysis)
            {
                var thecommit = new CandidateCommit();
                thecommit.CommitSha = theCommit.CommitId;
                thecommit.ChangedFiles.AddRange(theCommit.ChangedFiles.Select(x => x.FileNameWithOutExtension));
                thecommit.ChangedDateTime = theCommit.Commited_DateTime;

                var theParent = new CandidateCommit();
                theParent.CommitSha = theCommit.ParentCommitID;
                theParent.ChangedFiles.AddRange(theCommit.ChangedFiles.Select(x => x.FileNameWithOutExtension));
                theParent.ChangedDateTime = theCommit.Commited_DateTime;

                if (!Cache.theCommits.Any(x => x.CommitSha == thecommit.CommitSha))
                {
                    Cache.theCommits.Add(thecommit);
                }
                else
                {
                    Cache.theCommits.First(x => x.CommitSha == thecommit.CommitSha).ChangedDateTime = theCommit.Commited_DateTime;
                    Cache.theCommits.First(x => x.CommitSha == thecommit.CommitSha).ChangedFiles.AddRange(thecommit.ChangedFiles);
                    Cache.theCommits.First(x => x.CommitSha == thecommit.CommitSha).ChangedFiles = Cache.theCommits.First(x => x.CommitSha == thecommit.CommitSha).ChangedFiles.Distinct().ToList();
                }
                if (!Cache.theCommits.Any(x => x.CommitSha == theParent.CommitSha))
                {
                    Cache.theCommits.Add(theParent);
                }
                else
                {
                    Cache.theCommits.First(x => x.CommitSha == theParent.CommitSha).ChangedFiles.AddRange(theParent.ChangedFiles);
                    Cache.theCommits.First(x => x.CommitSha == theParent.CommitSha).ChangedFiles = Cache.theCommits.First(x => x.CommitSha == theParent.CommitSha).ChangedFiles.Distinct().ToList();
                }

                Cache.theCommits.First(x => x.CommitSha == thecommit.CommitSha).ParentCommit = Cache.theCommits.First(x => x.CommitSha == theParent.CommitSha);
            }

            RepositoryAnalyzer analyzer = new RepositoryAnalyzer();
            if (Cache.theCommits.FindAll(x => x.IsAnalysed == false).Count > 0)
                analyzer.CheckoutCommitsToSeparateLocations(MasterObject.tempRepoPath, Cache.theCommits.FindAll(x => x.IsAnalysed == false && String.IsNullOrEmpty(x.RepoLocation)), AppDomain.CurrentDomain.BaseDirectory);

            var toBeAnalysed = Cache.theCommits.FindAll(x => x.IsAnalysed == false);
            int batchSize = 50; // Size of each batch
            bool isChanged = false; // Track if any commit was changed during processing
            int processedCount = 0; // Counter to track the number of processed commits

            for (int i = 0; i < toBeAnalysed.Count; i += batchSize)
            {
                var batch = toBeAnalysed.Skip(i).Take(batchSize).ToList();
                Console.WriteLine($"Processing batch {i / batchSize + 1}/{(toBeAnalysed.Count + batchSize - 1) / batchSize}");

                // Initialize the CountdownEvent with the number of items in the batch
                using (var countdown = new CountdownEvent(batch.Count))
                {
                    Parallel.ForEach(batch, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 }, thecommit =>
                    {
                        if (!thecommit.IsAnalysed)
                        {
                            try
                            {
                                string result = new RunJavaApp().Run(thecommit.RepoLocation, thecommit.CommitSha, thecommit.ChangedFiles.Distinct().ToList());
                                var list = JsonConvert.DeserializeObject<List<CommitAST>>(result);

                                if (list.Count <= 1)
                                {
                                    lock (thecommit) // Locking the specific commit for thread-safe modifications
                                    {
                                        thecommit.CommitAST = list.FirstOrDefault();
                                        thecommit.IsAnalysed = true;
                                        isChanged = true; // Set isChanged to true as the commit was updated
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                                throw;
                            }
                            finally
                            {
                                // Signal the countdown event regardless of success or failure to ensure all are counted
                                countdown.Signal();
                            }
                        }
                        else
                        {
                            // If already analyzed, just signal the countdown
                            countdown.Signal();
                        }
                    });

                    // Wait for all threads to complete processing in this batch
                    countdown.Wait();
                }

                processedCount += batch.Count;
                Console.WriteLine($"Completed processing {processedCount} out of {toBeAnalysed.Count} commits.");

                // Save the entire list after each batch is fully processed
                lock (Cache.theCommits) // Lock the whole list to ensure thread-safe operation during save
                {
                    CRUDManager.Save_Updated_ParsedData(Cache.theCommits, MasterObject.CurrentRepoMetaData.RepoName);
                    //Console.WriteLine("Data saved successfully.");
                }
            }

            // Final save: check if any commit was updated after processing all
            if (isChanged)
            {
                lock (Cache.theCommits) // Ensure exclusive access during the final save operation
                {
                    CRUDManager.Save_Updated_ParsedData(Cache.theCommits, MasterObject.CurrentRepoMetaData.RepoName);
                    Console.WriteLine("Final data save completed.");
                }
            }


        }

        public List<MethodSet> FindChanedMethosSets_Prod(JavaFile current, JavaFile Parent)
        {
            List<MethodSet> theMethodSets = new List<MethodSet>();
            foreach (Method theMethod in current.MethodList)
            {
                if (Parent.MethodList.Any(x => x.MethodSignature_Raw == theMethod.MethodSignature_Raw))
                {
                    //Existing One,LetsCheck if it is Changed
                    string currentSnip = theMethod.CodeSnippet;
                    string parentSnip = Parent.MethodList.FirstOrDefault(x => x.MethodSignature_Raw == theMethod.MethodSignature_Raw).CodeSnippet;

                    if (!currentSnip.Equals(parentSnip))
                    {
                        var theMethodSet = new MethodSet();
                        theMethodSet.SourceMethodV1Snip = currentSnip;
                        theMethodSet.SourceMethodV2Snip = parentSnip;
                        theMethodSet.SourceMethodName = theMethod.MethodSignature;
                        theMethodSet.SourceMethodName_Raw = theMethod.MethodSignature_Raw;
                        theMethodSets.Add(theMethodSet);
                    }
                }
                else
                {
                    //NewMethod
                    string currentSnip = theMethod.CodeSnippet;
                    string parentSnip = "Method Not Found";

                    if (!currentSnip.Equals(parentSnip))
                    {
                        var theMethodSet = new MethodSet();
                        theMethodSet.SourceMethodV1Snip = currentSnip;
                        theMethodSet.SourceMethodV2Snip = parentSnip;
                        theMethodSet.SourceMethodName = theMethod.MethodSignature;
                        theMethodSet.SourceMethodName_Raw = theMethod.MethodSignature_Raw;
                        theMethodSets.Add(theMethodSet);
                    }
                }
            }
            return theMethodSets;
        }
        public List<string> format(List<string> calls)
        {
            List<string> strings = new List<string>();
            foreach (string s in calls)
            {
                string pattern = "<[^<>]*(?:<(?:[^<>]*)>)*[^<>]*>";

                // Replace the matches with an empty string
                string result = Regex.Replace(s, pattern, "");
                result = result.Replace("...", "[]");
                result = result.Replace("java.util.function.", "");
                result = result.Replace("java.util.", "");
                result = result.Replace("function.", "");
                strings.Add(result);
            }
            return strings;
        }

        public List<TestMethod> FindChanedMethosSets_Test(JavaFile current, JavaFile Parent)
        {
            List<TestMethod> theMethodSets = new List<TestMethod>();
            foreach (Method theMethod in current.MethodList)
            {
                if (theMethod.Annotations.Any(x => x.ToLower().Contains("test")) || theMethod.MethodSignature.ToLower().Trim().StartsWith("test"))
                    if (Parent.MethodList.Any(x => x.MethodSignature_Raw == theMethod.MethodSignature_Raw))
                    {
                        //Existing One,LetsCheck if it is Changed
                        string currentSnip = theMethod.CodeSnippet;
                        string parentSnip = Parent.MethodList.FirstOrDefault(x => x.MethodSignature_Raw == theMethod.MethodSignature_Raw).CodeSnippet;

                        if (!currentSnip.Equals(parentSnip))
                        {
                            var theMethodSet = new TestMethod();
                            theMethodSet.TestMethodSnip = currentSnip;
                            theMethodSet.TestClasssFileName = current.FileName;
                            theMethodSet.TestClass_Raw_FileContent_V1 = currentSnip;
                            theMethodSet.TestClass_Raw_FileContent_V2 = parentSnip;
                            theMethodSet.TestMethodName = theMethod.MethodSignature;
                            theMethodSet.TestMethodName_Raw = theMethod.MethodSignature_Raw;
                            theMethodSet.MethodCallsInside = format(theMethod.MethodCalls);
                            theMethodSets.Add(theMethodSet);
                        }
                    }
                    else
                    {
                        //NewMethod
                        string currentSnip = theMethod.CodeSnippet;
                        string parentSnip = "Method Not Found";

                        if (!currentSnip.Equals(parentSnip))
                        {
                            var theMethodSet = new TestMethod();
                            theMethodSet.TestMethodSnip = currentSnip;
                            theMethodSet.TestClasssFileName = current.FileName;
                            theMethodSet.TestClass_Raw_FileContent_V1 = currentSnip;
                            theMethodSet.TestClass_Raw_FileContent_V2 = parentSnip;
                            theMethodSet.TestMethodName = theMethod.MethodSignature;
                            theMethodSet.TestMethodName_Raw = theMethod.MethodSignature_Raw;
                            theMethodSet.MethodCallsInside = format(theMethod.MethodCalls);
                            theMethodSets.Add(theMethodSet);
                        }
                    }
            }
            return theMethodSets;
        }

        public List<TestMethod> FindChanedMethosSets_Test_Evaluation(JavaFile current, JavaFile Parent)
        {
            List<TestMethod> theMethodSets = new List<TestMethod>();
            foreach (Method theMethod in current.MethodList)
            {

                if (Parent.MethodList.Any(x => x.MethodSignature_Raw == theMethod.MethodSignature_Raw))
                {
                    if (theMethod.Annotations.Any(x => x.ToLower().Contains("test")) || theMethod.MethodSignature.ToLower().Trim().StartsWith("test"))
                    {
                        //Existing One,LetsCheck if it is Changed
                        string currentSnip = theMethod.CodeSnippet;
                        string parentSnip = Parent.MethodList.FirstOrDefault(x => x.MethodSignature_Raw == theMethod.MethodSignature_Raw).CodeSnippet;

                        if (!currentSnip.Equals(parentSnip))
                        {
                            var theMethodSet = new TestMethod();
                            theMethodSet.TestMethodSnip = currentSnip;
                            theMethodSet.TestClasssFileName = current.FileName;
                            theMethodSet.TestClass_Raw_FileContent_V1 = currentSnip;
                            theMethodSet.TestClass_Raw_FileContent_V2 = parentSnip;
                            theMethodSet.TestMethodName = theMethod.MethodSignature;
                            theMethodSet.TestMethodName_Raw = theMethod.MethodSignature_Raw;
                            theMethodSet.MethodCallsInside = format(theMethod.MethodCalls);
                            theMethodSets.Add(theMethodSet);
                        }
                    }
                    else
                    {
                        string currentSnip = theMethod.CodeSnippet;
                        string parentSnip = Parent.MethodList.FirstOrDefault(x => x.MethodSignature_Raw == theMethod.MethodSignature_Raw).CodeSnippet;
                        if (!currentSnip.Equals(parentSnip))
                            Cache.dummy.Add(parentSnip);
                    }
                }
            }
            return theMethodSets;
        }
        public List<MethodSet> FindChanedMethosSets_Prod_Evaluation(JavaFile current, JavaFile Parent)
        {
            List<MethodSet> theMethodSets = new List<MethodSet>();
            foreach (Method theMethod in current.MethodList)
            {
                if (Parent.MethodList.Any(x => x.MethodSignature_Raw == theMethod.MethodSignature_Raw))
                {
                    //Existing One,LetsCheck if it is Changed
                    string currentSnip = theMethod.CodeSnippet;
                    string parentSnip = Parent.MethodList.FirstOrDefault(x => x.MethodSignature_Raw == theMethod.MethodSignature_Raw).CodeSnippet;

                    if (!currentSnip.Equals(parentSnip))
                    {
                        var theMethodSet = new MethodSet();
                        theMethodSet.SourceMethodV1Snip = currentSnip;
                        theMethodSet.SourceMethodV2Snip = parentSnip;
                        theMethodSet.SourceMethodName = theMethod.MethodSignature;
                        theMethodSet.SourceMethodName_Raw = theMethod.MethodSignature_Raw;
                        theMethodSets.Add(theMethodSet);
                    }
                }
            }
            return theMethodSets;
        }
    }
}
