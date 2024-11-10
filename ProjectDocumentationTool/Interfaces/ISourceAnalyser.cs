using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Interfaces
{
    public interface ISourceAnalyser
    {
        ProjectInfo AnalyzeSolution(string solutionPath);
    }
}

