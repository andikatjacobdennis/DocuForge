using System.Text;
using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Services
{
    public class MarkdownGenerator : IMarkdownGenerator
    {
        public string GenerateSolutionMarkdown(SolutionInfoModel solutionInfo)
        {
            var markdown = new StringBuilder();
            markdown.AppendLine($"## Solution: {solutionInfo.Name}");
            markdown.AppendLine("\n### Projects:");
            foreach (var project in solutionInfo.ProjectInfos)
            {
                markdown.AppendLine(GenerateProjectMarkdown(project));
            }

            markdown.AppendLine("\n### Service Fabric Projects:");
            foreach (var sfProject in solutionInfo.ServiceFabricProjects)
            {
                markdown.AppendLine(GenerateServiceFabricMarkdown(sfProject));
            }

            return markdown.ToString();
        }

        public string GenerateProjectMarkdown(ProjectInfoModel projectInfo)
        {
            var markdown = new StringBuilder();
            markdown.AppendLine($"### Project: {projectInfo.ProjectName}");

            // Add project dependencies
            markdown.AppendLine("\n#### Dependencies:");
            foreach (var dependency in projectInfo.ProjectDependencies)
            {
                markdown.AppendLine($"- {dependency.Key}: {string.Join(", ", dependency.Value)}");
            }

            // Add package references
            markdown.AppendLine("\n#### Package References:");
            foreach (var package in projectInfo.PackageReferences)
            {
                markdown.AppendLine($"- {package.Key}: {string.Join(", ", package.Value)}");
            }

            return markdown.ToString();
        }

        public string GenerateServiceFabricMarkdown(ServiceFabricProjectInfoModel serviceFabricProject)
        {
            var markdown = new StringBuilder();
            markdown.AppendLine($"### Service Fabric Project: {serviceFabricProject.ProjectFilePath}");
            markdown.AppendLine($"\n- **Project Version**: {serviceFabricProject.ProjectVersion}");
            markdown.AppendLine($"- **Target Framework**: {serviceFabricProject.TargetFrameworkVersion}");

            // List services
            markdown.AppendLine("\n#### Services:");
            foreach (var service in serviceFabricProject.Services)
            {
                markdown.AppendLine($"- {service}");
            }

            // Add application manifest
            markdown.AppendLine($"\n#### Application Manifest: {serviceFabricProject.ApplicationManifest}");

            // List application parameters
            markdown.AppendLine("\n#### Application Parameters:");
            foreach (var param in serviceFabricProject.ApplicationParameters)
            {
                markdown.AppendLine($"- {param}");
            }

            // List publish profiles
            markdown.AppendLine("\n#### Publish Profiles:");
            foreach (var profile in serviceFabricProject.PublishProfiles)
            {
                markdown.AppendLine($"- {profile}");
            }

            return markdown.ToString();
        }

        public string CombineMarkdownSections(IEnumerable<string> markdownSections)
        {
            return string.Join("\n\n---\n\n", markdownSections);
        }
    }
}
