using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestCase_Management
{
    public class FindTestClassAndMethod
    {
        public void FindTestFileNameAndMethod(string filePath, string MethodName)
        {
            string rootDirectory = Path.GetDirectoryName(filePath); // Replace with the appropriate root directory path
            string fileName = Path.GetFileName(filePath);
            string methodName = MethodName; // The original method name

            // Combine the root directory and testing folder path
            string testingFolderPath = Path.Combine(rootDirectory, "Testing");

            // Get all files in the testing folder with the matching pattern
            string[] matchingFiles = { };
            try
            {
                matchingFiles = Directory.GetFiles(testingFolderPath, $"{Path.GetFileNameWithoutExtension(fileName)}_Test.*");
            }
            catch { }

            if (matchingFiles.Length > 0)
            {
                //Found Testing file 
                //Now Find the test method

                foreach (string file in matchingFiles)
                {
                    //Console.WriteLine(file);

                    // Read the content of the associated file
                    string fileContent = File.ReadAllText(file);

                    // Create syntax trees for the old and new code
                    SyntaxTree oldTree = CSharpSyntaxTree.ParseText(fileContent);
                    var oldRoot = oldTree.GetRoot();
                    var oldMethods = oldRoot.DescendantNodes().OfType<MethodDeclarationSyntax>();

                    foreach (var oldMethod in oldMethods)
                    {
                        if (oldMethod.Identifier.ValueText.ToLower() == MethodName.ToLower() + "_test" || oldMethod.Identifier.ValueText.ToLower() == "test_" + MethodName.ToLower())
                        {
                            bool isTrue = Sets.theSets.FirstOrDefault(x => x.SourceMethod == methodName)?.TestMethods.Any(y => y.TestMethodFileName == Path.GetFileName(file)) ?? false;

                            if (isTrue)
                            {
                                Sets.theSets.FirstOrDefault(x => x.SourceMethod == methodName).TestMethods.FirstOrDefault(y => y.TestMethodFileName == Path.GetFileName(file)).TestMethodNames.Add(oldMethod.Identifier.ValueText);

                            }
                            else
                            {
                                Set theSet = new Set();
                                theSet.SourceMethod = methodName;
                                theSet.SourceMethodFilePath = filePath;
                                theSet.SourceMethodFileName = fileName;

                                TestMethod theTestMethod = new TestMethod();
                                theTestMethod.TestMethodFilePath = file;
                                theTestMethod.TestMethodFileName = Path.GetFileName(file);
                                theTestMethod.TestMethodNames.Add(oldMethod.Identifier.ValueText);
                                theSet.TestMethods.Add(theTestMethod);

                                Sets.theSets.Add(theSet);
                                
                            }

                        }
                    }
                }
            }
            List<string> getAllFiles = new List<string>();
            if (filePath.Contains(MasterObject.filePath_1))
                getAllFiles = MasterObject.alltheFiles_1;
            else if (filePath.Contains(MasterObject.filePath_2))
                getAllFiles = MasterObject.alltheFiles_2;

            if (getAllFiles.Count > 0)
            {
                //Found Testing file 
                //Now Find the test method

                foreach (string file in getAllFiles)
                {
                    //Console.WriteLine(file);

                    // Read the content of the associated file
                    string fileContent = File.ReadAllText(file);

                    // Create syntax trees for the old and new code
                    SyntaxTree oldTree = CSharpSyntaxTree.ParseText(fileContent);
                    var oldRoot = oldTree.GetRoot();
                    var oldMethods = oldRoot.DescendantNodes().OfType<MethodDeclarationSyntax>();

                    foreach (var oldMethod in oldMethods)
                    {
                            if (oldMethod.Identifier.ValueText.ToLower() == MethodName.ToLower() + "_test" || oldMethod.Identifier.ValueText.ToLower() == "test_" + MethodName.ToLower())
                            {
                                if (Sets.theSets.Any(x => x.SourceMethod == methodName && x.TestMethods.Any(y => y.TestMethodFileName == Path.GetFileName(file))))
                                {
                                    Sets.theSets.FirstOrDefault(x => x.SourceMethod == methodName).TestMethods.FirstOrDefault(y => y.TestMethodFileName == Path.GetFileName(file)).TestMethodNames.Add(oldMethod.Identifier.ValueText);

                                }
                                else
                                {
                                    Set theSet = new Set();
                                    theSet.SourceMethod = methodName;
                                    theSet.SourceMethodFilePath = filePath;
                                    theSet.SourceMethodFileName = fileName;

                                    TestMethod theTestMethod = new TestMethod();
                                    theTestMethod.TestMethodFilePath = file;
                                    theTestMethod.TestMethodFileName = Path.GetFileName(file);
                                    theTestMethod.TestMethodNames.Add(oldMethod.Identifier.ValueText);
                                    theSet.TestMethods.Add(theTestMethod);

                                    Sets.theSets.Add(theSet);

                                }
                            }
                            else if (oldMethod.Body!=null && oldMethod.Body.ToString().Contains(methodName))
                            {
                            bool isTrue = Sets.theSets.FirstOrDefault(x => x.SourceMethod == methodName)?.TestMethods.Any(y => y.TestMethodFileName == Path.GetFileName(file))??false;

                            if (isTrue)
                            {
                                Sets.theSets.FirstOrDefault(x => x.SourceMethod == methodName).TestMethods.FirstOrDefault(y => y.TestMethodFileName == Path.GetFileName(file)).TestMethodNames.Add(oldMethod.Identifier.ValueText);

                            }
                            else
                            {
                                Set theSet = new Set();
                                theSet.SourceMethod = methodName;
                                theSet.SourceMethodFilePath = filePath;
                                theSet.SourceMethodFileName = fileName;

                                TestMethod theTestMethod = new TestMethod();
                                theTestMethod.TestMethodFilePath = file;
                                theTestMethod.TestMethodFileName = Path.GetFileName(file);
                                theTestMethod.TestMethodNames.Add(oldMethod.Identifier.ValueText);
                                theSet.TestMethods.Add(theTestMethod);

                                Sets.theSets.Add(theSet);

                            }
                        }
                    }
                }
            }
        }

        static bool ContainsMethodCall(string code, string methodName)
        {
            // Regular expression pattern to match method calls
            string pattern = @"\b" + methodName + @"\b";

            // Match the pattern in the code
            MatchCollection matches = Regex.Matches(code, pattern);

            // Check if any matches are found
            return matches.Count > 0;
        }
    }
}
