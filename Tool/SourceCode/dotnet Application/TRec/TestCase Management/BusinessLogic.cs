using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiffPlex.Chunkers;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection.Emit;

namespace TestCase_Management
{
    public class BusinessLogic
    {

        public List<string> ListFiles(string folderPath)
        {
            List<string> fileList = new List<string>();

            // Get all files in the current folder
            string[] files = Directory.GetFiles(folderPath);
            fileList.AddRange(files);

            // Recursively get files from subfolders
            string[] subDirectories = Directory.GetDirectories(folderPath);
            foreach (string subDirectory in subDirectories)
            {
                fileList.AddRange(ListFiles(subDirectory));
            }

            return fileList;
        }

        public List<FileInfo> ValidateFiles(List<string> theFilesList)
        {
            List<FileInfo> theFiles = new List<FileInfo>();
            foreach (string theFilePath in theFilesList)
            {
                try
                {
                    FileInfo theFileInfo = new FileInfo(theFilePath);
                    theFiles.Add(theFileInfo);
                }
                catch { }

            }

            return theFiles;
        }

        public List<FileInfo> getAllFiles(string folderPath)
        {
            return ValidateFiles(ListFiles(folderPath));
        }
        public void CompareFilesInFolder(List<FileInfo> version1AllFiles, List<FileInfo> version2AllFiles)
        {

            foreach (FileInfo theVersion1File in version1AllFiles)
            {
                if (theVersion1File.Extension.ToLower() != ".java")
                    continue;
                if (theVersion1File.Name.ToLower().Contains("test") && theVersion1File.FullName.ToLower().Contains("\\test\\"))
                    continue;

                FileInfo theVersion2File = version2AllFiles.FirstOrDefault(s => s.Name == theVersion1File.Name);

                if (theVersion2File != null && theVersion2File.Exists)
                {
                    string fileName = Path.GetFileName(MasterObject.filesChanged.FirstOrDefault());
                    // Compare the content of the source and target files
                    if (!FilesAreEqual(theVersion1File, theVersion2File) && MasterObject.filesChanged.Contains( theVersion1File.Name))
                    {
                        //Project.Add(theVersion1File, theVersion2File);

                    }
                }
            }
        }
        public bool FilesAreEqual(FileInfo file1, FileInfo file2)
        {
            // Compare file sizes
            if (file1.Length == file2.Length)
            {
                return true;
            }

            // Compare file content byte by byte
            using (FileStream fs1 = file1.OpenRead())
            using (FileStream fs2 = file2.OpenRead())
            {
                int byte1, byte2;

                do
                {
                    byte1 = fs1.ReadByte();
                    byte2 = fs2.ReadByte();

                    if (byte1 != byte2)
                    {
                        return false;
                    }
                } while (byte1 >= 0 && byte2 >= 0);
            }

            return true;
        }

        public List<string> ExtractMethodSignatures(string javaCode)
        {
            List<string> methodSignatures = new List<string>();
            List<string> ctors = new List<string>();
            List<string> accessModifiers = new List<string>() { "public", "private", "protected", "internal", "protected internal", "private protected" };
            // Regular expression to match method signatures
            string pattern = @"(\s*((public|private|protected|static|final|abstract)?=*\s*(\w+)\s+(\w+)\s*\([^)]*\)\s*)\{)";
            //string pattern = @"^(?<modifier>(public|private|protected|internal|static|void)?\s*)?(?<returnType>[a-zA-Z_][a-zA-Z0-9_<>, ]*)\s+(?<methodName>[a-zA-Z_][a-zA-Z0-9_]*)\s*\((?<parameters>[^\)]*)\)"; 

            MatchCollection matches = Regex.Matches(javaCode, pattern);

            foreach (Match match in matches)
            {
               // match.Value.Trim();
                if (match.Groups.Count >= 5)
                {
                    string visibility = match.Groups[3].Value.Trim();
                    string returnType = match.Groups[4].Value.Trim();
                    string methodName = match.Groups[5].Value.Trim();

                    if (String.IsNullOrEmpty(visibility) && returnType != null && accessModifiers.Contains(returnType.Trim()))
                    {
                        //This is a ctor not method. 
                        string methodSignature = $"{methodName}()";
                        ctors.Add(methodSignature);
                    }
                    else {
                        //string methodSignature = $"{visibility} {returnType} {methodName}()";
                        string methodSignature = $"{methodName}()";
                        methodSignatures.Add(methodSignature);
                    }
                }
            }

            return methodSignatures;
        }

        public string ExtractMethod(string javaCode, string methodName)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(javaCode);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            var v = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (MethodDeclarationSyntax method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (methodName.Contains(method.Identifier.ValueText))
                {
                    return method.ToFullString();
                }
            }

            return null;
            //// Regular expression to match the specified method content
            //string pattern = $@"(\s*(public|private|protected|static|final|abstract|\s)*\s*(\w+)\s+{methodName}\(\)\s*\([^)]*\)\s*\{{[^{{}}]*\}})";

            //Match match = Regex.Match(javaCode, pattern, RegexOptions.Singleline);

            //if (match.Success)
            //{
            //    return match.Groups[0].Value.Trim();
            //}

            //return null;
        }
        public void FindChangedSourceMethods(SourceClass sourceClass)
        {

            new Bl_Antlr().CompareCodes(sourceClass);
            //var oldMethods = ExtractMethodSignatures(sourceClass.SourceClass_Raw_V1_FileContent);
            //var newMethods = ExtractMethodSignatures(sourceClass.SourceClass_Raw_V2_FileContent);

            ////Just Validating Method Signature Not full Method. 
            //foreach (var oldMethod in oldMethods)
            //{
            //    if (newMethods.Any(x => x == oldMethod))
            //    {
            //        var oldmethodSnip = ExtractMethod(sourceClass.SourceClass_Raw_V1_FileContent, oldMethod);
            //        var newMethodSnip = ExtractMethod(sourceClass.SourceClass_Raw_V2_FileContent, newMethods.First(x => x == oldMethod));
            //        if (oldmethodSnip != null && newMethodSnip != null)
            //            if (oldmethodSnip.Trim() != newMethodSnip.Trim())
            //            {
            //                MethodSet theMethodSet = new MethodSet();
            //                theMethodSet.SourceMethodName = oldMethod;
            //                theMethodSet.SourceMethodV1Snip = oldmethodSnip;
            //                theMethodSet.SourceMethodV2Snip = newMethodSnip;
            //                sourceClass.MethodSets.Add(theMethodSet);
            //            }

            //    }
            //}
        }
        public void FindAssociatedTestMethods(SourceClass theSourceClass, List<FileInfo> theFilesList)
        {
            foreach (FileInfo theFile in theFilesList)
            {
                if (!theFile.Name.ToLower().EndsWith("java"))
                    continue;

                var methods = ExtractMethodSignatures(File.ReadAllText(theFile.FullName));
                if (methods != null && methods.Count > 0)
                {
                    foreach (string method in methods)
                    {
                        var methodSnip = ExtractMethod(theFile.FullName, method);
                        if (methodSnip != null)
                        {
                            foreach (var sourceMethod in theSourceClass.MethodSets)
                            {
                                string sourceMethodOnlyName = sourceMethod.SourceMethodName.Substring(sourceMethod.SourceMethodName.Length - 2);
                                if (methodSnip.Contains(sourceMethodOnlyName))
                                {
                                    int Priority = 0;

                                    //If the Test Method is in "Test" Folder Consider them first
                                    if (theFile.FullName.ToLower().Contains("/test/"))
                                    {
                                        Priority = 2;
                                    }
                                    //if the filename Contains the prefix or suffix as TEST that has same name as Source Name
                                    else if (theFile.Name.ToLower()
                                        .Contains(
                                        Path.GetFileNameWithoutExtension(theSourceClass.SourceClassFileName).ToLower() + "_test"
                                        ) ||
                                        theFile.Name.ToLower()
                                        .Contains(
                                            "test_" + Path.GetFileNameWithoutExtension(theSourceClass.SourceClassFileName).ToLower()
                                        ) ||
                                        theFile.Name.ToLower()
                                        .Contains(
                                        Path.GetFileNameWithoutExtension(theSourceClass.SourceClassFileName).ToLower() + "test"
                                        ) ||
                                        theFile.Name.ToLower()
                                        .Contains(
                                            "test" + Path.GetFileNameWithoutExtension(theSourceClass.SourceClassFileName).ToLower()
                                        )
                                        )
                                    {
                                        Priority = 1;
                                    }
                                    else
                                    {
                                        Priority = 3;
                                    }
                                    if (!sourceMethod.TestMethods.Any(x => x.TestMethodName == method && x.TestClasssFileName == theFile.Name))
                                    {

                                        TestMethod theTestMethod = new TestMethod();
                                        theTestMethod.Priority = Priority;
                                        theTestMethod.TestMethodName = method;
                                        theTestMethod.TestClassFile_Raw = theFile;
                                        theTestMethod.TestClassFilePath = theFile.FullName;

                                        sourceMethod.TestMethods.Add(theTestMethod);
                                    }
                                }

                            }
                        }
                    }
                }

            }

        }

        //public void FindSourceMethodAndTestMethod(FilesSet Details)
        //{
        //    //Sets theSet = new Sets();
        //    // Read the code from the files
        //    string oldCode = File.ReadAllText(Details.OldFile);
        //    string newCode = File.ReadAllText(Details.NewFile);

        //    //// Create syntax trees for the old and new code
        //    //SyntaxTree oldTree = CSharpSyntaxTree.ParseText(oldCode);
        //    //SyntaxTree newTree = CSharpSyntaxTree.ParseText(newCode);

        //    //// Get the root nodes of the syntax trees
        //    //var oldRoot = oldTree.GetRoot();
        //    //var newRoot = newTree.GetRoot();


        //    //// Retrieve method declarations from the old and new syntax trees
        //    //var oldMethods = oldRoot.DescendantNodes().OfType<MethodDeclarationSyntax>();
        //    //var newMethods = newRoot.DescendantNodes().OfType<MethodDeclarationSyntax>();

        //    // Retrieve method declarations from the old and new syntax trees
        //    var oldMethods = ExtractMethodSignatures(oldCode);
        //    var newMethods = ExtractMethodSignatures(newCode);

        //    try
        //    {
        //        // Compare the methods
        //        foreach (var oldMethod in oldMethods)
        //        {
        //            //var matchingMethod = newMethods.FirstOrDefault(newMethod =>
        //            //    oldMethod.Identifier.ValueText == newMethod.Identifier.ValueText);

        //            if (newMethods.Any(x => x == oldMethod))
        //            {
        //                //string oldMethodCode = ExtractMethod(oldCode, oldMethod);
        //                //string newMethodCode = ExtractMethod(newCode, oldMethod);
        //                //if (oldMethodCode.Trim() == newMethodCode.Trim())
        //                {
        //                    new FindTestClassAndMethod().FindTestFileNameAndMethod(Details.OldFile, oldMethod);
        //                    new FindTestClassAndMethod().FindTestFileNameAndMethod(Details.NewFile, oldMethod);
        //                    //return Sets.theSets.FirstOrDefault(x => x.SourceMethod == oldMethod);
        //                }
        //            }

        //            //if (matchingMethod != null)
        //            //{
        //            //    // Compare method bodies
        //            //    if (oldMethod.Body != null && matchingMethod.Body != null)
        //            //        if (oldMethod.Body.ToString() != matchingMethod.Body.ToString())
        //            //        {
        //            //            //   MessageBox.Show($"Method '{matchingMethod.Identifier.ValueText}' body was changed");
        //            //            new FindTestClassAndMethod().FindTestFileNameAndMethod(Details.NewFile, matchingMethod.Identifier.ValueText);
        //            //            return Sets.theSets.FirstOrDefault(x => x.SourceMethod == matchingMethod.Identifier.ValueText);
        //            //        }
        //            //}
        //            //else
        //            //{
        //            //    MessageBox.Show($"Method '{oldMethod.Identifier.ValueText}' was removed");
        //            //}
        //        }
        //    }
        //    catch { }
        //    //return null;
        //}

        //public Dictionary<string, int> ShowTestMethodsFromHistoricalData(FilesSet theDetails)
        //{
        //    Dictionary<string, int> fileCounter = new Dictionary<string, int>();

        //    foreach (History theHistory in MasterObject.theHistoryes)
        //    {
        //        theHistory.ValidateFiles();
        //        if (theHistory.theFilesChanged.Any(x => x.Name == Path.GetFileName(theDetails.NewFile)))
        //        {
        //            foreach (FileInfo theFile in theHistory.theFilesChanged)
        //            {
        //                if (fileCounter.ContainsKey(theFile.Name))
        //                {
        //                    fileCounter[theFile.Name]++;
        //                }
        //                else
        //                {
        //                    fileCounter[theFile.Name] = 1;
        //                }
        //            }


        //            //MessageBox.Show(theHistory.ToString());
        //            //theDetails.TestMethods_HistoricalData.Add(theHistory.ToString());
        //        }
        //    }
        //    return fileCounter;
        //}


    }
}
