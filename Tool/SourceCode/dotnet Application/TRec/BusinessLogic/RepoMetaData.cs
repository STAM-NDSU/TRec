using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class RepoMetaData
    {
        public string RepoName { get; set; }

        public string RepoURL { get; set; }
        public List<string> TotalCommits { get; set; } = new List<string>();
        public List<string> TotalValidCommits { get; set; } = new List<string>();
        public string SavedDataBaseFileNamewithPath { get; set; }
        public string ProjectName
        {
            get
            {
                return Path.GetFileName(RepoURL.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }
        }
    }
    public static class RepoWareHouse
    {
        public static List<RepoMetaData> RepoMetadata = new List<RepoMetaData>();
    }
}
