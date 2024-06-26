using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BusinessLogic;
using Commit = BusinessLogic.Commit;

namespace BusinessLogic
{
    public class RepositoryAnalyzer
    {
        /// <summary>
        /// This method Loads all the Commits that has a Valid Parent commit into MasterObject.Commits 
        /// </summary>
        /// <param name="repoPath"></param>
        //public void DownloadCommits(string repoPath)
        //{
        //    using (var repo = new Repository(repoPath))
        //    {
        //        int counter = 0;
        //        int totalcommits = repo.Commits.Count();
        //        foreach (var commit in repo.Commits)
        //        {
        //            counter++;
        //            Helper.SetStatus($"Downloading  {counter} / {totalcommits} Commits");

        //            // No parent means it's the first commit, so we have nothing to compare against.
        //            if (!commit.Parents.Any())
        //            {
        //                continue;
        //            }

        //            var parentCommit = commit.Parents.First();
        //            var changes = repo.Diff.Compare<TreeChanges>(parentCommit.Tree, commit.Tree);

        //            foreach (var change in changes)
        //            {
        //                if (MasterObject.Commits.Any(x => x.CommitId == commit.Sha))
        //                {
        //                    var FilePath = change.Path;
        //                    var ContentBefore = GetFileContentFromCommit(repo, change.Path, parentCommit);
        //                    var ContentAfter = GetFileContentFromCommit(repo, change.Path, commit);
        //                    MasterObject.Commits.FirstOrDefault(x => x.CommitId == commit.Sha).Add(FilePath, ContentAfter, ContentBefore);
        //                }
        //                else
        //                {
        //                    var FilePath = change.Path;
        //                    var ContentBefore = GetFileContentFromCommit(repo, change.Path, parentCommit);
        //                    var ContentAfter = GetFileContentFromCommit(repo, change.Path, commit);
        //                    MasterObject.Commits.Add(new Commit(commit.Sha, FilePath, ContentAfter, ContentBefore));
        //                }

        //            }
        //        }
        //    }
        //}

        public string GetProjectNameFromPath(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                // Attempt to retrieve a project name from the path in the repository's configuration
                // This method assumes there might be some identifiable information in the path structure stored in the configuration
                var path = repo.Info.Path;

                // The Info.Path usually points to the ".git" directory or file
                // We can navigate up one level to assume that's the project directory
                var directoryInfo = new DirectoryInfo(path).Parent;

                if (directoryInfo != null)
                {
                    return directoryInfo.Name;
                }

                throw new InvalidOperationException("Unable to determine project name from repository info.");
            }
        }

        public string getLastCommitSha_ID(string repoPath)
        {
            string lastCommitSha = String.Empty;

            using (var repo = new Repository(repoPath))
            {
                LibGit2Sharp.Commit lastCommit = repo.Head.Tip;
                lastCommitSha = lastCommit.Sha;
            }
            return lastCommitSha;
        }

        //public CandidateCommit DownloadSingleCommit_Recomendation(string repoPath, string commitSha)
        //{
        //    List<File_Contents> files = new List<File_Contents>();

        //    using (var repo = new Repository(repoPath))
        //    {
        //        var commit = repo.Lookup<LibGit2Sharp.Commit>(commitSha);
        //        if (commit == null)
        //        {
        //           return null;
        //        }
        //        var parentCommit = commit.Parents.First();
        //        var changes = repo.Diff.Compare<TreeChanges>(parentCommit.Tree, commit.Tree);
        //        //List<string> files = new List<string>();

        //        foreach (var change in changes)
        //        {
        //            var fileName = change.Path; // Assuming 'Path' is the file name here.
        //            var contentBefore = GetFileContentFromCommit(repo, fileName, parentCommit);
        //            var contentAfter = GetFileContentFromCommit(repo, fileName, commit);

        //            files.Add(new File_Contents
        //            {
        //                FilePath = fileName,
        //                Parent_Content = contentBefore,
        //                Content = contentAfter,
        //                DateTimeCommited = commit.Author.When
        //            }); ;
        //        }
        //    }

        //    return files;
        //}

        public void DownloadCommits(string repoPath, bool isSaved = false)
        {
            //Dictionary<string, string> data = new Dictionary<string, string>();
            using (var repo = new Repository(repoPath))
            {
                int counter = 0;
                int totalcommits = repo.Commits.Count();
                foreach (var commit in repo.Commits)
                {
                    counter++;

                    bool isCommitForRecomendation = false;
                    if (commit.Sha == MasterObject.CommitShaForRecomendation)
                    {
                        isCommitForRecomendation = true;
                    }
                    //Helper.SetStatus($"Downloading  {counter} / {totalcommits} Commits");

                    //data.Add(commit.Sha, commit.Author.When.ToString());
                    if (isSaved && isCommitForRecomendation==false)
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
                    MasterObject.Commits.FirstOrDefault(x => x.CommitId == commit.Sha).ParentCommitID = commit.Parents.First().Id.Sha;
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
                        if (testClass.Any(x => x.FileNameWithOutExtension.ToLower().Contains( theSourceClass.FileNameWithOutExtension.ToLower())))
                        {
                            if (!MasterObject.Commits.Any(x => x.CommitId == commit.Sha))
                                MasterObject.Commits.Add(new Commit() { CommitId = commit.Sha, Commited_DateTime = commit.Author.When, Commited_DateTime_Raw = commit.Author.When,ParentCommitID = commit.Parents.First().Id.Sha });

                            MasterObject.Commits.FirstOrDefault(x => x.CommitId == commit.Sha).ChangedFiles.AddRange(sourceClass);
                            MasterObject.Commits.FirstOrDefault(x => x.CommitId == commit.Sha).ChangedFiles.AddRange(testClass);
                            MasterObject.Commits.FirstOrDefault(x => x.CommitId == commit.Sha).ParentCommitID = commit.Parents.First().Id.Sha;
                            break;
                        }
                    }
                }
            }
            //var v = MasterObject.Commits.FirstOrDefault(x => x.CommitId == "eea36f49f6b09c302f5f51cfd6184472f436261d");
        }

        private string GetFileContentFromCommit(Repository repo, string path, LibGit2Sharp.Commit commit)
        {
            return "";
            //var blob = commit[path]?.Target as Blob;
            //return blob?.GetContentText(Encoding.UTF8) ?? string.Empty; // Specify the encoding if necessary
        }


        public void CheckoutCommitsToSeparateLocations(string sourceRepoPath, List<CandidateCommit> theCommits, string baseDirectory)
        {
            // Ensure base directory exists or create it
            //string baseDir = Path.Combine(baseDirectory, "bin");
            //Directory.CreateDirectory(baseDir);

            // Clone the repository to a temporary location
            string datetime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string tempRepoPath = Path.Combine(baseDirectory, datetime, "Repo");
            Repository.Clone(sourceRepoPath, tempRepoPath);

            using (var repo = new Repository(tempRepoPath))
            {
                int commitCount = 0;
                int totalcommits = theCommits.Count();
                foreach (var thecommit in theCommits)
                {
                    string commitSha = thecommit.CommitSha;
                    string destinationPath = Path.Combine(baseDirectory, datetime, commitSha);
                    if (Directory.Exists(destinationPath))
                    {
                        Console.WriteLine($"Destination path {destinationPath} already exists. Skipping this commit.");
                        continue;
                    }

                    Directory.CreateDirectory(destinationPath);

                    // Checkout the commit into the destination path
                    try
                    {
                        // Checkout the specific commit
                        var commit = repo.Lookup<LibGit2Sharp.Commit>(commitSha);
                        if (commit == null)
                        {
                            Console.WriteLine($"Commit {commitSha} not found. Skipping.");
                            continue;
                        }

                        // Commands class provides the ability to do various Git operations dynamically
                        repo.Reset(ResetMode.Hard, commit);
                        CopyRepositoryContents(tempRepoPath, destinationPath);
                        thecommit.RepoLocation = destinationPath;
                        //Console.WriteLine($"Successfully checked out commit {commitSha} to {destinationPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to checkout commit {commitSha}: {ex.Message}");
                    }

                    commitCount++;
                    if (commitCount % 10 == 0)
                    {
                        Console.WriteLine($"Checked out {commitCount}/{totalcommits} commits...");
                    }
                }

                // Final status update
                Console.WriteLine($"Total commits processed: {commitCount}");
            }

            // Optionally remove the temporary repository after all checkouts
            // Directory.Delete(tempRepoPath, true);
        }


        private void CopyRepositoryContents(string sourceRepo, string destinationPath)
        {
            foreach (var directoryPath in Directory.GetDirectories(sourceRepo, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(directoryPath.Replace(sourceRepo, destinationPath));

            foreach (var filePath in Directory.GetFiles(sourceRepo, "*.*", SearchOption.AllDirectories))
                File.Copy(filePath, filePath.Replace(sourceRepo, destinationPath), true);
        }

        /// <summary>
        /// It Finds All the Valid Commits that Has Source Class and Source along with Test in PrefixOrSuffix Commits into MasterObject.ValidCommits
        /// </summary>
        public void FindValidCommits()
        {
            foreach (Commit theCommit in MasterObject.Commits)
            {

                if (theCommit.ChangedFiles.Count > 1)
                {
                    List<File_Contents> testClass = new List<File_Contents>();
                    List<File_Contents> sourceClass = new List<File_Contents>();
                    if (theCommit.ChangedFiles.Any(x => x.FileName.ToLower().Contains("test")))
                    {
                        foreach (var theFile in theCommit.ChangedFiles)
                        {
                            if (theFile.FileName.ToLower().Contains("enum") || !theFile.FileName.ToLower().EndsWith("java"))
                                continue;
                            if (theFile.FileName.ToLower().Contains("test"))
                            {
                                testClass.Add(theFile);
                            }
                            else
                            {
                                sourceClass.Add(theFile);
                            }
                        }
                    }
                    foreach (var theSourceClass in sourceClass)
                    {
                        if (testClass.Any(x => x.FileName.ToLower().Contains(theSourceClass.FileNameWithOutExtension.ToLower())))
                        {
                            if (!MasterObject.ValidCommitsForAnalysis.Any(x => x.CommitId == theCommit.CommitId))
                                MasterObject.ValidCommitsForAnalysis.Add(theCommit);
                            break;
                        }
                        //if(testClass.Any())
                        //{
                        //    if (!MasterObject.ValidCommitsForAnalysis.Any(x => x.CommitId == theCommit.CommitId))
                        //        MasterObject.ValidCommitsForAnalysis.Add(theCommit);
                        //    break;
                        //}

                    }
                }
            }
        }
    }
}
