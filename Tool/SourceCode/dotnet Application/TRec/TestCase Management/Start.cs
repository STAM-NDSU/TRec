using BusinessLogic;
using LibGit2Sharp;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Commit = BusinessLogic.Commit;

namespace TestCase_Management
{
    public partial class Start : Form
    {

        public Start()
        {
            InitializeComponent();
        }

        public void Reset()
        {
            MasterObject.theHistoryes.Clear();
            MasterObject.CurrentProject = new Project();
        }

        public async void button3_ClickAsync(object sender, EventArgs e)
        {

            //MasterObject.flag_AllGood = true;
            //MasterObject.theGUID = "Git\\" + DateTime.Now.ToString("yyyyMMddTHHmmss");


            if (rd_new.Checked)
            {
                //Validations
                if (RepoWareHouse.RepoMetadata.Any(x => x.RepoURL == tb_URL.Text))
                {
                    DialogResult dialogResult = MessageBox.Show("Repo Already Analysed and Saved Data Already Exits. Do you want to OverWrite?", "Warning..!", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        RepoWareHouse.RepoMetadata.Remove(RepoWareHouse.RepoMetadata.Find(x => x.RepoURL == tb_URL.Text));
                    }
                }
                //Initial Validations
                // Clone the repository to a temporary directory
                Helper.SetStatus("Cloning Repository in Progress..!");
                //if(!Repository.IsValid(tb_URL.Text))
                //{
                //    MessageBox.Show("Invalid Repo URL.");
                //    return;
                //}
                string tempRepoPath = await Task.Run(() =>
                {
                    return Repository.Clone(tb_URL.Text, GetTempDirectory());
                });
                MasterObject.tempRepoPath = tempRepoPath;
                Helper.SetStatus("Cloning Repository Completed.!");

                //Download all the commits that has valid Parent
                Helper.SetStatus("Downloading All Commits that has Parent Branch -  in Progress..!");
                await Task.Run(() =>
                {
                    new RepositoryAnalyzer().DownloadCommits(tempRepoPath);
                });
                Helper.SetStatus("Downloading All Commits that has Parent Branch -  Completed..!");

                //Find All the Valid Commits for our Analysis. 
                Helper.SetStatus("Filterining All the Valid Commits for Analysis -  In Progress.!");
                await Task.Run(() =>
                {
                    new RepositoryAnalyzer().FindValidCommits();
                });
                Helper.SetStatus("Filterining All the Valid Commits for Analysis - Completed.!");

                RepoMetaData repoMetaData = new RepoMetaData();
                repoMetaData.TotalValidCommits = MasterObject.ValidCommitsForAnalysis.Select(x => x.CommitId).ToList();
                repoMetaData.TotalCommits = MasterObject.Commits.Select(x => x.CommitId).ToList();
                repoMetaData.RepoURL = tb_URL.Text;
                repoMetaData.RepoName = tb_URL.Text.Split('/').Last().Replace(".git", "");
                RepoWareHouse.RepoMetadata.Add(repoMetaData);
                MasterObject.CurrentRepoMetaData = repoMetaData;
                CRUDManager.Save_Updated_RepoWareHouse();

                MasterObject.Commits.Clear();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                int totalCommits = MasterObject.ValidCommitsForAnalysis.Count();
                int counter = 0;
                int flagCounter = 0;
                await Task.Run(() =>
                {
                    while (MasterObject.ValidCommitsForAnalysis.Any())
                    {
                        List<Commit> batch = null;
                        if (MasterObject.ValidCommitsForAnalysis.Count > 25)
                            batch = MasterObject.ValidCommitsForAnalysis.Take(25).ToList();
                        else
                            batch = MasterObject.ValidCommitsForAnalysis.ToList();
                        foreach (var theCommit in batch)
                        {
                            counter++;
                            flagCounter++;
                            Helper.SetStatus($"Step 1/2 of Analysed {counter} / {totalCommits}");
                            new Antlr_RepoAnalysis().FindSourceAndTestClasses(theCommit);
                        }
                        CRUDManager.Save_Updated_Project(MasterObject.CurrentRepoMetaData.RepoName);
                        if (MasterObject.ValidCommitsForAnalysis.Count > 25)
                            MasterObject.ValidCommitsForAnalysis.RemoveRange(0, 25);
                        else
                            MasterObject.ValidCommitsForAnalysis.Clear();

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                });
                CRUDManager.Save_Updated_Project(MasterObject.CurrentRepoMetaData.RepoName);
            }
            else if (rd_alreadyanalysed.Checked)
            {

                bool SelectedRadioFound = false;
                foreach (Control theControl in flp_SavedProjects.Controls)
                {
                    RadioButton theRadio = (RadioButton)theControl;
                    if (theRadio.Checked)
                    {

                        MasterObject.CurrentRepoMetaData = RepoWareHouse.RepoMetadata.FirstOrDefault(x => x.RepoName == theRadio.Text);
                        if (MasterObject.CurrentRepoMetaData != null)
                        {
                            CRUDManager.LoadProject(MasterObject.CurrentRepoMetaData.RepoName);
                            SelectedRadioFound = true;
                            break;
                        }

                    }
                }

                if (SelectedRadioFound == false)
                {
                    MessageBox.Show("Please Selected the Project from the list.");
                    return;
                }

                Helper.SetStatus("Cloning Repository in Progress..!");
                string tempRepoPath = await Task.Run(() =>
                {
                    return Repository.Clone(MasterObject.CurrentRepoMetaData.RepoURL, GetTempDirectory());
                });
                MasterObject.tempRepoPath = tempRepoPath;
                Helper.SetStatus("Cloning Repository Completed.!");

                Helper.SetStatus("Downloading commit to display recommendations for - in Progress..!");
                if (!String.IsNullOrEmpty(tb_CommitID.Text))
                {
                    MasterObject.CommitForRecomendation = new RepositoryAnalyzer().DownloadSingleCommit(tempRepoPath, tb_CommitID.Text);
                }
                Helper.SetStatus("Downloading commit to display recommendations for - Completed!");


                //Download all the commits that has valid Parent
                Helper.SetStatus("Downloading All Commits that has Parent Branch -  in Progress..!");
                await Task.Run(() =>
                {
                    new RepositoryAnalyzer().DownloadCommits(tempRepoPath, true);
                });
                Helper.SetStatus("Downloading All Commits that has Parent Branch -  Completed..!");

                //Find All the Valid Commits for our Analysis. 
                Helper.SetStatus("Filterining All the Valid Commits for Analysis -  In Progress.!");

                MasterObject.CurrentRepoMetaData.TotalCommits.AddRange(MasterObject.Commits.Select(x => x.CommitId));
                CRUDManager.Save_Updated_RepoWareHouse();

                await Task.Run(() =>
                {
                    new RepositoryAnalyzer().FindValidCommits();
                });
                Helper.SetStatus("Filterining All the Valid Commits for Analysis - Completed.!");

                int totalCommits = MasterObject.ValidCommitsForAnalysis.Count();
                int counter = 0;
                int flagCounter = 0;
                await Task.Run(() =>
                {
                    while (MasterObject.ValidCommitsForAnalysis.Any())
                    {
                        List<Commit> batch = null;
                        if (MasterObject.ValidCommitsForAnalysis.Count > 25)
                            batch = MasterObject.ValidCommitsForAnalysis.Take(25).ToList();
                        else
                            batch = MasterObject.ValidCommitsForAnalysis.ToList();
                        foreach (var theCommit in batch)
                        {
                            counter++;
                            flagCounter++;
                            Helper.SetStatus($"Analysed {counter} / {totalCommits}");
                            if (MasterObject.CurrentProject.AnalysedCommits.Any(x => x == theCommit.CommitId))
                                continue;
                            new Antlr_RepoAnalysis().FindSourceAndTestClasses(theCommit);
                        }
                        //CRUDManager.Save_Updated_Project(MasterObject.CurrentRepoMetaData.RepoName);
                        if (MasterObject.ValidCommitsForAnalysis.Count > 25)
                            MasterObject.ValidCommitsForAnalysis.RemoveRange(0, 25);
                        else
                            MasterObject.ValidCommitsForAnalysis.Clear();

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                });
                CRUDManager.Save_Updated_Project(MasterObject.CurrentRepoMetaData.RepoName);
            }

            string lastCommitSha = new RepositoryAnalyzer().getLastCommitSha_ID(MasterObject.tempRepoPath);
            if (MasterObject.CurrentProject.lastCommitID != lastCommitSha)
            {
                //now perform the analysis of finding correct test class. 
                int con = 0;
                int totalsourceclass = MasterObject.CurrentProject.theSourceClasses.Count();
                await Task.Run(() =>
                {
                    foreach (var theSourceClass in MasterObject.CurrentProject.theSourceClasses)
                    {
                        con++;
                        int total_changed_SrcMethods = theSourceClass.MethodSets.Count();
                        int src_count = 0;
                        foreach (var theChangedSourceMethodSets in theSourceClass.MethodSets)
                        {
                            src_count++;

                            int totalmethodsets = theChangedSourceMethodSets.TestMethods.Count();
                            int method_con = 0;
                            foreach (var theTestMethod_raw in theChangedSourceMethodSets.TestMethods)
                            {
                                method_con++;

                                Helper.SetStatus($"Step 2/2 of Analysed: Cls: {con} / {totalsourceclass} - CngSrc {src_count}/{total_changed_SrcMethods}, TstCls: {method_con} / {totalmethodsets}");

                                if (theTestMethod_raw.isAnalysed == true)
                                    continue;


                                string methodsignature = string.Empty;
                                try
                                {
                                    methodsignature = new Bl_Antlr().ExtraxtMethodNameOnly(theChangedSourceMethodSets.SourceMethodName);
                                }
                                catch { }

                                if (String.IsNullOrEmpty(methodsignature))
                                {
                                    theTestMethod_raw.isAnalysed = true;
                                    continue;
                                }
                                if (!theTestMethod_raw.TestClass_Raw_FileContent_V2.Contains(methodsignature) &&
                                    !theTestMethod_raw.TestClass_Raw_FileContent_V1.Contains(methodsignature))
                                {
                                    theTestMethod_raw.isAnalysed = true;
                                    theTestMethod_raw.isValidTestMethod = false;
                                    continue;
                                }


                                var changedTestMethods = new Antlr_RepoAnalysis().FindChangedMethods(theTestMethod_raw.TestClass_Raw_FileContent_V2, theTestMethod_raw.TestClass_Raw_FileContent_V1);

                                if (changedTestMethods.Any())
                                {
                                    foreach (var theTestMethod in changedTestMethods)
                                    {
                                        if (!theChangedSourceMethodSets.TestMethods_Analytics.Any(x => x.TestMethodName == theTestMethod.SourceMethodName))
                                        {
                                            if (!theTestMethod.SourceMethodV1Snip.Contains(methodsignature) && !theTestMethod.SourceMethodV2Snip.Contains(methodsignature))
                                                continue;
                                            if (!theTestMethod.SourceMethodV1Snip.ToLower().Contains("public") && !theTestMethod.SourceMethodV2Snip.ToLower().Contains("public"))
                                                continue;
                                            TestMethod_Analytic thetestAna = new TestMethod_Analytic();
                                            thetestAna.TestFileName=theTestMethod_raw.TestClasssFileName;
                                            thetestAna.TestMethodName = theTestMethod.SourceMethodName;
                                            thetestAna.ChangedDateTime.Add(theTestMethod_raw.CommitedDateTime.ToString());
                                            theChangedSourceMethodSets.TestMethods_Analytics.Add(thetestAna);
                                        }
                                        else
                                        {
                                            var TestAna = theChangedSourceMethodSets.TestMethods_Analytics.FirstOrDefault(x => x.TestMethodName == theTestMethod.SourceMethodName);
                                            TestAna.ChangedDateTime.Add(theTestMethod_raw.CommitedDateTime.ToString());

                                        }
                                    }
                                }
                                theTestMethod_raw.isAnalysed = true;

                            }
                        }
                        CRUDManager.Save_Updated_Project(MasterObject.CurrentRepoMetaData.RepoName);
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                });
                MasterObject.CurrentProject.lastCommitID=lastCommitSha;
                CRUDManager.Save_Updated_Project(MasterObject.CurrentRepoMetaData.RepoName);
            }

            //new Evaluation_BL().StartEvaluation();


            panel3.Controls.Clear();
            Result theResult = new Result();
            theResult.FormBorderStyle = FormBorderStyle.None;
            theResult.Dock = DockStyle.Fill;
            theResult.TopLevel = false;
            panel3.Controls.Add(theResult);
            theResult.Show();

            /*

            ////////////////////////////////////////////////////////////////////////////////////

            if (!MasterObject.flag_AllGood)
                return;

            Thread th1 = new Thread(() => new Loading("Check Out In Progress..!").ShowDialog());
            th1.Start();

            string tempRepopath = await Task.Run(() =>
            {
                return Repository.Clone(MasterObject.CurrentRepoMetaData.RepoURL, GetTempDirectory());
            });
            //tb_CommitID.Text = new GitDownload().CheckoutSpecificCommit(tempRepopath, tb_CommitID.Text);

            MasterObject.version1Directory = tb_CommitID.Text;
            // MasterObject.version2Directory = textBox4.Text;
            th1.Abort();



            if (!MasterObject.flag_AllGood)
                return;

            Thread th = new Thread(() => new Loading("Running Algorithm..!").ShowDialog());
            th.Start();

            MasterObject.version1AllFiles = new BusinessLogic().getAllFiles(tb_CommitID.Text);
            //MasterObject.version2AllFiles = new BusinessLogic().getAllFiles(textBox4.Text);



            //Pass the Both Versions All the Files
            new BusinessLogic().CompareFilesInFolder(MasterObject.version1AllFiles, MasterObject.version2AllFiles);

            List<History> theValidHistory = new List<History>();
            foreach (SourceClass theSourceClass in MasterObject.CurrentProject.theSourceClasses)
            {

                foreach (History theHistory in MasterObject.theHistoryes)
                {
                    if (theHistory.FilesChanged.Any(x => x.Contains(theSourceClass.SourceClassFileName)))
                    {
                        theValidHistory.Add(theHistory);
                    }
                }
                //if(MasterObject.theHistoryes.Any(x=>x.FilesChanged.Contains(theSourceClass.SourceClassFileName)))
                //{
                //    theValidHistory.AddRange(MasterObject.theHistoryes.FindAll(x => x.FilesChanged.Contains(theSourceClass.SourceClassFileName)));
                //}
            }

            foreach (History theHistory in theValidHistory)
            {
                //Needs Fixning
                //new GitDownload().CheckoutSpecificCommit(tempRepoPath, theHistory.CommitID);
                new GitDownload().CheckoutSpecificCommit("", theHistory.CommitID);
            }

            th.Abort();

            
            panel3.Controls.Clear();
            Result theResult = new Result();
            theResult.FormBorderStyle = FormBorderStyle.None;
            theResult.Dock = DockStyle.Fill;
            theResult.TopLevel = false;
            panel3.Controls.Add(theResult);
            theResult.Show();

            //var test = JsonConvert.SerializeObject(Project.theSourceClasses, Formatting.Indented);
            //File.WriteAllText("~\\MasterObject.json", test);
            //MessageBox.Show("Comparision Finished!");
            */

        }

        //public List<FilesSet> CompareFolders(string folder1Path, string folder2Path)
        //{
        //     return new BusinessLogic().CompareFilesInFolder(folder1Path, folder2Path);

        //}

        private void button5_Click(object sender, EventArgs e)
        {
            //new RunReport().ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //new RunReport().ShowDialog();
        }

        private static string GetTempDirectory()
        {
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string result = "";
            foreach (History theHistory in MasterObject.theHistoryes)
            {
                result = result + "\n" + theHistory.ToString();
            }
            MessageBox.Show(result);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    tb_CommitID.Text = fbd.SelectedPath;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    // textBox4.Text = fbd.SelectedPath;
                }
            }
        }

        private void Start_Load(object sender, EventArgs e)
        {

            rd_Type_CheckedChanged(sender, e);
            CRUDManager.LoadRepoWareHouse();
            Reset();
            LoadSavedGroupBox();
            Helper.StatusLabel = this.lbl_Status;

        }
        private void LoadSavedGroupBox()
        {
            foreach (var TheRepoMetaData in RepoWareHouse.RepoMetadata)
            {
                RadioButton theRadio = new RadioButton();
                theRadio.Text = TheRepoMetaData.RepoName;
                theRadio.AutoSize = true;
                flp_SavedProjects.Controls.Add(theRadio);
                flp_SavedProjects.SetFlowBreak(theRadio, true);
            }
        }

        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }


        private void rd_Type_CheckedChanged(object sender, EventArgs e)
        {
            if (rd_new.Checked)
            {
                gb_RepDetails.Visible = true;
                gb_SelectRep.Visible = false;
                //gb_AdditionalOptions.Visible = false;
                btn_InitialAnalysis.Enabled = true;
            }
            else
            {
                gb_RepDetails.Visible = false;
                //gb_AdditionalOptions.Visible = true;
                gb_SelectRep.Visible = true;
                btn_InitialAnalysis.Enabled = false;
            }

        }

        private void chbk_IncludeNewCommits_CheckedChanged(object sender, EventArgs e)
        {
            if (chbk_IncludeNewCommits.Checked)
                btn_InitialAnalysis.Enabled = true;
            if (!chbk_IncludeNewCommits.Checked)
                btn_InitialAnalysis.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (!string.IsNullOrEmpty(tb_CommitID.Text))
            {
                panel3.Controls.Clear();
                Result theResult = new Result();
                theResult.FormBorderStyle = FormBorderStyle.None;
                theResult.Dock = DockStyle.Fill;
                theResult.TopLevel = false;
                panel3.Controls.Add(theResult);
                theResult.Show();
            }
        }
    }
}
