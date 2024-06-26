using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{

    public class CandidateCommit
    {
        public string CommitSha { get; set; }
        public DateTimeOffset ChangedDateTime { get; set; }
        public string RepoLocation { get; set; }
        public List<string> ChangedFiles { get; set; } = new List<string>();
        public CommitAST CommitAST { get; set; }
        public CandidateCommit ParentCommit { get; set; }
        public bool IsAnalysed { get; set; } = false;
    }
}
