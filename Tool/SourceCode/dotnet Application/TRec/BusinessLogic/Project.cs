using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class Project
    {
        public string lastCommitID { get; set; }
        public List<SourceClass> theSourceClasses = new List<SourceClass>();
        public List<string> AnalysedCommits = new List<string>();
        public void Add(FileInfo version1Class, FileInfo version2Class)
        {
            SourceClass theSourceClass = new SourceClass();
            theSourceClass.SourceClass_Raw_V1 = version1Class;
            theSourceClass.SourceClass_Raw_V2 = version2Class;
            //theSourceClass.SourceClassFileName = version1Class.Name;
            theSourceClass.SourceClass_Raw_V1_FileContent = File.ReadAllText(version1Class.FullName);
            theSourceClass.SourceClass_Raw_V2_FileContent = File.ReadAllText(version2Class.FullName);
            theSourceClasses.Add(theSourceClass);
        }
    }
    public class TestMethod_Analytic
    {
        public string TestMethodName { get; set; }
        public string TestFileName { get; set; }
        public int Frequency { get; set; }
        public List<string> ChangedDateTime = new List<string>();
        public List<DateTimeOffset> ParsedChangedDateTime
        {
            get
            {
                return ChangedDateTime
                    .Select(dateTimeString =>
                    {
                        // Try to parse each string to a DateTimeOffset
                        DateTimeOffset.TryParse(dateTimeString, out DateTimeOffset parsedDateTime);
                        return parsedDateTime;
                    })
                    .ToList();
            }
        }
        public HashSet<DateTimeOffset> Parsed_ChangedDateTime = new HashSet<DateTimeOffset>();

    }

    [DebuggerDisplay("Name = {TestMethodName}")]
    public class TestMethod
    {
        public string TestClasssFileName { get; set; }
        public string TestClassFilePath { get; set; }
        public string TestClass_Raw_FileContent_V1 { get; set; }
        public string TestClass_Raw_FileContent_V2 { get; set; }
        public FileInfo TestClassFile_Raw { get; set; }
        public string TestMethodName { get; set; }
        public string TestMethodName_Raw { get; set; }
        public string TestMethodSnip { get; set; }
        public int Priority { get; set; } = 0;
        public string CommitedDateTime { get; set; }
        public bool isAnalysed { get; set; } = false;
        public bool isValidTestMethod { get; set; } = true;
        public List<string> MethodCallsInside { get; set; }

    }

    [DebuggerDisplay("Name = {SourceMethodName}")]
    public class MethodSet
    {
        public string SourceMethodName { get; set; }
        public string SourceMethodName_Raw { get; set; }
        public string SourceMethodV1Snip { get; set; }
        public string SourceMethodV2Snip { get; set; }
        public List<TestMethod> TestMethods = new List<TestMethod>();
        public List<TestMethod_Analytic> TestMethods_Analytics = new List<TestMethod_Analytic>();

    }

    [DebuggerDisplay("Name = {SourceClassFileName}")]
    public class SourceClass
    {
        public string SourceClassFileName { get { return Path.GetFileName(SourceClassFilePath); } }
        public string SourceClassFilePath { get; set; }
        public FileInfo SourceClass_Raw_V1 { get; set; }
        public FileInfo SourceClass_Raw_V2 { get; set; }

        public string SourceClass_Raw_V1_FileContent { get; set; }

        public string SourceClass_Raw_V2_FileContent { get; set; }

        public List<MethodSet> MethodSets = new List<MethodSet>();
        public string SourceClassFileNameWithOutExtension { get; set; }


    }
}
