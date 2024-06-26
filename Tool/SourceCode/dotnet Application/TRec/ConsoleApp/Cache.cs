using BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class Cache
    {
        public static List<CandidateCommit> theCommits = new List<CandidateCommit>();
        public static Dictionary<DateTimeOffset, string> theCommitsWithLinks = new Dictionary<DateTimeOffset, string>();
    }
}
