using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using ProjectDocumentationTool.Interfaces;

namespace ProjectDocumentationTool.Utilities
{
    public class DiagramGenerator: IDiagramGenerator
    {
        private readonly ILogger<DiagramGenerator> _logger;

        // Constructor with dependency injection for ILogger
        public DiagramGenerator(ILogger<DiagramGenerator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateClassDiagramAsync(string exePath, string projectFolderPath, int timeoutMilliseconds)
        {
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                _logger.LogError("Executable path is invalid or file does not exist: {ExePath}", exePath);
                throw new ArgumentException("Executable path is invalid or file does not exist: {ExePath}", exePath);
            }

            if (string.IsNullOrWhiteSpace(projectFolderPath) || !Directory.Exists(projectFolderPath))
            {
                _logger.LogError("Project folder path is invalid or does not exist: {ProjectFolder}", projectFolderPath);
                throw new ArgumentException("Project folder path is invalid or does not exist: {ProjectFolder}", projectFolderPath);
            }

            if (string.IsNullOrEmpty(exePath) || string.IsNullOrEmpty(projectFolderPath))
            {
                throw new ArgumentException("Both exePath and projectFolderPath must be provided.");
            }

            if (!File.Exists(exePath))
            {
                _logger.LogError("Executable not found at path: {ExePath}", exePath);
                throw new FileNotFoundException("Executable not found.", exePath);
            }

            if (!Directory.Exists(projectFolderPath))
            {
                _logger.LogError("Project folder path does not exist: {ProjectFolderPath}", projectFolderPath);
                throw new DirectoryNotFoundException("Project folder path does not exist.");
            }

            using (var cts = new CancellationTokenSource(timeoutMilliseconds))
            {
                try
                {
                    return await Task.Run(() => RunProcess(exePath, projectFolderPath, cts.Token), cts.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogError("Process timeout reached for executable at path: {ExePath}", exePath);
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while generating the class diagram.");
                    return null;
                }
            }
        }

        private string RunProcess(string exePath, string projectFolderPath, CancellationToken token)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = exePath;
                process.StartInfo.Arguments = $"\"{projectFolderPath}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);
                process.ErrorDataReceived += (sender, args) => errorBuilder.AppendLine(args.Data);

                process.Start();

                // Start reading the output and error streams
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                try
                {
                    // Wait for process exit or cancellation
                    while (!process.WaitForExit(100))
                    {
                        if (token.IsCancellationRequested)
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch (InvalidOperationException ex)
                            {
                                // Handle the case where the process has already exited
                                _logger.LogWarning("Process already exited: {Message}", ex.Message);
                            }
                            catch (Exception ex)
                            {
                                // Handle any other exceptions during the Kill operation
                                _logger.LogError(ex, "Error occurred while trying to kill the process.");
                                throw; // Re-throw the exception if necessary
                            }

                            // Ensure the cancellation token acknowledges the cancellation
                            token.ThrowIfCancellationRequested();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Operation was canceled.");
                }
                catch (Exception ex)
                {
                    // Catch any other unexpected exceptions
                    _logger.LogError(ex, "An unexpected error occurred during process monitoring.");
                    //throw; // Optionally re-throw if you want to propagate the exception
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Process exited with code {ExitCode}. Error: {Error}", process.ExitCode, errorBuilder.ToString());
                    return null;
                }

                var debugFilePath = Path.Combine(projectFolderPath, "default.plantuml");
                if (!File.Exists(debugFilePath))
                {
                    _logger.LogError("default.plantuml file was not generated in the project folder: {ProjectFolderPath}", projectFolderPath);
                    return null;
                }

                // Read the generated debug.puml content and return as string
                return File.ReadAllText(debugFilePath);
            }
        }
    }
}
