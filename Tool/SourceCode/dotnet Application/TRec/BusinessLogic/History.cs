using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{

    public class History
    {
        public string  CommitID { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }

        public string Message { get; set; }
        public List<string> FilesChanged { get; set; } = new List<string>();

        public List<FileInfo> theFilesChanged = new List<FileInfo>();

        public void ValidateFiles()
        {
            foreach (string theFilePath in FilesChanged)
            {
                try
                {
                    FileInfo theFileInfo = new FileInfo(theFilePath);
                    theFilesChanged.Add(theFileInfo);
                }
                catch { }

            }            
        }
        public override string ToString()
        {
            return $"Commit Id : {CommitID} \nAuthor : {Author} \nDate : {Date} \nMessage : {Message} \nFilesChanged: \n {String.Join("\n", FilesChanged)}";
        }
    }
}
