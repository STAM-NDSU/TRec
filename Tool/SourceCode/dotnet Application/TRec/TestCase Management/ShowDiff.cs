using DiffPlex.DiffBuilder.Model;
using DiffPlex;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using DiffPlex.DiffBuilder;
using DiffMatchPatch;
using System.Reflection;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCase_Management
{
    public partial class ShowDiff : Form
    {
        string fileContent1;
        string fileContent2;

        public ShowDiff(string file1, string file2)
        {
            InitializeComponent();
            this.fileContent1 = file1;
            this.fileContent2 = file2;
        }


        public void CompareAndHighlightFiles()
        {
            string content1 = fileContent1;
            string content2 = fileContent2;

            // Set the font and text for the controls
            richTextBox1.Text = content1;
            richTextBox2.Text = content2;

            // Compare the content using DiffMatchPatch
            diff_match_patch differ = new diff_match_patch();
            List<Diff> diffs = differ.diff_main(content1, content2);
            differ.diff_cleanupSemantic(diffs);

            int index1 = 0;
            int index2 = 0;
            foreach (Diff diff in diffs)
            {
                //FileDifferences theFileDiff = new FileDifferences();
                int length = diff.text.Length;
                switch (diff.operation)
                {
                    case Operation.DELETE:
                        richTextBox1.Select(index1, length);
                        richTextBox1.SelectionBackColor = System.Drawing.Color.Yellow;
                        index1 += length;
                        //theFileDiff.FileName = "File1";
                       // theFileDiff.FilePath = filePath1;
                        //theFileDiff.LineIndex = index1;
                        break;
                    case Operation.INSERT:
                        richTextBox2.Select(index2, length);
                        richTextBox2.SelectionBackColor = System.Drawing.Color.Yellow;
                        index2 += length;
                        //theFileDiff.FileName = "File2";
                       // theFileDiff.FilePath = filePath2;
                        //theFileDiff.LineIndex = index2;
                        break;
                    case Operation.EQUAL:
                        index1 += length;
                        index2 += length;
                        break;
                }
                //theFileDiffList.Add(theFileDiff);

            }

        }
        //public void Main()
        //{
        //    string filePath = "path_to_your_cs_file.cs";
        //    int desiredIndex = 150; // The text index you want to find the line number for

        //    int lineNumber = FindLineNumber(filePath, desiredIndex);

        //}

        public int FindLineNumber(string filePath, int desiredIndex)
        {
            int lineNumber = 0;
            int currentIndex = 0;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    currentIndex += line.Length + Environment.NewLine.Length;

                    if (currentIndex >= desiredIndex)
                    {
                        break;
                    }
                }
            }

            return lineNumber;
        }

        private void ShowDiff_Load(object sender, EventArgs e)
        {
            CompareAndHighlightFiles();
        }

        private void button3_Click(object sender, EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }
    }
}