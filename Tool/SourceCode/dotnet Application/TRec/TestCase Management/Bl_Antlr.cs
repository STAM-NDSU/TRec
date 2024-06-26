using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using BusinessLogic;

namespace TestCase_Management
{

    public class MethodCallExtractor : Java8ParserBaseListener
    {
        private readonly ICharStream _input;
        public HashSet<string> MethodSignatures { get; } = new HashSet<string>();

        public MethodCallExtractor(ICharStream input)
        {
            _input = input;
        }

        public override void EnterMethodInvocation(Java8Parser.MethodInvocationContext context)
        {
            var methodNameContext = context.methodName();
            if (methodNameContext != null)
            {
                int start = context.Start.StartIndex;
                int stop = methodNameContext.Stop.StopIndex;  // Using the corrected context here

                string methodSignature = _input.GetText(Interval.Of(start, stop));
                MethodSignatures.Add(methodSignature);
            }
        }
    }
    public class JavaMethodSignatureExtractor : Java8ParserBaseListener
    {
        private readonly ICharStream _input;
        public string MethodSignature { get; private set; }

        public JavaMethodSignatureExtractor(ICharStream input)
        {
            _input = input;
        }

        public override void EnterMethodDeclaration(Java8Parser.MethodDeclarationContext context)
        {
            int start = context.methodHeader().Start.StartIndex;
            int stop = context.methodHeader().Stop.StopIndex;

            MethodSignature = _input.GetText(Interval.Of(start, stop));
        }
    }
    public class JavaMethodExtractor : Java8ParserBaseListener
    {

        public Dictionary<string, string> Methods { get; } = new Dictionary<string, string>();

        private readonly ICharStream _input;

        public JavaMethodExtractor(ICharStream input)
        {
            _input = input;
        }

        public override void EnterMethodDeclaration(Java8Parser.MethodDeclarationContext context)
        {
            int start = context.Start.StartIndex;
            int stop = context.Stop.StopIndex;

            string methodName = context.methodHeader().methodDeclarator().Identifier().GetText();
            string paramsList = context.methodHeader().methodDeclarator().formalParameterList()?.GetText() ?? "";
            string methodSignature = $"{methodName}({paramsList})";

            string methodBody = _input.GetText(Interval.Of(start, stop));

            Methods[methodSignature] = methodBody;
        }
    }
    public class Bl_Antlr
    {
        public static HashSet<string> ExtractMethodCalls(string javaCode)
        {
            var inputStream = new AntlrInputStream(javaCode);
            var lexer = new Java8Lexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new Java8Parser(tokenStream);
            var tree = parser.compilationUnit();

            var extractor = new MethodCallExtractor(inputStream);
            var walker = new ParseTreeWalker();
            walker.Walk(extractor, tree);

            return extractor.MethodSignatures;
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

        public void CompareCodes(SourceClass sourceClass)
        {
            var methods1 = ExtractMethods(sourceClass.SourceClass_Raw_V1_FileContent);
            var methods2 = ExtractMethods(sourceClass.SourceClass_Raw_V2_FileContent);

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
                        sourceClass.MethodSets.Add(theMethodSet);
                    }
                    methods2.Methods.Remove(kvp.Key); // remove matched method for further processing
                }
                //else
                //{
                //    Console.WriteLine($"Method '{kvp.Key}' has been removed.");
                //}
            }

            // Any remaining methods in methods2 are new additions
            //foreach (var kvp in methods2.Methods)
            //{
            //    Console.WriteLine($"Method '{kvp.Key}' has been added.");
            //}
        }
        public string ExtraxtMethodNameOnly(string MethodSignature)
        {
            //string input = "int hashCode()";
            string pattern = @"\([^)]*\)";
            string result = Regex.Replace(MethodSignature, pattern, "");

            Regex regex = new Regex(@"(?<=\s)\w+$");
            Match match = regex.Match(result);
            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return null;
            }
        }
        public void ExtractMethodCalls(SourceClass sourceClass)
        {

            // Assuming you want to search in a specific directory and its subdirectories.
            string directoryPath = MasterObject.version1Directory;

            var javaFiles = Directory.EnumerateFiles(directoryPath, "*.java", SearchOption.AllDirectories);

            //string targetMethodSignature = "yourExtractedSignatureHere";
            List<string> referencingFiles = new List<string>();

            int cou = 0;
            foreach (var file in javaFiles)
            {
                cou++;
                string content = File.ReadAllText(file);

                //var methodSignatures = ExtractMethodCalls(content);

                foreach (var theMethodSet in sourceClass.MethodSets)
                {
                    string sourceMethodNameOnly = ExtraxtMethodNameOnly(theMethodSet.SourceMethodName);
                    if (sourceMethodNameOnly != null && content.Contains(sourceMethodNameOnly))
                    {
                        var methods = ExtractMethods(sourceClass.SourceClass_Raw_V1_FileContent);
                        foreach (var kvp in methods.Methods)
                        {

                            if (kvp.Value.Contains(sourceMethodNameOnly))
                            {
                                var obj = new TestMethod();
                                obj.TestMethodSnip = kvp.Value;
                                obj.TestMethodName= ExtractMethodSignature(kvp.Value);
                                //obj.TestClasssFileName = file;
                                //Priority Missing. 
                                theMethodSet.TestMethods.Add(obj);

                                //var refernceMethod = ExtractMethodSignature(kvp.Value); ;
                                //referencingFiles.Add(theMethodSet.SourceMethodName + " - " + refernceMethod + " - " + file);
                            }

                        }
                    }

                }

            }

        }
    }
}
