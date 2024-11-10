using System;
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
                //sb.AppendLine("skinparam linetype ortho"); // Optional styling for readability

                // Assuming you have a method or dictionary to map GUIDs to project names
                Dictionary<string, string> projectGuidToNameMap = GetProjectGuidToNameMap(solutionInfo.ProjectInfos);

                // Process each ProjectInfoModel in the solution
                foreach (ProjectInfoModel projectInfo in solutionInfo.ProjectInfos)
                {
                    // Get the project name using the GUID from the map
                    string projectName = projectGuidToNameMap[projectInfo.Guid];

                    _logger.LogInformation("Adding project node for: {ProjectName} with GUID: {ProjectGuid}", projectName, projectInfo.Guid);
                    sb.AppendLine($"package \"{projectName}\" {{");
                    sb.AppendLine($"  \"[{projectName}]\""); // Reference to project by name
                    sb.AppendLine("}");

                    // Add dependency relationships from ProjectDependencies
                    foreach (KeyValuePair<string, List<string>> dependency in projectInfo.ProjectDependencies)
                    {
                        foreach (string dep in dependency.Value)
                        {
                            string dependentName = projectInfo.ProjectName; 

                            _logger.LogDebug("Adding dependency from {ProjectName} to {Dependency}", dependentName, dependency.Key);
                            sb.AppendLine($"\"[{dependentName}]\" --> \"[{dependency.Key}]\" : Depends on");
                        }
                    }
                }

                // Process each ServiceFabricProjectInfoModel in the solution
                foreach (ServiceFabricProjectInfoModel sfProjectInfo in solutionInfo.ServiceFabricProjects)
                {
                    var projectName = Path.GetFileNameWithoutExtension(sfProjectInfo.ProjectFilePath);

                    _logger.LogInformation("Adding Service Fabric project node for: {SfProjectPath}", sfProjectInfo.ProjectFilePath);
                    sb.AppendLine($"package \"{projectName}\" {{");
                    sb.AppendLine($"  \"[{projectName}]\"");
                    sb.AppendLine("}");

                    // Add ProjectReferences in ServiceFabricProjectInfoModel
                    foreach (string projectReference in sfProjectInfo.ProjectReferences)
                    {
                        string referenceName = Path.GetFileNameWithoutExtension(projectReference);
                        _logger.LogDebug("Adding reference from {SfProjectPath} to {ReferenceName}", sfProjectInfo.ProjectFilePath, referenceName);
                        sb.AppendLine($"\"[{projectName}]\" --> \"[{referenceName}]\" : Depends on");
                    }

                    // Include Service Fabric services as nodes
                    foreach (string service in sfProjectInfo.Services)
                    {
                        _logger.LogDebug("Adding service node for: {Service}", service);
                        sb.AppendLine($"\"[{service}]\"");
                        sb.AppendLine($"\"[{projectName}]\" --> \"[{service}]\" : Contains");
                    }
                }

                // Finalize UML syntax
                sb.AppendLine("@enduml");

                // Ensure the folder exists
                var directoryPath = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Write to file
                _logger.LogInformation("Writing PlantUML diagram to {OutputPath}", outputPath);
                File.WriteAllText(outputPath, sb.ToString());

                _logger.LogInformation("Successfully generated the PlantUML dependency diagram for the solution.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the PlantUML dependency diagram for the solution.");
            }
        }

        // Helper method to build the mapping from GUID to project name
        private Dictionary<string, string> GetProjectGuidToNameMap(IEnumerable<ProjectInfoModel> projectInfos)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (ProjectInfoModel project in projectInfos)
            {
                map[project.Guid] = project.ProjectName; // Assuming ProjectInfoModel has ProjectName and Guid
            }
            return map;
        }
    }
}
