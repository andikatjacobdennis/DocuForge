namespace ProjectDocumentationTool.Interfaces
{
    public interface IDiagramGenerator
    {
        Task<string> GenerateClassDiagramAsync(string exePath, string projectFolderPath, int timeoutMilliseconds);
    }
}

