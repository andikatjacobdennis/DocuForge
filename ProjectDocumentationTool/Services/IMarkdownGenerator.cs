using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Services
{
    // Interface for a Markdown documentation generator
    public interface IMarkdownGenerator
    {
        /// <summary>
        /// Generates markdown documentation for a SolutionInfoModel.
        /// </summary>
        /// <param name="solutionInfo">The SolutionInfoModel containing project and service fabric information.</param>
        /// <returns>Returns a markdown string with detailed documentation.</returns>
        string GenerateSolutionMarkdown(SolutionInfoModel solutionInfo);

        /// <summary>
        /// Generates markdown documentation for a ProjectInfoModel.
        /// </summary>
        /// <param name="projectInfo">The ProjectInfoModel containing project details and dependencies.</param>
        /// <returns>Returns a markdown string detailing the project structure and dependencies.</returns>
        string GenerateProjectMarkdown(ProjectInfoModel projectInfo);

        /// <summary>
        /// Generates markdown documentation for a ServiceFabricProjectInfoModel.
        /// </summary>
        /// <param name="serviceFabricProject">The ServiceFabricProjectInfoModel containing service fabric project details.</param>
        /// <returns>Returns a markdown string with the service fabric project details.</returns>
        string GenerateServiceFabricMarkdown(ServiceFabricProjectInfoModel serviceFabricProject);

        /// <summary>
        /// Combines multiple markdown strings into a single formatted markdown document.
        /// </summary>
        /// <param name="markdownSections">A list of markdown strings to combine.</param>
        /// <returns>A single combined markdown string.</returns>
        string CombineMarkdownSections(IEnumerable<string> markdownSections);
    }
}
