using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using ProjectDocumentationTool.Models;

namespace ProjectDocumentationTool.Utilities
{
    public class PlantUmlDiagramGenerator
    {
        private readonly ILogger<PlantUmlDiagramGenerator> _logger;

        public PlantUmlDiagramGenerator(ILogger<PlantUmlDiagramGenerator> logger)
        {
            _logger = logger;
        }

        public void GenerateDependencyDiagram(SolutionInfoModel solutionInfo, string outputPath)
        {
            try
            {
                _logger.LogInformation("Starting to generate the PlantUML dependency diagram for the solution.");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("@startuml");
                sb.AppendLine("title Project Dependency Diagram");

                // Assuming a dictionary to map GUIDs to project names
                Dictionary<string, string> projectGuidToNameMap = GetProjectGuidToNameMap(solutionInfo.ProjectInfos);

                // Add components for each project in the solution
                foreach (ProjectInfoModel projectInfo in solutionInfo.ProjectInfos)
                {
                    string projectName = projectGuidToNameMap[projectInfo.Guid];
                    _logger.LogInformation("Adding project component: {ProjectName}", projectName);
                    sb.AppendLine($"component {projectName}");
                }

                // Add dependencies between projects
                foreach (ProjectInfoModel projectInfo in solutionInfo.ProjectInfos)
                {
                    string projectName = projectGuidToNameMap[projectInfo.Guid];

                    foreach (KeyValuePair<string, List<string>> dependency in projectInfo.ProjectDependencies)
                    {
                        foreach (string depGuid in dependency.Value)
                        {
                            if (projectGuidToNameMap.TryGetValue(depGuid, out string dependentName))
                            {
                                _logger.LogDebug("Adding dependency from {ProjectName} to {DependentName}", projectName, dependentName);
                                sb.AppendLine($"{projectName} --> {dependentName} : references");
                            }
                        }
                    }
                }

                // Process dependencies for Service Fabric projects
                foreach (ServiceFabricProjectInfoModel sfProjectInfo in solutionInfo.ServiceFabricProjects)
                {
                    string projectName = Path.GetFileNameWithoutExtension(sfProjectInfo.ProjectFilePath);
                    sb.AppendLine($"component {projectName}");

                    foreach (string projectReference in sfProjectInfo.ProjectReferences)
                    {
                        string referenceName = Path.GetFileNameWithoutExtension(projectReference);
                        _logger.LogDebug("Adding reference from {SfProjectPath} to {ReferenceName}", sfProjectInfo.ProjectFilePath, referenceName);
                        sb.AppendLine($"{projectName} --> {referenceName} : references");
                    }

                    foreach (string service in sfProjectInfo.Services)
                    {
                        sb.AppendLine($"component {service}");
                        sb.AppendLine($"{projectName} --> {service} : contains");
                    }
                }

                sb.AppendLine("@enduml");

                // Ensure the output directory exists
                var directoryPath = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Write to file
                _logger.LogInformation("Writing PlantUML diagram to {OutputPath}", outputPath);
                File.WriteAllText(outputPath, sb.ToString());

                _logger.LogInformation("Successfully generated the PlantUML dependency diagram.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the PlantUML dependency diagram.");
            }
        }

        private Dictionary<string, string> GetProjectGuidToNameMap(IEnumerable<ProjectInfoModel> projectInfos)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (ProjectInfoModel project in projectInfos)
            {
                map[project.Guid] = project.ProjectName;
            }
            return map;
        }
    }
}
