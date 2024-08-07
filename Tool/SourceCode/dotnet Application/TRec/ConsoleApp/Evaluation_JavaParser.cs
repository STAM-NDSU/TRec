using BusinessLogic;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class linkage
    {
        public string sourceMethodFileName { get; set; }
        public string testMethodFileName { get; set; }
        public string SourceMethodName { get; set; }
        public string TestMethodName { get; set; }
        public string Sha_DateTime_All { get; set; }
        public int Count { get; set; }
    }
    public class TestMethods_eval
    {
        public string filepath { get; set; }
        public string Sig { get; set; }
        public string Ann { get; set; }
        public string Both { get; set; }
    }
    public class Evaluation_JavaParser
    {
        public void Run()
        {
            getLinksCount();
            //do evaluation now
            var theEvaluationCommits = findEvaluationCommits();
            doEvaluation(theEvaluationCommits);

        }
        public void doEvaluation(List<CandidateCommit> commits)
        {
            List<Evaluation_Model> theEvaluationModel = new List<Evaluation_Model>();

            foreach (var theCommit in commits)
            {
                theCommit.ChangedFiles = theCommit.ChangedFiles.Distinct().ToList();
                //Differentiating Prod class and Test Class
                List<string> testClass = new List<string>();
                List<string> prodClass = new List<string>();
                if (theCommit.ChangedFiles.Any(x => x.ToLower().Contains("test")))
                {
                    foreach (var theFile in theCommit.ChangedFiles)
                    {
                        if (theFile.ToLower().Contains("enum"))
                            continue;
                        if (theFile.ToLower().Contains("test"))
                        {
                            testClass.Add(theFile);
                        }
                        else
                        {
                            prodClass.Add(theFile);
                        }
                    }
                }

                //MasterObject.CurrentProject.theSourceClasses.Clear();
                foreach (string theFileName in prodClass)
                {

                    SourceClass theSourceClass = new SourceClass();
                    theSourceClass.SourceClassFileNameWithOutExtension = theFileName;

                    var ParsedFile_current = theCommit?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == theFileName) ?? null;

                    var ParsedFile_Parent = theCommit.ParentCommit?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == theFileName) ?? null;
                    if (ParsedFile_Parent != null && ParsedFile_current != null)
                    {
                        theSourceClass.MethodSets = new BusinessLogic().FindChanedMethosSets_Prod(ParsedFile_current, ParsedFile_Parent);
                        if (theSourceClass.MethodSets.Count > 0)
                        {
                            var tstclsses = testClass.FindAll(x => x.ToLower().Contains(theSourceClass.SourceClassFileNameWithOutExtension.ToLower()));
                            foreach (var tstCls in tstclsses)
                            {
                                var ParsedFile_current_test = theCommit?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == tstCls) ?? null;
                                var ParsedFile_Parent_test = theCommit.ParentCommit?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == tstCls) ?? null;
                                var TestMethods = new List<TestMethod>();
                                if (ParsedFile_Parent_test != null && ParsedFile_current_test != null)
                                    TestMethods = new BusinessLogic().FindChanedMethosSets_Test(ParsedFile_current_test, ParsedFile_Parent_test);

                                foreach (MethodSet theSrcMet in theSourceClass.MethodSets)
                                {
                                    try
                                    {
                                        List<string> calledmethods = new List<string>();
                                        foreach (var theTstMethod in TestMethods)
                                        {
                                            if (theTstMethod.MethodCallsInside.Any(x => Program.AreMethodsEqual(x,theSrcMet.SourceMethodName)))
                                            {
                                                calledmethods.Add(theTstMethod.TestMethodName_Raw);
                                            }
                                        }

                                        var evaluationLine = new Evaluation_Model();
                                        evaluationLine.Commit = theCommit.CommitSha;
                                        evaluationLine.Datetime = theCommit.ChangedDateTime.ToString();
                                        evaluationLine.ProductionClass = theSourceClass.SourceClassFileNameWithOutExtension;
                                        evaluationLine.ChangedProductionMethods = theSrcMet.SourceMethodName_Raw;
                                        evaluationLine.TestClass = tstCls;

                                        var changedAndCalled = TestMethods.FindAll(x => x.MethodCallsInside.Any(y => Program.AreMethodsEqual(y,theSrcMet.SourceMethodName))).Select(x => x.TestMethodName_Raw).ToList();
                                        evaluationLine.ChangedAndCalled = String.Join("\n", changedAndCalled);
                                        evaluationLine.ChangedTestMethods = String.Join("\n", TestMethods.Select(x => x.TestMethodName_Raw).ToList());
                                        evaluationLine.CalledTestMethods = String.Join("\n", calledmethods);

                                        var theRealSourceClass = MasterObject.CurrentProject.theSourceClasses.FirstOrDefault(x => x.SourceClassFileNameWithOutExtension == theSourceClass.SourceClassFileNameWithOutExtension);
                                        if (theRealSourceClass == null)
                                            continue;
                                        var theRealSourceMethodSet = theRealSourceClass.MethodSets.FirstOrDefault(y => y.SourceMethodName_Raw == theSrcMet.SourceMethodName_Raw);
                                        if (theRealSourceMethodSet != null)
                                        {
                                            var theRealTestAna = theRealSourceMethodSet.TestMethods_Analytics;
                                            var filteredAndSorted = FilterAndSortByDate(theRealTestAna, theCommit.ChangedDateTime);

                                            var sortedAnalytics = filteredAndSorted
                                                                    .Select(item => item.TestMethodName)
                                                                    .ToList();
                                            if (sortedAnalytics.Any())
                                                evaluationLine.RecomendedTests = String.Join("\n", sortedAnalytics);

                                            List<int> matchingIndices = new List<int>();

                                            if(changedAndCalled.Count>4)
                                            {

                                            }
                                            foreach (var item in changedAndCalled)
                                            {
                                                int index = sortedAnalytics.IndexOf(item);
                                                if (index != -1)
                                                {
                                                    //if(!matchingIndices.Contains(index))
                                                    matchingIndices.Add(index);
                                                }
                                            }


                                            if (!matchingIndices.Any())
                                            {
                                                evaluationLine.RecomendedIndex = "No Index";

                                                //Finding Why No Index
                                                // if(changedmethods_test.)
                                            }
                                            else
                                                evaluationLine.RecomendedIndex = string.Join(", ", matchingIndices.Select(index => index + 1));
                                        }
                                        else
                                        {
                                            evaluationLine.RecomendedTests = "No Recomendations Found";
                                            evaluationLine.RecomendedIndex = "No Index";
                                        }
                                        theEvaluationModel.Add(evaluationLine);


                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
            
            var v =Cache.theCommitsWithLinks;
        }
        public void getLinksCount()
        {
            List<linkage> theLinks = new List<linkage>();
            foreach (var thesourceclass in MasterObject.CurrentProject.theSourceClasses)
            {
                foreach (var themethod in thesourceclass.MethodSets)
                {
                    foreach (var ana in themethod.TestMethods_Analytics)
                    {
                        List<string> Datetime_Sha = new List<string>();
                        var obj = new linkage();
                        obj.sourceMethodFileName = thesourceclass.SourceClassFileNameWithOutExtension;
                        obj.SourceMethodName = themethod.SourceMethodName_Raw;
                        obj.TestMethodName = ana.TestMethodName;
                        obj.testMethodFileName=themethod.TestMethods.FirstOrDefault(x=>x.TestMethodName_Raw==ana.TestMethodName).TestClasssFileName;
                        obj.Count = ana.Parsed_ChangedDateTime.Count();
                        
                        foreach(var changedDatetime in ana.Parsed_ChangedDateTime)
                        {
                            string sha = string.Empty;
                            Cache.theCommitsWithLinks.TryGetValue(changedDatetime, out sha);
                            Datetime_Sha.Add(changedDatetime + ":" + sha);
                        }
                        obj.Sha_DateTime_All = String.Join("\n", Datetime_Sha);
                        theLinks.Add(obj);
                        if (obj.Count > 1)
                        {

                        }
                    }

                }
            }
            var v = theLinks;

        }

        public static List<(string TestMethodName, int Frequency, List<DateTimeOffset> FilteredDates)> FilterAndSortByDate(List<TestMethod_Analytic> methods, DateTimeOffset myDateTimeValue)
        {
            var filteredList = methods
                .Select(method => new
                {
                    method.TestMethodName,
                    FilteredDates = method.Parsed_ChangedDateTime.Where(date => date <= myDateTimeValue).ToList()
                })
                .Where(x => x.FilteredDates.Count > 0) // Optional: Filter out methods with 0 relevant dates
                .Select(x => (x.TestMethodName, Frequency: x.FilteredDates.Count, x.FilteredDates))
                .OrderByDescending(x => x.Frequency) // Sort by Frequency (count of FilteredDates) in descending order
                .ThenByDescending(x => x.FilteredDates.Max()) // Then by the latest DateTimeOffset in descending order
                .ThenBy(x => x.TestMethodName) // Then by TestMethodName in ascending order for tie-breakers
                .ToList();

            return filteredList;
        }

        public List<CandidateCommit> findEvaluationCommits()
        {
            //List<CandidateCommit> theEvaluationCommits = new List<CandidateCommit>();
            List<CandidateCommit> theEvaluationCommits = new List<CandidateCommit>();
            var commits = Cache.theCommits.FindAll(x => x.ParentCommit != null && x.ParentCommit.CommitAST != null);
            foreach (var theCommit in commits)
            {
                theCommit.ChangedFiles = theCommit.ChangedFiles.Distinct().ToList();
                //Differentiating Prod class and Test Class
                List<string> testClass = new List<string>();
                List<string> prodClass = new List<string>();
                if (theCommit.ChangedFiles.Any(x => x.ToLower().Contains("test")))
                {
                    foreach (var theFile in theCommit.ChangedFiles)
                    {
                        if (theFile.ToLower().Contains("enum"))
                            continue;
                        if (theFile.ToLower().Contains("test"))
                        {
                            testClass.Add(theFile);
                        }
                        else
                        {
                            prodClass.Add(theFile);
                        }
                    }
                }

                bool hasProdMethods = false;
                bool hasTestMEthods = false;
                //MasterObject.CurrentProject.theSourceClasses.Clear();
                foreach (string theFileName in prodClass)
                {
                    SourceClass theSourceClass = new SourceClass();
                    theSourceClass.SourceClassFileNameWithOutExtension = theFileName;

                    var ParsedFile_current = theCommit?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == theFileName) ?? null;

                    var ParsedFile_Parent = theCommit.ParentCommit?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == theFileName) ?? null;
                    if (ParsedFile_Parent != null && ParsedFile_current != null)
                    {
                        theSourceClass.MethodSets = new BusinessLogic().FindChanedMethosSets_Prod_Evaluation(ParsedFile_current, ParsedFile_Parent);

                        if (theSourceClass.MethodSets.Count > 0)
                        {
                            hasProdMethods = true;
                            var tstclsses = testClass.FindAll(x => x.ToLower().Contains(theSourceClass.SourceClassFileNameWithOutExtension.ToLower()));
                            foreach (var tstCls in tstclsses)
                            {
                                var ParsedFile_current_test = theCommit?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == tstCls) ?? null;
                                var ParsedFile_Parent_test = theCommit.ParentCommit?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == tstCls) ?? null;
                                var TestMethods = new List<TestMethod>();
                                if (ParsedFile_Parent_test != null && ParsedFile_current_test != null)
                                    TestMethods = new BusinessLogic().FindChanedMethosSets_Test_Evaluation(ParsedFile_current_test, ParsedFile_Parent_test);
                                if(TestMethods.Count > 0)
                                    hasTestMEthods=true;

                            }
                        }
                    }
                }
                if (hasProdMethods == true && hasTestMEthods == true)
                {
                    theEvaluationCommits.Add(theCommit);
                }
            }
            return theEvaluationCommits;
        }
    }
}
