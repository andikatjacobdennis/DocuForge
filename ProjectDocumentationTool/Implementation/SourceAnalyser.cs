using ProjectDocumentationTool.Interfaces;
using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Implementation
{
    public class SourceAnalyser : ISourceAnalyser
    {
        public ProjectInfo AnalyzeSolution(string solutionPath)
        {
            // Example logic: Use Microsoft.CodeAnalysis.MSBuild to analyze the solution
            Console.WriteLine($"Analyzing solution at {solutionPath}...");
            var projectInfo = new ProjectInfo();
            // Fill ProjectInfo with data
            return projectInfo;
        }
    }
}
