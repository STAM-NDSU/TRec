using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blob = LibGit2Sharp.Blob;

namespace BusinessLogic
{

    public class Evaluation_BL
    {
        public void StartEvaluation()
        {
            //findAllTestCases(MasterObject.CurrentRepoMetaData.RepoURL, System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString()));
            analysedcommits();
            Analysis();
            CloneReop(MasterObject.CurrentRepoMetaData.RepoURL);
            DownloadandCommits(MasterObject.tempRepoPath);
            //var filteredCommits = FilterCommits();
            //var CandidateCommits = FilterCandidateCommits(filteredCommits);
            var CandidateCommits = FilterCandidateCommits(MasterObject.Commits);
        }

        public void findAllTestCases(string repoUrl, string path)
        {


            //Console.Write("Enter the Git repository URL: ");
            //string repoUrl = Console.ReadLine();

            // Specify the path where the repository will be cloned
            // string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                // Clone the repository
                //Console.WriteLine($"Cloning {repoUrl} into {path}");
                // string temppath = Repository.Clone(repoUrl, System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString()));
                string appDataFolderPath = @"C:\TCM\";//AppDomain.CurrentDomain.BaseDirectory;
                string tempFolderPath = Path.Combine(appDataFolderPath, Path.Combine(new Random().Next(int.MinValue, int.MaxValue).ToString()));

                Repository.Clone(repoUrl, tempFolderPath);

                using (var repo = new Repository(tempFolderPath))
                {
                    // Retrieve the commit object for the given SHA
                    var commit = repo.Lookup<LibGit2Sharp.Commit>(repo.Head.Tip.Id);

                    if (commit == null)
                    {
                        //Console.WriteLine($"Commit with SHA {commitSha} not found.");
                        return;
                    }

                    // Checkout the commit
                    var checkoutOptions = new CheckoutOptions
                    {
                        CheckoutModifiers = CheckoutModifiers.Force
                    };

                    repo.Checkout(commit.Tree, null, checkoutOptions);



                }
                TraverseFiles(tempFolderPath);



                //foreach (TreeEntry treeEntry in branch.Tip.Tree)
                //{
                //    if (treeEntry.TargetType == TreeEntryTargetType.Blob && treeEntry.Name.ToLower().Contains("test"))
                //    {
                //        Console.WriteLine($"Processing file: {treeEntry.Path}");
                //        using (var stream = ((Blob)treeEntry.Target).GetContentStream())
                //        using (var reader = new StreamReader(stream))
                //        {
                //            string content = reader.ReadToEnd();
                //            // Assuming you have a method to parse content with Antlr
                //            var methods=new Bl_Antlr().ExtractMethods(content);
                //        }
                //    }
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            //finally
            //{
            //    // Clean up by deleting the cloned directory
            //    //if (Directory.Exists(path))
            //    //{
            //    //    Directory.Delete(path, true);
            //    //}
            //}
        }
        void TraverseFiles(string directoryPath)
        {
            int keytest = 0;
            int annotate = 0;
            int keyandAnn = 0;
            List<TestMethods_eval> Result = new List<TestMethods_eval>();

            foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(filePath);
                //Console.WriteLine($"Found file: {filePath}");
                if (fileName.ToLower().Contains("test") && fileName.EndsWith(".java"))
                {
                    // Here you would call your Antlr processing method
                    //Console.WriteLine($"File contains 'test' and is a Java file: {filePath}");
                    string content = File.ReadAllText(filePath);
                    var methods = new Bl_Antlr().ExtractMethods(content);
                    foreach (var keyvalue in methods.Methods)
                    {
                        bool flag= false;
                        bool ifBoth = false;
                        if (keyvalue.Key.ToLower().Contains("test"))
                        {
                            keytest++;
                            flag = true;
                        }
                            
                        if (keyvalue.Value.ToLower().Contains("@test")) 
                        {
                            annotate++;
                            flag = true;
                        }

                        if (keyvalue.Key.ToLower().Contains("test") && keyvalue.Value.ToLower().Contains("@test"))
                        {
                            keyandAnn++;

                            ifBoth = true;
                        }
                        if(flag)
                        {

                            Result.Add(new TestMethods_eval() { Sig = keyvalue.Key, Ann = keyvalue.Value, Both = ifBoth.ToString(),filepath= filePath });
                        }

                    }
                    // Example: ProcessFileWithAntlr(filePath);
                }
            }


        }
        List<DateTimeOffset> theList = new List<DateTimeOffset>();
        public void analysedcommits()
        {
            //List<DateTimeOffset> theList = new List<DateTimeOffset>();
            foreach (var v in MasterObject.CurrentProject.theSourceClasses)
            {
                foreach (var m in v.MethodSets)
                {
                    foreach (var a in m.TestMethods_Analytics)
                    {
                        theList.AddRange(a.ParsedChangedDateTime);
                    }
                }
            }
            //var abc= theList;
        }
        public void Analysis()
        {
            List<linkage> theLinks = new List<linkage>();
            foreach (var thesourceclass in MasterObject.CurrentProject.theSourceClasses)
            {
                foreach (var themethod in thesourceclass.MethodSets)
                {
                    foreach (var ana in themethod.TestMethods_Analytics)
                    {
                        var obj = new linkage();
                        obj.sourceMethodFileName=thesourceclass.SourceClassFileNameWithOutExtension;
                        obj.SourceMethodName = themethod.SourceMethodName;
                        obj.TestMethodName = ana.TestMethodName;
                        obj.Count = ana.Parsed_ChangedDateTime.Count();
                        theLinks.Add(obj);
                        if(obj.Count > 1)
                        {

                        }
                    }

                }
            }
            var v = theLinks;

        }
        public void evaluation_javaparser()
        {

        }
        public List<Commit> FilterCandidateCommits(List<Commit> Commits)
        {
            List<Evaluation_Model> theEvaluationModel = new List<Evaluation_Model>();
            List<Commit> theCandidateCommits = new List<Commit>();
            foreach (var theCommit in Commits)
            {
                if (!theList.Any(x => x == theCommit.Commited_DateTime))
                {
                    continue;
                }
                List<File_Contents> testClass = new List<File_Contents>();
                List<File_Contents> sourceClass = new List<File_Contents>();
                foreach (var changedfile in theCommit.ChangedFiles)
                {

                    if (changedfile.FileName.ToLower().Contains("test"))
                    {
                        testClass.Add(changedfile);
                    }
                    else
                    {
                        sourceClass.Add(changedfile);
                    }
                }
                foreach (var theSourceClass in sourceClass)
                {
                    var changedmethods_Prod = new Antlr_RepoAnalysis().FindChangedMethods_Evaluation(theSourceClass.Parent_Content, theSourceClass.Content);
                    if (changedmethods_Prod.Any())
                    {
                        testClass = testClass.FindAll(x => x.FileNameWithOutExtension.ToLower().Contains(theSourceClass.FileNameWithOutExtension.ToLower()));
                        foreach (var thetestClass in testClass)
                        {

                            var test_AllMethods = new Antlr_RepoAnalysis().ExtrackMethods_Eval(thetestClass.Content);
                            if (test_AllMethods.Any())
                            {
                                var changedmethods_test = new Antlr_RepoAnalysis().FindChangedMethods_Evaluation(thetestClass.Parent_Content, thetestClass.Content);
                                if (changedmethods_test.Any())
                                    foreach (var changedmethod in changedmethods_Prod)
                                    {
                                        var methodsignature = new Bl_Antlr().ExtraxtMethodNameOnly(changedmethod.SourceMethodName);
                                        try
                                        {
                                            var calledmethods = test_AllMethods.FindAll(x => x.SourceMethodV1Snip.Contains(methodsignature+"("));

                                            var evaluationLine = new Evaluation_Model();
                                            evaluationLine.Commit = theCommit.CommitId;
                                            evaluationLine.Datetime = theCommit.Commited_DateTime_Raw.ToString();
                                            evaluationLine.ProductionClass = theSourceClass.FileName;
                                            evaluationLine.ChangedProductionMethods = changedmethod.SourceMethodName;
                                            evaluationLine.TestClass = thetestClass.FileName;
                                            var changedandCalled = changedmethods_test.FindAll(x=>x.SourceMethodV1Snip.Contains(methodsignature + "("));
                                            evaluationLine.ChangedTestMethods = String.Join("\n", changedandCalled.Select(x => x.SourceMethodName).ToList());
                                            evaluationLine.CalledTestMethods = String.Join("\n", calledmethods.Select(x => x.SourceMethodName).ToList());


                                            var m = MasterObject.CurrentProject.theSourceClasses.FirstOrDefault(x => x.SourceClassFileName == theSourceClass.FileName)?.MethodSets.FirstOrDefault(x => x.SourceMethodName == changedmethod.SourceMethodName) ?? null;
                                            if (m == null)
                                                continue;
                                            evaluationLine.RecomendedTests = "";//String.Join("\n", new Details_Method().EvaluationBL(m, theCommit.Commited_DateTime_Raw));
                                            //List<int> matchingIndices = changedandCalled.Select(x => x.SourceMethodName).ToList()
                                            //.SelectMany((method, index) => new Details_Method().EvaluationBL(m, theCommit.Commited_DateTime_Raw).Select((test, testIndex) => new { method, test, testIndex }))
                                            //.Where(x => x.method == x.test)
                                            //.Select(x => x.testIndex)
                                            //.Distinct()
                                            //.ToList();

                                            //if (!matchingIndices.Any())
                                            //{
                                            //    evaluationLine.RecomendedIndex = "No Index";

                                            //    //Finding Why No Index
                                            //   // if(changedmethods_test.)
                                            //}
                                            //else
                                            //    evaluationLine.RecomendedIndex = string.Join(", ", matchingIndices.Select(index => index + 1));
                                            //theEvaluationModel.Add(evaluationLine);
                                        }
                                        catch { }
                                    }
                            }
                        }
                    }
                }
            }
            return theCandidateCommits;
        }
        public async void CloneReop(string RepoURL)
        {
            string tempRepoPath = await Task.Run(() =>
            {
                return Repository.Clone(RepoURL, System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString()));
            });


        }
        private string GetFileContentFromCommit(Repository repo, string path, LibGit2Sharp.Commit commit)
        {
            var blob = commit[path]?.Target as Blob;
            return blob?.GetContentText(Encoding.UTF8) ?? string.Empty; // Specify the encoding if necessary
        }
        public List<Commit> FilterCommits()
        {
            List<Commit> commits = MasterObject.Commits.FindAll(x => x.ChangedFiles.Count <= 3);
            return commits;
        }
        public void DownloadandCommits(string repoPath, bool isSaved = false)
        {
            MasterObject.Commits.Clear();
            using (var repo = new Repository(repoPath))
            {
                int counter = 0;
                int totalcommits = repo.Commits.Count();
                foreach (var commit in repo.Commits)
                {
                    counter++;
                   // Helper.SetStatus($"Downloading  {counter} / {totalcommits} Commits");

                    if (isSaved)
                    {
                        if (MasterObject.CurrentRepoMetaData != null)
                            if (MasterObject.CurrentRepoMetaData.TotalCommits.Any(x => x == commit.Sha))
                                continue;

                    }

                    if (!MasterObject.Commits.Any(x => x.CommitId == commit.Sha))
                        MasterObject.Commits.Add(new Commit() { CommitId = commit.Sha, Commited_DateTime_Raw = commit.Author.When, Commited_DateTime = commit.Author.When });

                    // No parent means it's the first commit, so we have nothing to compare against.
                    if (!commit.Parents.Any())
                    {
                        continue;
                    }

                    var parentCommit = commit.Parents.First();
                    var changes = repo.Diff.Compare<TreeChanges>(parentCommit.Tree, commit.Tree);

                    List<File_Contents> testClass = new List<File_Contents>();
                    List<File_Contents> sourceClass = new List<File_Contents>();

                    // We're assuming here that the 'change' object has a FileName property or similar to check against.
                    foreach (var change in changes)
                    {
                        var fileName = change.Path; // Assuming 'Path' is the file name here.
                        var contentBefore = GetFileContentFromCommit(repo, fileName, parentCommit);
                        var contentAfter = GetFileContentFromCommit(repo, fileName, commit);

                        // Skip files with "enum" in their name or files that do not end with ".java"
                        if (fileName.ToLower().Contains("enum") || !fileName.ToLower().EndsWith("java"))
                        {
                            continue;
                        }

                        // Check if the file is a test class
                        if (fileName.ToLower().Contains("test"))
                        {
                            testClass.Add(new File_Contents
                            {
                                FilePath = fileName,
                                Parent_Content = contentBefore,
                                Content = contentAfter
                            });
                        }
                        else // Otherwise, it's a source class
                        {
                            sourceClass.Add(new File_Contents
                            {
                                FilePath = fileName,
                                Parent_Content = contentBefore,
                                Content = contentAfter
                            });
                        }
                    }
                    foreach (var theSourceClass in sourceClass)
                    {
                        if (testClass.Any(x => x.FileNameWithOutExtension.ToLower().Contains(theSourceClass.FileNameWithOutExtension.ToLower())))
                        {
                            if (!MasterObject.Commits.Any(x => x.CommitId == commit.Sha))
                                MasterObject.Commits.Add(new Commit() { CommitId = commit.Sha, Commited_DateTime = commit.Author.When, Commited_DateTime_Raw = commit.Author.When });

                            MasterObject.Commits.FirstOrDefault(x => x.CommitId == commit.Sha).ChangedFiles.AddRange(sourceClass);
                            MasterObject.Commits.FirstOrDefault(x => x.CommitId == commit.Sha).ChangedFiles.AddRange(testClass);
                            break;
                        }
                    }
                }
            }

        }
    }
}
