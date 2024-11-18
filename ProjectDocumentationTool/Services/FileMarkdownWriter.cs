using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Services
{
    // Implementation of IMarkdownWriter to write markdown to a file
    public class FileMarkdownWriter : IMarkdownWriter
    {
        private readonly ILogger<FileMarkdownWriter> _logger;

        public FileMarkdownWriter(ILogger<FileMarkdownWriter> logger)
        {
            _logger = logger;
        }

        public void WriteMarkdown(string content, string destination)
        {
            // Ensure the directory exists
            string? directory = Path.GetDirectoryName(destination);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write content to file
            File.WriteAllText(destination, content);
        }

        public string GenerateClassMarkdown(List<ClassInfo> classInfoList)
        {
            if (classInfoList == null || classInfoList.Count == 0)
            {
                _logger.LogError("Invalid or empty classInfoList input.");
            }

            var markdown = new System.Text.StringBuilder();

            foreach (var classInfo in classInfoList)
            {
                // Write Class Header
                markdown.AppendLine($"# Class: {classInfo.Name}");
                if (!string.IsNullOrWhiteSpace(classInfo.Namespace))
                    markdown.AppendLine($"**Namespace:** {classInfo.Namespace}");
                if (!string.IsNullOrWhiteSpace(classInfo.BaseClass))
                    markdown.AppendLine($"**Base Class:** {classInfo.BaseClass}");
                if (classInfo.Interfaces?.Count > 0)
                    markdown.AppendLine($"**Interfaces:** {string.Join(", ", classInfo.Interfaces)}");
                if (classInfo.GenericParameters?.Count > 0)
                    markdown.AppendLine($"**Generics:** {string.Join(", ", classInfo.GenericParameters)}");
                if (!string.IsNullOrWhiteSpace(classInfo.XmlComment))
                    markdown.AppendLine($"\n**Comment:** {classInfo.XmlComment}\n");

                // Properties Section
                if (classInfo.Properties.Count > 0)
                {
                    markdown.AppendLine("## Properties");
                    markdown.AppendLine("| Name | Type | Accessibility | Comment |");
                    markdown.AppendLine("|------|------|---------------|---------|");
                    foreach (var prop in classInfo.Properties)
                    {
                        markdown.AppendLine($"| {prop.Name} | {prop.Type} | {prop.Accessibility} | {prop.XmlComment ?? ""} |");
                    }
                }

                // Methods Section
                if (classInfo.Methods.Count > 0)
                {
                    markdown.AppendLine("\n## Methods");
                    markdown.AppendLine("| Name | Parameters | Return Type | Accessibility | Comment |");
                    markdown.AppendLine("|------|------------|-------------|---------------|---------|");
                    foreach (var method in classInfo.Methods)
                    {
                        var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
                        markdown.AppendLine($"| {method.Name} | {parameters} | {method.ReturnType} | {method.Accessibility} | {method.XmlComment ?? ""} |");
                    }
                }

                // Delegates Section
                if (classInfo.Delegates.Count > 0)
                {
                    markdown.AppendLine("\n## Delegates");
                    markdown.AppendLine("| Name | Parameters | Return Type | Accessibility | Comment |");
                    markdown.AppendLine("|------|------------|-------------|---------------|---------|");
                    foreach (var del in classInfo.Delegates)
                    {
                        var parameters = string.Join(", ", del.Parameters.Select(p => $"{p.Type} {p.Name}"));
                        markdown.AppendLine($"| {del.Name} | {parameters} | {del.ReturnType} | {del.Accessibility} | {del.XmlComment ?? ""} |");
                    }
                }

                // Events Section
                if (classInfo.Events.Count > 0)
                {
                    markdown.AppendLine("\n## Events");
                    markdown.AppendLine("| Name | Type | Accessibility | Comment |");
                    markdown.AppendLine("|------|------|---------------|---------|");
                    foreach (var evt in classInfo.Events)
                    {
                        markdown.AppendLine($"| {evt.Name} | {evt.Type} | {evt.Accessibility} | {evt.XmlComment ?? ""} |");
                    }
                }

                markdown.AppendLine("\n---\n"); // Add a separator between classes
            }

            return markdown.ToString();
        }
    }
}