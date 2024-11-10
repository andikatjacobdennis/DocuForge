using System.Text;
using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Services
{
    public class MarkdownGenerator : IMarkdownGenerator
    {
        public string GenerateSolutionMarkdown(SolutionInfoModel solutionInfo)
        {
            var markdown = new StringBuilder();
            markdown.AppendLine($"## Solution: {solutionInfo.Name}\n");

            // Add solution configurations (Build configurations)
            markdown.AppendLine("### Solution Configurations:");
            if (solutionInfo.SolutionConfigurationPlatforms.Any())
            {
                foreach (var config in solutionInfo.SolutionConfigurationPlatforms)
                {
                    markdown.AppendLine($"- {config}");
                }
            }
            else
            {
                markdown.AppendLine("No solution configurations found.");
            }

            // Add project details
            markdown.AppendLine("\n### Projects:");
            foreach (var project in solutionInfo.ProjectInfos)
            {
                markdown.AppendLine(GenerateProjectMarkdown(project, solutionInfo.ProjectConfigurationPlatforms));
            }

            // Add Service Fabric project details
            markdown.AppendLine("\n### Service Fabric Projects:");
            foreach (var sfProject in solutionInfo.ServiceFabricProjects)
            {
                markdown.AppendLine(GenerateServiceFabricMarkdown(sfProject));
            }

            return markdown.ToString();
        }

        public string GenerateProjectMarkdown(ProjectInfoModel projectInfo, Dictionary<string, Dictionary<string, string>> projectConfigPlatforms)
        {
            var markdown = new StringBuilder();
            markdown.AppendLine($"### Project: {projectInfo.ProjectName}\n");

            // Add build configurations for the project
            markdown.AppendLine("#### Build Configurations:");
            if (projectConfigPlatforms.ContainsKey(projectInfo.ProjectName) && projectConfigPlatforms[projectInfo.ProjectName].Any())
            {
                foreach (var config in projectConfigPlatforms[projectInfo.ProjectName])
                {
                    markdown.AppendLine($"- **{config.Key}**: {config.Value}");
                }
            }
            else
            {
                markdown.AppendLine("No build configurations found.");
            }

            // Add project dependencies
            if (projectInfo.ProjectDependencies.Any())
            {
                markdown.AppendLine("\n#### Dependencies:");
                foreach (var dependency in projectInfo.ProjectDependencies)
                {
                    markdown.AppendLine($"- **{dependency.Key}**: {string.Join(", ", dependency.Value)}");
                }
            }
            else
            {
                markdown.AppendLine("\nNo dependencies found.");
            }

            // Add package references
            if (projectInfo.PackageReferences.Any())
            {
                markdown.AppendLine("\n#### Package References:");
                foreach (var package in projectInfo.PackageReferences)
                {
                    markdown.AppendLine($"- **{package.Key}**: {string.Join(", ", package.Value)}");
                }
            }
            else
            {
                markdown.AppendLine("\nNo package references found.");
            }

            return markdown.ToString();
        }

        public string GenerateServiceFabricMarkdown(ServiceFabricProjectInfoModel serviceFabricProject)
        {
            var markdown = new StringBuilder();
            markdown.AppendLine($"### Service Fabric Project: {serviceFabricProject.ProjectFilePath}\n");

            markdown.AppendLine($"- **Project Version**: {serviceFabricProject.ProjectVersion}");
            markdown.AppendLine($"- **Target Framework**: {serviceFabricProject.TargetFrameworkVersion}");

            // List services if any
            if (serviceFabricProject.Services.Any())
            {
                markdown.AppendLine("\n#### Services:");
                foreach (var service in serviceFabricProject.Services)
                {
                    markdown.AppendLine($"- {service}");
                }
            }
            else
            {
                markdown.AppendLine("\nNo services found.");
            }

            // Add application manifest
            markdown.AppendLine($"\n#### Application Manifest: {serviceFabricProject.ApplicationManifest}");

            // List application parameters if any
            if (serviceFabricProject.ApplicationParameters.Any())
            {
                markdown.AppendLine("\n#### Application Parameters:");
                foreach (var param in serviceFabricProject.ApplicationParameters)
                {
                    markdown.AppendLine($"- {param}");
                }
            }
            else
            {
                markdown.AppendLine("\nNo application parameters found.");
            }

            // List publish profiles if any
            if (serviceFabricProject.PublishProfiles.Any())
            {
                markdown.AppendLine("\n#### Publish Profiles:");
                foreach (var profile in serviceFabricProject.PublishProfiles)
                {
                    markdown.AppendLine($"- {profile}");
                }
            }
            else
            {
                markdown.AppendLine("\nNo publish profiles found.");
            }

            return markdown.ToString();
        }

        public string CombineMarkdownSections(IEnumerable<string> markdownSections)
        {
            return string.Join("\n\n---\n\n", markdownSections);
        }
    }
}