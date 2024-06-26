using BusinessLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public class CRUDManager
    {
        public static string LoadRepoWareHouse()
        {
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase", "RepoWareHouse");
            string filePath = Path.Combine(directoryPath, "RepoData.json");

            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Ensure the file exists
                if (!File.Exists(filePath))
                {
                    using (File.Create(filePath))
                    {
                        // File created. You can optionally write initial content here.
                    }
                }
                else
                {
                    // Load existing data
                    RepoWareHouse.RepoMetadata = JsonConvert.DeserializeObject<List<RepoMetaData>>(File.ReadAllText(filePath));
                }

                // Initialize RepoMetadata if it is null
                if (RepoWareHouse.RepoMetadata == null)
                {
                    RepoWareHouse.RepoMetadata = new List<RepoMetaData>();
                }

                return "";
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or display an error message)
                return "Error: " + ex.Message;
                throw;
            }
        }


        public static string Save_Updated_RepoWareHouse()
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\DataBase\\RepoWareHouse\\RepoData.json";

            try
            {
                if (!File.Exists(filePath))
                    using (File.Create(filePath))
                    {
                        //File Created. 
                    }
                else
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(RepoWareHouse.RepoMetadata));
                return "";
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or display an error message)
                return "Error: " + ex.Message;
            }
        }
        public static string Save_Updated_ParsedData(List<CandidateCommit> theCommits, string ProjectName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dirPath = Path.Combine(baseDir, "DataBase", "ParsedData");
            string filePath = Path.Combine(dirPath, $"{ProjectName}.json");

            try
            {
                // Ensure the directory exists
                Directory.CreateDirectory(dirPath);

                // Check if the file exists before creating a new one
                if (!File.Exists(filePath))
                {
                    // Using statement ensures that the StreamWriter is closed correctly
                    using (StreamWriter file = File.CreateText(filePath)) // Automatically creates a new file or overwrites an existing file.
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        // Serialize directly to file stream, reducing memory footprint
                        serializer.Serialize(file, theCommits);
                    }
                    return "File created and saved successfully.";
                }
                else
                {
                    using (StreamWriter file = File.CreateText(filePath)) // Automatically creates a new file or overwrites an existing file.
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        // Serialize directly to file stream, reducing memory footprint
                        serializer.Serialize(file, theCommits);
                    }
                    return "File already exists. No new file created.";
                }
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or display an error message)
                return "Error: " + ex.Message;
            }
        }

        public static string LoadParsedData(ref List<CandidateCommit> theCommits, string ProjectName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase", "ParsedData", $"{ProjectName}.json");

            try
            {
                // Use a StreamReader to read the file stream directly
                using (StreamReader file = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    // Deserialize directly from the stream, reducing memory footprint
                    theCommits = (List<CandidateCommit>)serializer.Deserialize(file, typeof(List<CandidateCommit>));
                }
                return "";
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or display an error message)
                return $"Error: {ex.Message}";
            }


        }

        public static string LoadProject(string ProjectName)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\DataBase\\ProjectWareHouse\\" + ProjectName + ".json";

            try
            {

                MasterObject.CurrentProject = JsonConvert.DeserializeObject<Project>(File.ReadAllText(filePath));
                return "";
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or display an error message)
                return "Error: " + ex.Message;
            }

        }

        public static string Save_Updated_Project(string ProjectName)
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\DataBase\\ProjectWareHouse\\" + ProjectName + ".json";

            try
            {
                if (!File.Exists(filePath))
                    using (File.Create(filePath))
                    {
                        //File Created. 
                    }
                else
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(MasterObject.CurrentProject));
                return "";
            }
            catch (Exception ex)
            {
                // Handle the exception appropriately (e.g., log or display an error message)
                return "Error: " + ex.Message;
            }
        }
    }
}
