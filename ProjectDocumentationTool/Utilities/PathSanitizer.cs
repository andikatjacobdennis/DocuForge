using Microsoft.Extensions.Logging;

namespace ProjectDocumentationTool.Utilities
{
    public class PathSanitizer
    {
        private readonly ILogger<PathSanitizer> _logger;

        // Constructor receives an ILogger via dependency injection
        public PathSanitizer(ILogger<PathSanitizer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string SanitizeSolutionPath(string solutionPath)
        {
            try
            {
                // Null or empty path check
                if (string.IsNullOrWhiteSpace(solutionPath))
                {
                    _logger.LogWarning("Solution path is null or empty.");
                    return string.Empty;
                }

                // Remove any double quotes (if any)
                solutionPath = solutionPath.Replace("\"", string.Empty);

                // Check for invalid characters in the path
                if (solutionPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                {
                    _logger.LogWarning("Solution path contains invalid characters.");
                    return string.Empty;
                }

                // Check if the file exists
                if (!File.Exists(solutionPath))
                {
                    _logger.LogWarning("Solution file does not exist.");
                    return string.Empty;
                }

                // Normalize the path
                string normalizedPath = Path.GetFullPath(solutionPath);

                // Ensure the extension is ".sln"
                if (!normalizedPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Solution path does not end with '.sln'.");
                    return string.Empty;
                }

                // Return the sanitized path
                return normalizedPath;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access exception occurred while sanitizing path.");
                return string.Empty;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument exception occurred while sanitizing path.");
                return string.Empty;
            }
            catch (PathTooLongException ex)
            {
                _logger.LogError(ex, "Path is too long exception occurred.");
                return string.Empty;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "I/O exception occurred while sanitizing path.");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while sanitizing path.");
                return string.Empty;
            }
        }
    }
}
