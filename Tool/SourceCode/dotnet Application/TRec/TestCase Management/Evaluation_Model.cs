using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCase_Management
{
    internal class Evaluation_Model
    {
        public string Commit { get; set; }
        public string Datetime { get; set; }
        public string ProductionClass { get; set; }
        public string ChangedProductionMethods { get; set; }
        public string TestClass { get; set; }
        public string ChangedTestMethods { get; set; }
        public string CalledTestMethods { get; set; }
        public string RecomendedTests { get; set; }
        public string RecomendedIndex { get; set; }
    }
}
