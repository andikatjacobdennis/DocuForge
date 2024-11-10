using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Interfaces
{
    public interface ISourceAnalyser
    {
        SolutionInfoModel AnalyzeSolution(string solutionPath);
    }
}

