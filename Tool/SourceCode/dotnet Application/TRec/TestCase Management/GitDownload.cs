using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Linq;
using LibGit2Sharp;
using System.Collections.Generic;
using System.CodeDom;
using System.Threading;
using BusinessLogic;

namespace TestCase_Management
{
    public partial class GitDownload
    {
        //public string Commit1 { get; set; }
        //public string Commit2 { get; set; }
        //public string Owner { get; set; }
        //public string Repository { get; set; }

        public string CheckoutSpecificCommit(string repositoryPath, string commitSha)
        {

            string appDataFolderPath = @"C:\TCM\";//AppDomain.CurrentDomain.BaseDirectory;
            string tempFolderPath = Path.Combine(appDataFolderPath, Path.Combine(MasterObject.theGUID, new Random().Next(int.MinValue, int.MaxValue).ToString()));

            Repository.Clone(repositoryPath, tempFolderPath);

            using (var repo = new Repository(tempFolderPath))
            {
                // Retrieve the commit object for the given SHA
                var commit = repo.Lookup<LibGit2Sharp.Commit>(commitSha);

                if (commit == null)
                {
                    Console.WriteLine($"Commit with SHA {commitSha} not found.");
                    return "";
                }

                // Checkout the commit
                var checkoutOptions = new CheckoutOptions
                {
                    CheckoutModifiers = CheckoutModifiers.Force
                };

                repo.Checkout(commit.Tree, null, checkoutOptions);


                return tempFolderPath;
            }


            //Console.WriteLine($"Checked out commit with SHA {commitSha} to {destinationFolderPath} successfully.");

            //using (var repo = new Repository(repositoryPath))
            //{
            //    // Retrieve the commit object for the given SHA
            //    var commit = repo.Lookup<Commit>(commitSha);

            //    if (commit == null)
            //    {
            //        Console.WriteLine($"Commit with SHA {commitSha} not found.");
            //        return "";
            //    }

            //    // Checkout the commit
            //    var checkoutOptions = new CheckoutOptions
            //    {
            //        CheckoutModifiers = CheckoutModifiers.Force
            //    };

            //    // Here, you need to specify the paths of the files you want to checkout.
            //    // If you want to checkout all files, you can provide an empty list.
            //    var paths = new List<string>();

            //    repo.Checkout(commit.Tree, null, checkoutOptions);

            //    return repo.Info.WorkingDirectory;
            //    //Console.WriteLine($"Checked out commit with SHA {commitSha} successfully.");
            //}
        }
        public void AnalyzeRepository(string repoPath, string CommitId1 = "", string CommitId2 = "")
        {
            try
            {

                using (var repo = new Repository(repoPath))
                {
                    // Get the HEAD commit (the most recent commit in the repository)
                    var headCommit = repo.Head.Tip;
                    
                    // Traverse the commit history
                    foreach (var commit in repo.Commits)
                    {
                        
                        History theHistory = new History();

                        //theList.Add($"Commit: {commit.Id.Sha}");
                        theHistory.CommitID = commit.Id.Sha;
                        //theList.Add($"Author: {commit.Author.Name} <{commit.Author.Email}>");
                        theHistory.Author = $"{commit.Author.Name} <{commit.Author.Email}>";
                        //theList.Add($"Date: {commit.Author.When}");
                        theHistory.Date = commit.Author.When.DateTime;
                        //theList.Add($"Message: {commit.Message}");
                        theHistory.Message = $" {commit.Message}";

                        // Show the changed files in this commit
                        if (commit.Parents.Count() > 0)
                        {
                            var changes = repo.Diff.Compare<TreeChanges>(commit.Parents.First().Tree, commit.Tree);

                            foreach (var change in changes)
                            {
                                //theList.Add($"Change Path :- {change.Path}");
                                try { theHistory.FilesChanged.Add(change.Path); } catch { }
                            }
                        }
                        else
                        {
                            var changes = commit.Tree;

                            foreach (var change in changes)
                            {
                                //theList.Add($"Change Path :- {change.Path}");
                                try { theHistory.FilesChanged.Add(change.Path); } catch { }
                            }
                        }

                        MasterObject.theHistoryes.Add(theHistory);
                        //MessageBox.Show(String.Join("\n", theList));
                    }
                }

            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }

            List<History> theValidHistories = new List<History>();
            foreach (History theHistory in MasterObject.theHistoryes)
            {
                if (theHistory.FilesChanged.Count > 1)
                    if (theHistory.FilesChanged.Any(x => x.ToLower().Contains("test")))
                    {
                        List<string> testClasses = new List<string>();
                        List<string> sourceClasses = new List<string>();
                        foreach (var theFileChanged in theHistory.FilesChanged)
                        {
                            if (theFileChanged.ToLower().Contains("enum") || !theFileChanged.ToLower().EndsWith("java"))
                                continue;
                            if (theFileChanged.ToLower().Contains("test"))
                            {
                                testClasses.Add(Path.GetFileName(theFileChanged));
                            }
                            else
                            {
                                sourceClasses.Add(Path.GetFileName(theFileChanged));
                            }
                        }
                        foreach (string srcClass in sourceClasses)
                        {
                            if (testClasses.Any(x => x.ToLower().Contains(Path.GetFileNameWithoutExtension(srcClass.ToLower()))))
                            {
                                theValidHistories.Add(theHistory);
                                break;
                            }
                        }
                    }
            }
            MasterObject.theHistoryes.Sort((x, y) => DateTime.Compare(x.Date, y.Date));
            List<string> filesCHanged = new List<string>();
            List<string> commitsBetween = new List<string>();
            bool start = false;
            foreach (History theHistory in MasterObject.theHistoryes)
            {


                if (start)
                {
                    foreach (string fileName in theHistory.FilesChanged)
                    {
                        filesCHanged.Add(Path.GetFileName(fileName));
                    }
                    filesCHanged.AddRange(theHistory.FilesChanged);
                    commitsBetween.Add(theHistory.CommitID);
                }

                if (theHistory.CommitID == CommitId1)
                {
                    start = true;
                }
                if (theHistory.CommitID == CommitId2)
                {
                    if (!start)
                    {
                        MessageBox.Show("Commit 1 is new Version and Commit 2 is older version, Please swap the values!");
                        MasterObject.flag_AllGood = false;
                    }
                    start = false;
                }
            }
            MasterObject.filesChanged = filesCHanged;
            MasterObject.commitsBetween = commitsBetween;
        }

        //public GitDownload(string commit1, string commit2, string owner, string repository)
        //{
        //    Commit1 = commit1;
        //    Commit2 = commit2;
        //    Owner = owner;
        //    Repository = repository;
        //}
        public GitDownload()
        {

        }
        //public async void DownloadCommits()
        //{
        //    string owner = Owner;//"owner_username";
        //    string repository = Repository;//"repository_name";
        //    string commitSha1 = Commit1;
        //    string commitSha2 = Commit2;//txtCommit2.Text.Trim();

        //    await DownloadCommits(owner, repository, commitSha1, commitSha2);
        //}

        //public async Task DownloadCommits(string owner, string repository, string commitSha1, string commitSha2)
        //{
        //    string apiBaseUrl = "https://api.github.com";
        //    string cloneUrl = $"https://github.com/{owner}/{repository}.git";
        //    string tempDir = Path.Combine(Path.GetTempPath(), "GitHubTemp");

        //    using (HttpClient httpClient = new HttpClient())
        //    {
        //        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("YourApp"); // Replace "YourApp" with your application name

        //        try
        //        {
        //            // Download first commit
        //            string commit1ApiUrl = $"{apiBaseUrl}/repos/{owner}/{repository}/commits/{commitSha1}";
        //            string commit1DownloadUrl = await GetCommitDownloadUrl(httpClient, commit1ApiUrl);
        //            await CloneAndExtract(commit1DownloadUrl, commitSha1, tempDir);

        //            // Download second commit
        //            string commit2ApiUrl = $"{apiBaseUrl}/repos/{owner}/{repository}/commits/{commitSha2}";
        //            string commit2DownloadUrl = await GetCommitDownloadUrl(httpClient, commit2ApiUrl);
        //            await CloneAndExtract(commit2DownloadUrl, commitSha2, tempDir);

        //            MessageBox.Show("Commits downloaded successfully!");
        //        }
        //        catch (HttpRequestException ex)
        //        {
        //            MessageBox.Show($"Error accessing GitHub API: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}

        //public async Task<string> GetCommitDownloadUrl(HttpClient httpClient, string apiUrl)
        //{
        //    HttpResponseMessage commitResponse = await httpClient.GetAsync(apiUrl);
        //    commitResponse.EnsureSuccessStatusCode();
        //    string commitJson = await commitResponse.Content.ReadAsStringAsync();

        //    // Parse commit details to get the repository clone URL at the specific commit
        //    string treeUrl = JsonConvert.DeserializeObject<dynamic>(commitJson).commit.tree.url;
        //    HttpResponseMessage treeResponse = await httpClient.GetAsync(treeUrl);
        //    treeResponse.EnsureSuccessStatusCode();
        //    string treeJson = await treeResponse.Content.ReadAsStringAsync();

        //    // Parse the tree details to get the repository clone URL at the specific commit
        //    string downloadUrl = JsonConvert.DeserializeObject<dynamic>(treeJson).tree.FirstOrDefault().url;
        //    return downloadUrl.Replace("https://api.github.com/repos", "https://github.com").Replace("/git/trees", "");
        //}

        //public async Task CloneAndExtract(string downloadUrl, string commitSha, string tempDir)
        //{
        //    string commitDir = Path.Combine(tempDir, commitSha);

        //    // Clone the repository at the specific commit using Git
        //    ProcessStartInfo gitCloneInfo = new ProcessStartInfo
        //    {
        //        FileName = "git",
        //        Arguments = $"clone --depth 1 {downloadUrl} {commitDir}",
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        CreateNoWindow = true,
        //        UseShellExecute = false,
        //    };

        //    Process gitProcess = new Process
        //    {
        //        StartInfo = gitCloneInfo,
        //    };

        //    gitProcess.OutputDataReceived += GitProcess_OutputDataReceived;
        //    gitProcess.ErrorDataReceived += GitProcess_ErrorDataReceived;

        //    gitProcess.Start();
        //    gitProcess.BeginOutputReadLine();
        //    gitProcess.BeginErrorReadLine();
        //    gitProcess.WaitForExit();
        //}

        //public void GitProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data != null)
        //    {
        //        Console.WriteLine(e.Data);
        //    }
        //}

        //public void GitProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data != null)
        //    {
        //        Console.WriteLine(e.Data);
        //    }
        //}
    }
}
