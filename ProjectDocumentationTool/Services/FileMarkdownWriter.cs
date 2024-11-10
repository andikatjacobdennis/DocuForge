namespace ProjectDocumentationTool.Services
{
    // Implementation of IMarkdownWriter to write markdown to a file
    public class FileMarkdownWriter : IMarkdownWriter
    {
        public void WriteMarkdown(string content, string destination)
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(destination);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write content to file
            File.WriteAllText(destination, content);
        }
    }
}
