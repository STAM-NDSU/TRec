using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCase_Management
{
    public class FilesSet
    {
        public FilesSet(string oldFile,string newFile)
        {
            this.OldFile = oldFile;
            this.NewFile = newFile;
        }
        public string OldFile { get; set; }
        public string NewFile { get; set; }

        public List<string> TestMethods_HistoricalData { get; set; }
        public List<string> TestMethods_Algo { get; set; }
    }
}
