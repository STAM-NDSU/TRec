using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public enum UIType
    {
        console = 0,
        GUI = 1
    }
    public static class MasterObject
    {
        public static string theGUID = null;
        public static List<FileInfo> version1AllFiles { get; set; }
        public static List<FileInfo> version2AllFiles { get; set; }
        public static string version1Directory { get; set; }
        public static string version2Directory { get; set; }

        public static List<string> filesChanged;
        public static List<string> commitsBetween;
        public static bool flag_AllGood { get; set; } = true;

        public static List<Commit> Commits = new List<Commit>();
        public static List<Commit> ValidCommitsForAnalysis = new List<Commit>();

        public static Project CurrentProject = new Project();

        public static RepoMetaData CurrentRepoMetaData = null;
        public static string RepoName { get; set; }
        public static string CommitShaForRecomendation { get; set; }

        public static string tempRepoPath { get; set; }
        public static UIType UIType { get; set; } = UIType.GUI;

    }

}
