using ProjectDocumentationTool.Interfaces;
using ProjectDocumentationTool.Models;
using Microsoft.Extensions.Logging;
using ProjectDocumentationTool.Utilities;

namespace ProjectDocumentationTool.Services
{
    public class MenuService : IMenuService
    {
        private readonly IDiagramGenerator _diagramGenerator;
        private readonly ISourceAnalyser _sourceAnalyser;
        private readonly ILogger<MenuService> _logger;
        private readonly PathSanitizer _pathSanitizer;
        private readonly DocumentationService _documentationService;
        private readonly PlantUmlDiagramGenerator _plantUmlDiagramGenerator;
        private readonly SolutionProjectExtractor _solutionProjectExtractor;

        // Constructor with ILogger injected
        public MenuService(IDiagramGenerator diagramGenerator,
            ISourceAnalyser sourceAnalyser,
            ILogger<MenuService> logger,
            PathSanitizer pathSanitizer,
            DocumentationService documentationService,
            PlantUmlDiagramGenerator plantUmlDiagramGenerator,
            SolutionProjectExtractor solutionProjectExtractor)
        {
            _diagramGenerator = diagramGenerator;
            _sourceAnalyser = sourceAnalyser;
            _logger = logger;
            _pathSanitizer = pathSanitizer;
            _documentationService = documentationService;
            _plantUmlDiagramGenerator = plantUmlDiagramGenerator;
            _solutionProjectExtractor = solutionProjectExtractor;
        }

        public void DisplayMenu()
        {
            while (true) // Infinite loop to keep displaying the menu until the user exits
            {
                Console.Clear(); // Clears the screen to refresh the menu
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("              Project Documentation Tool            ");
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("1. Analyze Repos");
                Console.WriteLine("2. Analyze Solution");
                Console.WriteLine("0. Exit");
                Console.WriteLine("----------------------------------------------------");
                Console.Write("Please select an option: ");

                string? choice = Console.ReadLine()?.Trim();

                _logger.LogInformation("User selected option: {Choice}", choice); // Log user choice

                switch (choice)
                {
                    case "1":
                        AnalyzeRepos();
                        break;


                    case "2":
                        AnalyzeSolution();
                        break;

                    case "0":
                        Console.WriteLine("Exiting the program. Goodbye!");
                        _logger.LogInformation("User is exiting the program.");
                        return; // Exit the method and end the program

                    default:
                        Console.WriteLine("Invalid choice. Please enter a valid option.");
                        _logger.LogWarning("Invalid menu option selected: {Choice}", choice);
                        break;
                }

                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey(); // Wait for user to press a key before refreshing the menu
            }
        }

        private void AnalyzeRepos()
        {
            string reposRootFolder = GetValidDirectoryPath("Please enter the path to the repositories root folder:");
            string outputFolder = GetValidDirectoryPath("Please enter the path to the output folder:");

            // Remove any surrounding double quotes from the output folder path
            outputFolder = outputFolder.Trim('"');

            // Check if the output folder exists, if not, create it
            {
                try
                {
                    if (!Directory.Exists(outputFolder))
                    {
                        Directory.CreateDirectory(outputFolder);
                        Console.WriteLine($"Output folder '{outputFolder}' created successfully.");
                        _logger.LogInformation($"Output folder '{outputFolder}' created successfully.");
                    }

                    // Call the Extract method with user-provided values
                    try
                    {
                        var solutions = _solutionProjectExtractor.Extract(reposRootFolder, outputFolder);
                        Console.WriteLine("Solution and Project Extracted successfully.");
                        _logger.LogInformation("Solution and Project Extracted successfully.");

                        foreach (var solution in solutions) 
                        {
                            string solutionRepoId = $"{solution.SolutionRepoId:000}_{Path.GetFileNameWithoutExtension(solution.SolutionPath)}";
                            SolutionAnalyzer(solutionRepoId, solution.SolutionPath, outputFolder);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        _logger.LogError(ex, "An error occurred extracting solution from repository.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the output folder: {ex.Message}");
                    _logger.LogError(ex, "An error occurred creating the output folder.");
                }
            }
        }

        private void AnalyzeSolution()
        {
            // Ensure the solution path is correct
            Console.Write("Enter the full path of the solution file: ");
            string? solutionPath = Console.ReadLine()?.Trim();
            solutionPath = _pathSanitizer.SanitizeSolutionPath(solutionPath);
            if (string.IsNullOrEmpty(solutionPath))
            {
                Console.WriteLine("Error: Solution path cannot be empty. Please try again.");
                _logger.LogWarning("Solution path was empty or null.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                SolutionAnalyzer("Documentation", solutionPath, @".\");
            }
        }

        private void SolutionAnalyzer(string solutionRepoId, string? solutionPath, string outputFolder)
        {
            try
            {
                SolutionInfoModel solutionInfo = _sourceAnalyser.AnalyzeSolution(solutionPath);
                Console.WriteLine($"Solution '{solutionPath}' analyzed successfully.");
                _logger.LogInformation("Solution analyzed successfully: {SolutionPath}", solutionPath);

                // Generate and save documentation
                _documentationService.GenerateAndSaveDocumentation(solutionInfo, $"{outputFolder}\\{solutionRepoId}\\Documentation.md");

                // Generate and save dependency diagram
                _plantUmlDiagramGenerator.GenerateDependencyDiagram(solutionInfo, $"{outputFolder}\\{solutionRepoId}\\diagram\\Documentation.puml");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error analyzing the solution: {ex.Message}");
                _logger.LogError(ex, "Error analyzing the solution at path: {SolutionPath}", solutionPath);
            }
        }

        // Method to get a valid directory path from the user
        private string GetValidDirectoryPath(string prompt)
        {
            string path = string.Empty;
            bool isValid = false;

            while (!isValid)
            {
                Console.WriteLine(prompt);
                path = Console.ReadLine();

                // Validate if the directory exists and is indeed a directory
                if (Directory.Exists(path))
                {
                    isValid = true; // Path is valid
                }
                else
                {
                    Console.WriteLine("The specified path is not valid. Please enter a valid directory path.");
                }
            }

            return path;
        }
    }
}
