using BusinessLogic;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Commit = BusinessLogic.Commit;

namespace ConsoleApp
{
    internal class Program
    {
        private static string GetTempDirectory()
        {
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
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


        static async Task Main(string[] args)
        {
            try
            {
                //new RunJavaApp().Run();
                string starttime = DateTime.Now.ToString();
                CRUDManager.LoadRepoWareHouse();

                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("****** Welcome to TRec - Regression Test Recommender ******");
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("Please Enter Local Repo Path");
                string repo_Path = Console.ReadLine();
                if (repo_Path.EndsWith("\\") || repo_Path.EndsWith("/"))
                {
                    repo_Path = repo_Path.TrimEnd('\\', '/');
                }
                repo_Path = repo_Path + "\\.git";

                MasterObject.tempRepoPath = repo_Path;
                Console.WriteLine("Please Enter CommitID for recomendations: ");
                string commitid = Console.ReadLine();
                MasterObject.CommitShaForRecomendation = commitid;
                Console.WriteLine();
                Console.WriteLine();

                string tempProjectname = new RepositoryAnalyzer().GetProjectNameFromPath(repo_Path);
                if (RepoWareHouse.RepoMetadata.Any(x => x.RepoName == tempProjectname))
                {
                    //Already saved Project

                    MasterObject.CurrentRepoMetaData = RepoWareHouse.RepoMetadata.FirstOrDefault(x => x.RepoName == tempProjectname);

                    Console.WriteLine("Downloading Commits - In Progress.");
                    new RepositoryAnalyzer().DownloadCommits(repo_Path, true);
                    Console.WriteLine("Downloading Commits - Completed.");
                    MasterObject.CurrentRepoMetaData.TotalCommits.AddRange(MasterObject.Commits.Select(x => x.CommitId));
                    CRUDManager.Save_Updated_RepoWareHouse();
                    new RepositoryAnalyzer().FindValidCommits();
                    CRUDManager.LoadParsedData(ref Cache.theCommits, MasterObject.CurrentRepoMetaData.RepoName);
                    var thecom= Cache.theCommits.FirstOrDefault(x=>x.CommitSha== "6540edfdc0b710f05316bbe6b4f0eb140dca317c");
                    new BusinessLogic().CheckoutAndParseforJava();

                }
                else
                {
                    //new Project
                    Console.WriteLine("Could not find TCT Link Database.");
                    Console.WriteLine("Performing Historical Analysis to extract TCT Links");
                    Console.WriteLine("Downloading Commits - In Progress.");
                    new RepositoryAnalyzer().DownloadCommits(repo_Path);
                    Console.WriteLine("Downloading Commits - Completed.");
                    new RepositoryAnalyzer().FindValidCommits();


                    RepoMetaData repoMetaData = new RepoMetaData();
                    repoMetaData.TotalValidCommits = MasterObject.ValidCommitsForAnalysis.Select(x => x.CommitId).ToList();
                    repoMetaData.TotalCommits = MasterObject.Commits.Select(x => x.CommitId).ToList();
                    repoMetaData.RepoURL = repo_Path;
                    repoMetaData.RepoName = new RepositoryAnalyzer().GetProjectNameFromPath(repo_Path);//repo_Path.Split('/').Last().Replace(".git", "");
                    RepoWareHouse.RepoMetadata.Add(repoMetaData);
                    MasterObject.CurrentRepoMetaData = repoMetaData;

                    new BusinessLogic().CheckoutAndParseforJava();

                    CRUDManager.Save_Updated_RepoWareHouse();
                    string endtime = DateTime.Now.ToString();
                    //CRUDManager.Save_Updated_ParsedData(Cache.theCommits, MasterObject.CurrentRepoMetaData.RepoName);
                }
                {
                    MasterObject.CurrentProject.theSourceClasses.Clear();
                    var commits = Cache.theCommits.FindAll(x => x.ParentCommit != null && x.ParentCommit.CommitAST != null);
                    HashSet<string> theCommitsWithLinks = new HashSet<string>();
                    //Dictionary<string, DateTimeOffset> theCommitswithLinks_Dict = new Dictionary<string, DateTimeOffset>();
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

                                            foreach (var tstMethod in TestMethods)
                                            {
                                                if (tstMethod.MethodCallsInside.Contains(theSrcMet.SourceMethodName))
                                                {
                                                    theSrcMet.TestMethods.Add(tstMethod);

                                                    if (!theSrcMet.TestMethods_Analytics.Any(x => x.TestMethodName == tstMethod.TestMethodName))
                                                    {
                                                        var tstAna = new TestMethod_Analytic();
                                                        tstAna.TestMethodName = tstMethod.TestMethodName_Raw;
                                                        tstAna.TestFileName = tstCls;
                                                        theSrcMet.TestMethods_Analytics.Add(tstAna);
                                                    }
                                                    theSrcMet.TestMethods_Analytics.First(x => x.TestMethodName == tstMethod.TestMethodName_Raw).Parsed_ChangedDateTime.Add(theCommit.ChangedDateTime);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (theSourceClass.MethodSets.Count > 0)
                            {
                                if (theSourceClass.MethodSets.Any(x => x.TestMethods.Count > 0))
                                {
                                    theCommitsWithLinks.Add(theCommit.CommitSha);
                                    if (!Cache.theCommitsWithLinks.ContainsKey(theCommit.ChangedDateTime))
                                        Cache.theCommitsWithLinks.Add(theCommit.ChangedDateTime, theCommit.CommitSha);
                                }
                                if (!MasterObject.CurrentProject.theSourceClasses.Any(x => x.SourceClassFileNameWithOutExtension == theFileName))
                                {
                                    theSourceClass.MethodSets = theSourceClass.MethodSets.FindAll(x => x.TestMethods.Count > 0);
                                    MasterObject.CurrentProject.theSourceClasses.Add(theSourceClass);
                                }
                                else
                                {
                                    theSourceClass.MethodSets = theSourceClass.MethodSets.FindAll(x => x.TestMethods.Count > 0);
                                    foreach (var theMethod in theSourceClass.MethodSets)
                                    {
                                        if (!MasterObject.CurrentProject.theSourceClasses.FirstOrDefault(x => x.SourceClassFileNameWithOutExtension == theFileName).MethodSets.Any(y => y.SourceMethodName_Raw == theMethod.SourceMethodName_Raw))
                                        {
                                            MasterObject.CurrentProject.theSourceClasses.FirstOrDefault(x => x.SourceClassFileNameWithOutExtension == theFileName).MethodSets.Add(theMethod);
                                        }
                                        else
                                        {
                                            foreach (var testMethod in theMethod.TestMethods)
                                            {
                                                if (!MasterObject.CurrentProject.theSourceClasses.FirstOrDefault(x => x.SourceClassFileNameWithOutExtension == theFileName).MethodSets.FirstOrDefault(y => y.SourceMethodName_Raw == theMethod.SourceMethodName_Raw).TestMethods.Any(z => z.TestMethodName_Raw == testMethod.TestMethodName_Raw))
                                                {
                                                    var testAna = theMethod.TestMethods_Analytics.FirstOrDefault(x => x.TestMethodName == testMethod.TestMethodName_Raw);
                                                    MasterObject.CurrentProject.theSourceClasses.FirstOrDefault(x => x.SourceClassFileNameWithOutExtension == theFileName).MethodSets.FirstOrDefault(y => y.SourceMethodName_Raw == theMethod.SourceMethodName_Raw).TestMethods.Add(testMethod);
                                                    MasterObject.CurrentProject.theSourceClasses.FirstOrDefault(x => x.SourceClassFileNameWithOutExtension == theFileName).MethodSets.FirstOrDefault(y => y.SourceMethodName_Raw == theMethod.SourceMethodName_Raw).TestMethods_Analytics.Add(testAna);

                                                }
                                                else
                                                {
                                                    var datetimelist = theMethod.TestMethods_Analytics.FirstOrDefault(x => x.TestMethodName == testMethod.TestMethodName_Raw).Parsed_ChangedDateTime;

                                                    var test = MasterObject.CurrentProject.theSourceClasses.FirstOrDefault(x => x.SourceClassFileNameWithOutExtension == theFileName).MethodSets.FirstOrDefault(y => y.SourceMethodName_Raw == theMethod.SourceMethodName_Raw).TestMethods_Analytics.FirstOrDefault(x => x.TestMethodName == testMethod.TestMethodName_Raw);
                                                    test.Parsed_ChangedDateTime.UnionWith(datetimelist);
                                                }
                                            }

                                        }
                                    }

                                }
                            }

                        }
                    }
                }
                var CommitForRecomendation = Cache.theCommits.FirstOrDefault(x => x.CommitSha == MasterObject.CommitShaForRecomendation);

                //Now Show Recomendations
                bool showsRecomen_Flag = false;
                if(CommitForRecomendation != null)
                foreach (var ChangedFile in CommitForRecomendation.ChangedFiles)
                {
                    if (!ChangedFile.ToLower().Contains("test"))
                        foreach (SourceClass sourceClass in MasterObject.CurrentProject.theSourceClasses)
                        {
                            if (sourceClass != null && ChangedFile == sourceClass.SourceClassFileNameWithOutExtension)
                            {
                                if (sourceClass.MethodSets.Any())
                                {
                                    var ParsedFile_current = CommitForRecomendation?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == ChangedFile) ?? null;

                                    var ParsedFile_Parent = CommitForRecomendation.ParentCommit?.CommitAST?.theFiles.FirstOrDefault(x => x.FileName == ChangedFile) ?? null;
                                    if (ParsedFile_Parent != null && ParsedFile_current != null)
                                    {
                                        var Methods = new BusinessLogic().FindChanedMethosSets_Prod(ParsedFile_current, ParsedFile_Parent);


                                        showsRecomen_Flag = true;
                                        
                                        string className = sourceClass.SourceClassFileNameWithOutExtension;
                                        Console.WriteLine($"--------- {className} ---------");
                                        Console.WriteLine("-----------------------------------------");

                                        // Console.WriteLine(className);
                                        foreach (var theMethod in Methods)
                                        // foreach (MethodSet methodSet in sourceClass.MethodSets)
                                        {
                                            var methodSet = sourceClass.MethodSets.FirstOrDefault(x => x.SourceMethodName == theMethod.SourceMethodName);
                                            
                                            string prodmethodname = theMethod.SourceMethodName_Raw;
                                            string recomendations = "";
                                            string all_Otherrecomendations = "";
                                            if (methodSet!=null && methodSet.TestMethods.Any())
                                            {
                                                //if (!methodSet.SourceMethodV1Snip.ToLower().Contains("public"))
                                                //    continue;
                                                List<TestMethod_Analytic> theTestAna = new List<TestMethod_Analytic>();


                                                var filteredAndSorted = FilterAndSortByDate(methodSet.TestMethods_Analytics, CommitForRecomendation.ChangedDateTime);

                                                var sortedAnalytics = filteredAndSorted
                                                                        .Take(5)
                                                                        .Select(item => item.TestMethodName)
                                                                        .ToList();
                                                if (sortedAnalytics.Any())
                                                    recomendations = String.Join("\n", sortedAnalytics);
                                                else
                                                    recomendations = "We Could not find any Recomendations.";

                                                all_Otherrecomendations = String.Join("\n", methodSet.TestMethods_Analytics.Select(x => x.TestMethodName).ToList());

                                                prodmethodname = methodSet.SourceMethodName;

                                            }
                                            Console.WriteLine($"Method: {prodmethodname}");
                                            Console.WriteLine("--------------------------------------------------------------------");
                                            Console.WriteLine($"Recommended Tests:");
                                            if (!String.IsNullOrEmpty(recomendations))
                                                Console.WriteLine(recomendations);
                                            else
                                                Console.WriteLine("No Recomendations Found.");
                                            Console.WriteLine();
                                            Console.WriteLine();

                                        }
                                       
                                    }

                                }
                            }

                        }
                }
                else
                    Console.WriteLine("No Recomendations Found.");
                Console.WriteLine();
                Console.WriteLine();
                if (!showsRecomen_Flag)
                {
                    Console.WriteLine($"No Recomndations Found.");
                }
                //string endtime = DateTime.Now.ToString();
                //var v = Cache.theCommitsWithLinks;
                // new Evaluation_JavaParser().Run();
                //new Evaluation_BL().StartEvaluation();

                while (true)
                {
                    Console.WriteLine("Please Enter \"C\" to Close");
                    string option = Console.ReadLine();
                    if (option == "C")
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();

            }


        }

    }
}

