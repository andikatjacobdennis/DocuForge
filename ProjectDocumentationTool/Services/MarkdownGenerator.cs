using System.Text;
using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Services
{
    public class MarkdownGenerator : IMarkdownGenerator
    {
        public string GenerateSolutionMarkdown(SolutionInfoModel solutionInfo)
        {
            StringBuilder markdown = new StringBuilder();
            markdown.AppendLine($"## Solution: {solutionInfo.Name}\n");
            markdown.AppendLine($"![Visual Studio Project Depedency Diagram](./diagram/VisualStudioProjectDepedencyDiagram.svg)\n");

            // Add solution configurations (Build configurations)
            markdown.AppendLine("### Solution Configurations:");
            if (solutionInfo.SolutionConfigurationPlatforms.Any())
            {
                foreach (string config in solutionInfo.SolutionConfigurationPlatforms)
                {
                    markdown.AppendLine($"- {config}");
                }
            }
            else
            {
                markdown.AppendLine("No solution configurations found.");
            }

            // Add project details with a table for each project
            markdown.AppendLine("\n### Projects:");
            foreach (ProjectInfoModel project in solutionInfo.ProjectInfos)
            {
                markdown.AppendLine(GenerateProjectMarkdown(project, solutionInfo.ProjectConfigurationPlatforms));

                markdown.AppendLine($"#### {project.ProjectName}");
                markdown.AppendLine("| Configuration | ActiveCfg | BuildCfg | DeployCfg |");
                markdown.AppendLine("|---------------|-----------|----------|-----------|");

                // For each project, loop through its configuration details and display them
                if (solutionInfo.ProjectConfigDetails.ContainsKey(project.Guid))
                {
                    foreach (KeyValuePair<string, ConfigurationDetail> configEntry in solutionInfo.ProjectConfigDetails[project.Guid])
                    {
                        ConfigurationDetail configDetail = configEntry.Value;
                        string activeCfg = configDetail.ActiveCfg ?? "N/A";
                        string buildCfg = configDetail.BuildCfg ?? "N/A";
                        string deployCfg = configDetail.DeployCfg ?? "N/A";

                        markdown.AppendLine($"| {configEntry.Key} | {activeCfg} | {buildCfg} | {deployCfg} |");
                    }
                }
                else
                {
                    markdown.AppendLine("No configurations found for this project.");
                }
            }

            // Add Service Fabric project details
            markdown.AppendLine("\n### Service Fabric Projects:");
            foreach (ServiceFabricProjectInfoModel sfProject in solutionInfo.ServiceFabricProjects)
            {
                markdown.AppendLine(GenerateServiceFabricMarkdown(sfProject));
            }

            return markdown.ToString();
        }


        public string GenerateProjectMarkdown(ProjectInfoModel projectInfo, Dictionary<string, Dictionary<string, string>> projectConfigPlatforms)
        {
            StringBuilder markdown = new StringBuilder();
            markdown.AppendLine($"### Project: {projectInfo.ProjectName}\n");

            // Add project dependencies
            if (projectInfo.ProjectDependencies.Any())
            {
                markdown.AppendLine("\n#### Dependencies:");
                foreach (KeyValuePair<string, List<string>> dependency in projectInfo.ProjectDependencies)
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
                foreach (KeyValuePair<string, List<string>> package in projectInfo.PackageReferences)
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
            StringBuilder markdown = new StringBuilder();
            markdown.AppendLine($"### Service Fabric Project: {serviceFabricProject.ProjectFilePath}\n");

            markdown.AppendLine($"- **Project Version**: {serviceFabricProject.ProjectVersion}");
            markdown.AppendLine($"- **Target Framework**: {serviceFabricProject.TargetFrameworkVersion}");

            // List services if any
            if (serviceFabricProject.Services.Any())
            {
                markdown.AppendLine("\n#### Services:");
                foreach (string service in serviceFabricProject.Services)
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
                foreach (string param in serviceFabricProject.ApplicationParameters)
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
                foreach (string profile in serviceFabricProject.PublishProfiles)
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