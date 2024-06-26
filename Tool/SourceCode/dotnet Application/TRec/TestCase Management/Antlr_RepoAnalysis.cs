using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic;

namespace TestCase_Management
{
    public class Antlr_RepoAnalysis
    {
        public JavaMethodExtractor ExtractMethods(string javaCode)
        {
            var inputStream = new AntlrInputStream(javaCode);
            var lexer = new Java8Lexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new Java8Parser(tokenStream);
            var tree = parser.compilationUnit();

            var extractor = new JavaMethodExtractor(inputStream);
            var walker = new ParseTreeWalker();
            walker.Walk(extractor, tree);

            return extractor;
        }

        public string ExtractMethodSignature(string methodSnippet)
        {
            var inputStream = new AntlrInputStream(methodSnippet);
            var lexer = new Java8Lexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new Java8Parser(tokenStream);

            // Using the methodDeclaration rule as the entry point for parsing
            var tree = parser.methodDeclaration();

            var extractor = new JavaMethodSignatureExtractor(inputStream);
            var walker = new ParseTreeWalker();
            walker.Walk(extractor, tree);

            return extractor.MethodSignature;
        }
        public void FindSourceAndTestClasses(Commit TheCommit)
        {

            List<File_Contents> testClass = new List<File_Contents>();
            List<File_Contents> sourceClass = new List<File_Contents>();
            if (TheCommit.ChangedFiles.Any(x => x.FileName.ToLower().Contains("test")))
            {
                foreach (var theFile in TheCommit.ChangedFiles)
                {
                    if (theFile.FileName.ToLower().Contains("enum") || !theFile.FileName.ToLower().EndsWith("java"))
                        continue;
                    if (theFile.FileName.ToLower().Contains("test"))
                    {
                        testClass.Add(theFile);
                    }
                    else
                    {
                        sourceClass.Add(theFile);
                    }
                }
            }

            foreach (var sourceCls in sourceClass)
            {
                if (testClass.Any(x => x.FileName.ToLower().Contains(sourceCls.FileNameWithOutExtension.ToLower())))
                {

                    if (!MasterObject.CurrentProject.theSourceClasses.Any(x => x.SourceClassFilePath == sourceCls.FilePath))
                    {
                        SourceClass srcCls = new SourceClass();
                        srcCls.SourceClassFilePath = sourceCls.FilePath;
                        //srcCls.SourceClass_Raw_V2_FileContent = sourceCls.Parent_Content;
                        //srcCls.SourceClass_Raw_V1_FileContent = sourceCls.Content;
                        MasterObject.CurrentProject.theSourceClasses.Add(srcCls);
                    }

                    SourceClass theSourceClass = MasterObject.CurrentProject.theSourceClasses.FirstOrDefault(x => x.SourceClassFilePath == sourceCls.FilePath);

                    if (theSourceClass != null)
                    {
                        var srcMethods = FindChangedMethods(sourceCls.Content, sourceCls.Parent_Content);
                        //if(!srcMethods.Any())
                        //    srcMethods = FindChangedMethods(theSourceClass.SourceClass_Raw_V2_FileContent,theSourceClass.SourceClass_Raw_V1_FileContent);
                        foreach (var srcMethod in srcMethods)
                        {
                            if(!theSourceClass.MethodSets.Any(x=>x.SourceMethodName== srcMethod.SourceMethodName))
                            {
                                theSourceClass.MethodSets.Add(srcMethod);
                            }
                            MethodSet sourceMethodSet = theSourceClass.MethodSets.FirstOrDefault(x => x.SourceMethodName == srcMethod.SourceMethodName);

                            foreach (var tstCls in testClass.FindAll(x => x.FileName.ToLower().Contains(sourceCls.FileNameWithOutExtension.ToLower())))
                            //foreach (var tstCls in testClass)
                            {
                                sourceMethodSet.TestMethods.Add(new TestMethod() { TestClassFilePath = tstCls.FilePath, TestClass_Raw_FileContent_V1 = tstCls.Parent_Content, TestClass_Raw_FileContent_V2 = tstCls.Content,CommitedDateTime=TheCommit.Commited_DateTime.ToString() });
                            }
                        }                        
                    }

                }
            }
            MasterObject.CurrentProject.AnalysedCommits.Add(TheCommit.CommitId);

            var v =MasterObject.CurrentProject.theSourceClasses;
        }
        public List<MethodSet> FindChangedMethods(string Code1, string Code2)
        {
            //Needs fix
            var methods1 = ExtractMethods(Code1);
            var methods2 = ExtractMethods(Code2);
            List<MethodSet> theMethodSetList = new List<MethodSet>();
            foreach (var kvp in methods1.Methods)
            {
                if (methods2.Methods.TryGetValue(kvp.Key, out var methodBody))
                {
                    if (methodBody != kvp.Value)
                    {
                        MethodSet theMethodSet = new MethodSet();
                        theMethodSet.SourceMethodName = ExtractMethodSignature(kvp.Value); ;
                        theMethodSet.SourceMethodV1Snip = kvp.Value;
                        theMethodSet.SourceMethodV2Snip = methodBody;
                        theMethodSetList.Add(theMethodSet);
                    }
                    methods2.Methods.Remove(kvp.Key);
                }
                else
                {
                    MethodSet theMethodSet = new MethodSet();
                    theMethodSet.SourceMethodName = ExtractMethodSignature(kvp.Value); ;
                    theMethodSet.SourceMethodV1Snip = kvp.Value;
                    theMethodSet.SourceMethodV2Snip = "Newly Added Method";
                    theMethodSetList.Add(theMethodSet);
                }
            }
            return theMethodSetList;
        }
        public List<MethodSet> FindChangedMethods_Evaluation(string Code1, string Code2)
        {
            //Needs fix
            var methods1 = ExtractMethods(Code1);
            var methods2 = ExtractMethods(Code2);
            List<MethodSet> theMethodSetList = new List<MethodSet>();
            foreach (var kvp in methods1.Methods)
            {
                if (methods2.Methods.TryGetValue(kvp.Key, out var methodBody))
                {
                    if (methodBody != kvp.Value)
                    {
                        MethodSet theMethodSet = new MethodSet();
                        theMethodSet.SourceMethodName = ExtractMethodSignature(kvp.Value); ;
                        theMethodSet.SourceMethodV1Snip = kvp.Value;
                        theMethodSet.SourceMethodV2Snip = methodBody;
                        theMethodSetList.Add(theMethodSet);
                    }
                    methods2.Methods.Remove(kvp.Key);
                }                
            }
            return theMethodSetList;
        }
        public List<MethodSet> ExtrackMethods_Eval(string Code1)
        {
            //Needs fix
            var methods1 = ExtractMethods(Code1);
            //var methods2 = ExtractMethods(Code2);
            List<MethodSet> theMethodSetList = new List<MethodSet>();
            foreach (var kvp in methods1.Methods)
            {
                //if (methods2.Methods.TryGetValue(kvp.Key, out var methodBody))
                {
                    //if (methodBody != kvp.Value)
                    {
                        MethodSet theMethodSet = new MethodSet();
                        theMethodSet.SourceMethodName = ExtractMethodSignature(kvp.Value); ;
                        theMethodSet.SourceMethodV1Snip = kvp.Value;
                        //theMethodSet.SourceMethodV2Snip = methodBody;
                        theMethodSetList.Add(theMethodSet);
                    }
                    //methods2.Methods.Remove(kvp.Key);
                }
            }
            return theMethodSetList;
        }
    }
}
