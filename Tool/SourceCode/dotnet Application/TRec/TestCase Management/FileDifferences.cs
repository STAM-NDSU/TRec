using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCase_Management
{
    public class FileDifferences
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int LineIndex { get; set; }
        public int LineNumber { get; set; }
        public string MethodName { get; set; }
        public string TestMethodName { get; set; }

    }
}
