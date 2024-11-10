namespace ProjectDocumentationTool.Services
{
    // Interface for writing markdown content to various destinations
    public interface IMarkdownWriter
    {
        /// <summary>
        /// Writes the markdown content to a specified destination.
        /// </summary>
        /// <param name="content">The markdown content to write.</param>
        /// <param name="destination">The output path or identifier for the destination.</param>
        void WriteMarkdown(string content, string destination);
    }
}
