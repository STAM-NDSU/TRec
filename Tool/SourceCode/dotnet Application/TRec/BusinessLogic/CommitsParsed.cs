using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class ParsedCommits
    {
        public List<CommitAST> Commits { get; set; }
    }
    public class CommitAST
    {
        public string CommitID { get; set; }
        public List<JavaFile> theFiles { get; set; }
    }
    public class JavaFile
    {
        public string FileName { get; set; }
        public List<Method> MethodList { get; set; } = new List<Method>();
    }
    public class Method
    {
        public string MethodSignature_Raw { get; set; }
        public string MethodSignature { get; set; }
        public string CodeSnippet { get; set; }
        public List<string> MethodCalls { get; set; }
        public List<string> Annotations { get; set; }
    }
}
