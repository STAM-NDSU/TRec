using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;

namespace ConsoleApp
{
   

    class RunJavaApp
    {
        public string Run(string RepoPath, string CommitSha,List<string> ChangedFiles )
        {
            // Path to the Java executable
            //string javaExecutablePath = @"C:\Users\saiki\.jdks\openjdk-22.0.1\bin\java.exe "; // Adjust as necessary
            string javaExecutablePath = "java ";
            // Java application JAR path
            string javaAppPath = AppDomain.CurrentDomain.BaseDirectory+"Java\\Parsing.jar";

            // Arguments to pass to the Java application
           
            List<string> arguments = new List<string>();
            arguments.Add(RepoPath);
            arguments.Add(CommitSha);
            arguments.AddRange(ChangedFiles);


            string javaAppArguments = String.Join(" ", arguments.Select(arg => $"\"{arg}\""));
            // Create a new process
            Process javaProcess = new Process();

            javaProcess.StartInfo.FileName = javaExecutablePath;
            javaProcess.StartInfo.Arguments = $"-jar \"{javaAppPath}\" {javaAppArguments}";
            javaProcess.StartInfo.UseShellExecute = false;
            javaProcess.StartInfo.RedirectStandardOutput = true;
            javaProcess.StartInfo.RedirectStandardError = true;

            try
            {
                // Start the Java process
                javaProcess.Start();

                // Read the output (optional)
                StreamReader outputReader = javaProcess.StandardOutput;
                StreamReader errorReader = javaProcess.StandardError;
                string output = outputReader.ReadToEnd();
                string errors = errorReader.ReadToEnd();

                // Wait for the process to end
                javaProcess.WaitForExit();

                // Output results
               // Console.WriteLine("Java application output:");
                //Console.WriteLine(output);

                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine("Java application errors:");
                    Console.WriteLine(errors);
                }
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                throw;
            }
           
            //Console.ReadLine();
        }
    }

}
