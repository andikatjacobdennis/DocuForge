using Microsoft.Extensions.Logging;

namespace ProjectDocumentationTool.Utilities
{
    public class CSharpFileFinder
    {
        private readonly ILogger<CSharpFileFinder> _logger;

        public CSharpFileFinder(ILogger<CSharpFileFinder> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Finds all .cs files in the specified directory and its subdirectories using DFS.
        /// </summary>
        /// <param name="rootPath">Root directory to search.</param>
        /// <returns>List of file paths to .cs files.</returns>
        public List<string> FindCsFiles(string rootPath)
        {
            var csFiles = new List<string>();

            if (!Directory.Exists(rootPath))
            {
                _logger.LogError("The directory '{RootPath}' does not exist.", rootPath);
                throw new DirectoryNotFoundException($"The directory '{rootPath}' does not exist.");
            }

            _logger.LogInformation("Starting search for .cs files in directory: {RootPath}", rootPath);

            FindCsFilesDFS(rootPath, csFiles);

            _logger.LogInformation("Search completed. Found {FileCount} .cs files.", csFiles.Count);
            return csFiles;
        }

        /// <summary>
        /// Recursive helper method to perform DFS and find .cs files.
        /// </summary>
        /// <param name="currentPath">Current directory being explored.</param>
        /// <param name="csFiles">List to store found .cs files.</param>
        private void FindCsFilesDFS(string currentPath, List<string> csFiles)
        {
            _logger.LogDebug("Exploring directory: {CurrentPath}", currentPath);

            try
            {
                // Add .cs files in the current directory
                foreach (var file in Directory.GetFiles(currentPath, "*.cs"))
                {
                    _logger.LogTrace("Found .cs file: {FilePath}", file);
                    csFiles.Add(file);
                }

                // Recurse into subdirectories
                foreach (var directory in Directory.GetDirectories(currentPath))
                {
                    _logger.LogDebug("Descending into subdirectory: {Directory}", directory);
                    FindCsFilesDFS(directory, csFiles);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Access denied to directory: {CurrentPath}. Error: {Message}", currentPath, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while exploring directory: {CurrentPath}. Error: {Message}", currentPath, ex.Message);
            }
        }
    }
}
