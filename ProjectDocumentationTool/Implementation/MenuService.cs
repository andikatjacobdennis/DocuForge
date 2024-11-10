using ProjectDocumentationTool.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace ProjectDocumentationTool.Implementation
{
    public class MenuService : IMenuService
    {
        private readonly IDiagramGenerator _diagramGenerator;
        private readonly ISourceAnalyser _sourceAnalyser;
        private readonly ILogger<MenuService> _logger;

        // Constructor with ILogger injected
        public MenuService(IDiagramGenerator diagramGenerator, ISourceAnalyser sourceAnalyser, ILogger<MenuService> logger)
        {
            _diagramGenerator = diagramGenerator;
            _sourceAnalyser = sourceAnalyser;
            _logger = logger;
        }

        public void DisplayMenu()
        {
            while (true) // Infinite loop to keep displaying the menu until the user exits
            {
                Console.Clear(); // Clears the screen to refresh the menu
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("              Project Documentation Tool            ");
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("1. Analyze Solution");
                Console.WriteLine("2. Generate Diagram");
                Console.WriteLine("0. Exit");
                Console.WriteLine("----------------------------------------------------");
                Console.Write("Please select an option: ");

                var choice = Console.ReadLine()?.Trim();

                _logger.LogInformation("User selected option: {Choice}", choice); // Log user choice

                switch (choice)
                {
                    case "1":
                        // Ensure the solution path is correct
                        Console.Write("Enter the full path of the solution file: ");
                        var solutionPath = Console.ReadLine()?.Trim();

                        if (string.IsNullOrEmpty(solutionPath))
                        {
                            Console.WriteLine("Error: Solution path cannot be empty. Please try again.");
                            _logger.LogWarning("Solution path was empty or null.");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                            continue; // Retry the menu if path is invalid
                        }

                        try
                        {
                            var projectInfo = _sourceAnalyser.AnalyzeSolution(solutionPath);
                            Console.WriteLine($"Solution '{solutionPath}' analyzed successfully.");
                            _logger.LogInformation("Solution analyzed successfully: {SolutionPath}", solutionPath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error analyzing the solution: {ex.Message}");
                            _logger.LogError(ex, "Error analyzing the solution at path: {SolutionPath}", solutionPath);
                        }
                        break;

                    case "2":
                        try
                        {
                            _diagramGenerator.GenerateDiagram();
                            Console.WriteLine("Diagram generated successfully.");
                            _logger.LogInformation("Diagram generated successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error generating the diagram: {ex.Message}");
                            _logger.LogError(ex, "Error generating the diagram.");
                        }
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
    }
}
