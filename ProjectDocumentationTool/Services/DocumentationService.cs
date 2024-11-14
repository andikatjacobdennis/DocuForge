using ProjectDocumentationTool.Models;
using ProjectDocumentationTool.Services;

public class DocumentationService
{
    private readonly IMarkdownGenerator _markdownGenerator;
    private readonly IMarkdownWriter _markdownWriter;

    public DocumentationService(IMarkdownGenerator markdownGenerator, IMarkdownWriter markdownWriter)
    {
        _markdownGenerator = markdownGenerator;
        _markdownWriter = markdownWriter;
    }

    public void GenerateAndSaveDocumentation(SolutionInfoModel solutionInfo, string outputFilePath)
    {
        // Generate the markdown content
        string markdownContent = _markdownGenerator.GenerateSolutionMarkdown(solutionInfo, outputFilePath);

        // Write the markdown content to the specified file
        _markdownWriter.WriteMarkdown(markdownContent, outputFilePath);
    }
}