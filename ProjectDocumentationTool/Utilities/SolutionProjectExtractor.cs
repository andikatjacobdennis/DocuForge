using ProjectDocumentationTool.Models;
using System.Text.RegularExpressions;

namespace ProjectDocumentationTool.Utilities
{
    public class SolutionProjectExtractor
    {
        public List<SolutionInfoModel> Extract(string reposRootFolder, string outputFolder)
        {
            string excludeInput = string.Empty;
            HashSet<string> additionalExcludes = new HashSet<string>(excludeInput.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()));

            // Combine the default excluded folders with any additional exclusions from user input
            HashSet<string> excludeFolders = GetExcludedFolders();
            excludeFolders.UnionWith(additionalExcludes);

            string fileTypeInput = ".cs";
            List<string> allowedFileTypes = fileTypeInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                .Select(f => f.Trim().ToLower())
                                                .ToList();

            List<SolutionInfoModel> solutions = FindSolutionFiles(reposRootFolder, excludeFolders, allowedFileTypes);
            if (solutions.Count > 0)
            {
                for (int i = 0; i < solutions.Count; i++)
                {
                    SolutionInfoModel? solution = solutions[i];
                    string solutionName = $"{solution.SolutionRepoId:000}_{Path.GetFileNameWithoutExtension(solution.SolutionPath)}";
                }
            }
            else
            {
                Console.WriteLine("No solution files found in the directory.");
            }

            return solutions;
        }

        // List of default folders to exclude
        private HashSet<string> GetExcludedFolders()
        {
            return new HashSet<string> { "bin", "obj", ".vs", "node_modules" };
        }

        // Method to get project paths and details from a solution file
        private SolutionInfoModel GetSolutionInfo(string solutionFilePath, HashSet<string> excludeFolders, List<string> allowedFileTypes)
        {
            SolutionInfoModel solutionInfo = new SolutionInfoModel { SolutionPath = solutionFilePath };
            solutionInfo.ProjectInfos = new List<ProjectInfoModel>();
            
            if (!File.Exists(solutionFilePath))
            {
                Console.WriteLine("Solution file not found.");
                return solutionInfo;
            }

            // Regex pattern to match lines that define project paths
            Regex projectLinePattern = new Regex(@"Project\(\""\{[A-F0-9-]+\}\""\) = \""(.*?)\""\s*,\s*\""(.*?)\""",
                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string? solutionDirectory = Path.GetDirectoryName(solutionFilePath);

            foreach (string line in File.ReadLines(solutionFilePath))
            {
                Match match = projectLinePattern.Match(line);
                if (match.Success)
                {
                    // Capture project name and relative path
                    string projectName = match.Groups[1].Value;
                    string relativePath = match.Groups[2].Value;
                    string absolutePath = Path.GetFullPath(Path.Combine(solutionDirectory, relativePath));
                    string? projectFolder = Path.GetDirectoryName(absolutePath);
                    string projectType = Path.GetExtension(absolutePath).TrimStart('.').ToUpper(); // e.g., 'CSPROJ' or 'VBPROJ'

                    // Create a new ProjectInfo and populate the files list
                    ProjectInfoModel projectInfo = new ProjectInfoModel
                    {
                        ProjectPath = absolutePath,
                        ProjectName = projectName,
                        ProjectFolder = projectFolder,
                        ProjectType = projectType,
                        //Files = GetProjectFiles(projectFolder, excludeFolders, allowedFileTypes)
                    };

                    // Add project info to solution info
                    solutionInfo.ProjectInfos.Add(projectInfo);
                }
            }

            return solutionInfo;
        }

        // Method to find all solution files in a directory and its subdirectories
        private List<SolutionInfoModel> FindSolutionFiles(string directoryPath, HashSet<string> excludeFolders, List<string> allowedFileTypes)
        {
            List<SolutionInfoModel> solutions = new List<SolutionInfoModel>();

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory not found.");
                return solutions;
            }

            string[] firstLevelDirectories = Directory.GetDirectories(directoryPath);

            for (int i = 0; i < firstLevelDirectories.Length; i++)
            {
                string? firstLevelDirectory = firstLevelDirectories[i];
                try
                {
                    string[] solutionFiles = Directory.GetFiles(firstLevelDirectory, "*.sln", SearchOption.AllDirectories);
                    foreach (string solutionFile in solutionFiles)
                    {
                        SolutionInfoModel solutionInfo = GetSolutionInfo(solutionFile, excludeFolders, allowedFileTypes);
                        solutionInfo.SolutionRepoId = i + 1;
                        solutions.Add(solutionInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error searching for solution files: {ex.Message}");
                }
            }
            return solutions;
        }

        // Method to get detailed file information in a project folder, excluding specified folders and filtering by allowed file types
        private List<FileInfoModel> GetProjectFiles(string projectFolder, HashSet<string> excludeFolders, List<string> allowedFileTypes)
        {
            List<FileInfoModel> files = new List<FileInfoModel>();
            HashSet<string> addedFilePaths = new HashSet<string>(); // Track added file paths to avoid duplicates

            if (!Directory.Exists(projectFolder))
            {
                return files;
            }

            try
            {
                IEnumerable<string> directories = Directory.GetDirectories(projectFolder, "*", SearchOption.AllDirectories)
                                           .Where(d => !excludeFolders.Contains(new DirectoryInfo(d).Name));

                foreach (string? directory in directories)
                {
                    foreach (string filePath in Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        if (allowedFileTypes.Contains(fileInfo.Extension.ToLower()))
                        {
                            // Only add the file if it hasn't been added before
                            if (addedFilePaths.Add(fileInfo.FullName)) // Add returns false if already exists
                            {
                                files.Add(new FileInfoModel
                                {
                                    FileName = fileInfo.Name,
                                    FilePath = fileInfo.FullName,
                                    Size = fileInfo.Length,
                                    DateCreated = fileInfo.CreationTime,
                                    DateModified = fileInfo.LastWriteTime
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting files for project folder {projectFolder}: {ex.Message}");
            }

            return files;
        }
    }
}
