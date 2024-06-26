using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using TestCase_Management;

namespace TestCase_Management
{
    public class JavaMethodExtractor2
    {
        private readonly string _repoPath;
        private readonly string _commitSha;

        public JavaMethodExtractor2(string repoPath, string commitSha)
        {
            _repoPath = repoPath;
            _commitSha = commitSha;
        }

        public void ExtractChangedMethods()
        {

            using (var repo = new Repository(_repoPath))
            {
                var commit = repo.Commits.FirstOrDefault(c => c.Sha == _commitSha);
                var parentCommit = commit?.Parents.FirstOrDefault();
                if (parentCommit != null)
                {
                    var patches = repo.Diff.Compare<Patch>(parentCommit.Tree, commit.Tree);

                    foreach (var p in patches)
                    {
                        Console.WriteLine($"Changed file: {p.Path}");
                        var changedLines = GetChangedLines(p.Hunks);

                        // Here we should parse the file and get the full method body
                        // For demonstration, we'll just print the changed lines
                        foreach (var line in changedLines)
                        {
                            Console.WriteLine($"Changed line: {line}");
                        }

                        // Parse the Java file and find the methods that contain the changed lines
                        var methodBodies = ParseJavaFileAndGetMethods(p.Path, changedLines);
                        foreach (var methodBody in methodBodies)
                        {
                            Console.WriteLine(methodBody);
                        }
                    }
                }
            }
        }

        private List<int> GetChangedLines(IEnumerable<Hunk> hunks)
        {
            // Logic to parse the hunks and extract line numbers
            var changedLines = new List<int>();
            foreach (var hunk in hunks)
            {
                // Pseudo-code for extracting changed lines
                changedLines.AddRange(hunk.Lines.Where(l => l.IsAddition()).Select(l => l.NewLineNumber));
            }
            return changedLines;
        }

        private IEnumerable<string> ParseJavaFileAndGetMethods(string filePath, List<int> changedLines)
        {
            // Use ANTLR to parse the Java file
            // Pseudo-code as actual parsing will depend on the ANTLR-generated classes
            var fileContent = System.IO.File.ReadAllText(filePath);
            //AntlrInputStream inputStream = new AntlrInputStream(fileContent);
            //Java8Parser lexer = new Java8Parser(inputStream); // Replace with actual lexer class
            //CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            //Java8Parser parser = new Java8Parser(tokenStream); // Replace with actual parser class
            var inputStream = new AntlrInputStream(fileContent);
            var lexer = new Java8Lexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new Java8Parser(tokenStream);

            IParseTree tree = parser.compilationUnit(); // Start rule for Java grammar
            ParseTreeWalker walker = new ParseTreeWalker();
            JavaParseTreeListener listener = new JavaParseTreeListener(changedLines);
            walker.Walk(listener, tree);

            return listener.GetMethodBodies();
        }
    }

    public class JavaParseTreeListener : Java8ParserBaseListener // Replace with actual base listener class
    {
        private List<int> _changedLines;
        private List<string> _methodBodies = new List<string>();

        public JavaParseTreeListener(List<int> changedLines)
        {
            _changedLines = changedLines;
        }

        public override void EnterMethodDeclaration(Java8Parser.MethodDeclarationContext context)
        {
            var methodStartLine = context.Start.Line;
            var methodEndLine = context.Stop.Line;

            if (_changedLines.Any(line => line >= methodStartLine && line <= methodEndLine))
            {
                _methodBodies.Add(context.GetText());
            }
        }

        public List<string> GetMethodBodies()
        {
            return _methodBodies;
        }
    }
}
