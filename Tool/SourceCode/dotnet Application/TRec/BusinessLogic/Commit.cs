using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    [DebuggerDisplay("Name = {CommitId}")]
    public class Commit
    {
        public string CommitId { get; set; }
        public string ParentCommitID { get; set; }
        public List<File_Contents> ChangedFiles { get; set; } = new List<File_Contents>();
        public DateTimeOffset Commited_DateTime { get; set; }
        public DateTimeOffset Commited_DateTime_Raw { get; set; }
        public Commit()
        {

        }
        public Commit(string CommitId, string FilePath,string ParentSha)
        {
            this.CommitId = CommitId;
            this.ParentCommitID = ParentCommitID;
            File_Contents file_Contents = new File_Contents();
            file_Contents.FilePath = FilePath;
            ChangedFiles.Add(file_Contents);

        }
        public void Add(string FilePath, string Content, string Parent_Content)
        {
            File_Contents file_Contents = new File_Contents();
            file_Contents.Content = Content;
            file_Contents.Parent_Content = Parent_Content;
            file_Contents.FilePath = FilePath;

            if (Content != null)
                if (Parent_Content != null)
                    if (!ChangedFiles.Any(x => x.FilePath == FilePath))
                        ChangedFiles.Add(file_Contents);
        }

    }
    [DebuggerDisplay("Name = {FileName}")]
    public class File_Contents
    {
        public string FilePath { get; set; }
        public string FileName
        {
            get { return Path.GetFileName(FilePath); }
        }
        public string FileNameWithOutExtension
        {
            get { return Path.GetFileNameWithoutExtension(FilePath); }
        }
        public string Content { get; set; }
        public string Parent_Content { get; set; }
        public DateTimeOffset DateTimeCommited { get; set; }
    }
}
