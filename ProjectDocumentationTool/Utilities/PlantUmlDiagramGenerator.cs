using System.Text;
using Microsoft.Extensions.Logging;
using PlantUml.Net;
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
                    if (projectInfo.Guid == null)
                    {
                        continue;
                    }
                    string projectName = projectGuidToNameMap[projectInfo.Guid];
                    _logger.LogInformation("Adding project component: {ProjectName}", projectName);
                    sb.AppendLine($"component {projectName}");
                }

                // Add dependencies between projects
                foreach (ProjectInfoModel projectInfo in solutionInfo.ProjectInfos)
                {
                    if (projectInfo.Guid == null)
                    {
                        continue;
                    }
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
                string? directoryPath = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Write to file
                _logger.LogInformation("Writing PlantUML diagram to {OutputPath}", outputPath);
                File.WriteAllText(outputPath, sb.ToString());

                _logger.LogInformation("Successfully generated the PlantUML dependency diagram.");

                // Read as json for debugging
                PlantUmlConverter PlantUmlConverter = new PlantUmlConverter();
                string json = PlantUmlConverter.ConvertPlantUmlToJson(outputPath);

                var factory = new RendererFactory();

                var renderer = factory.CreateRenderer(new PlantUmlSettings());

                var bytes = renderer.RenderAsync(sb.ToString(), OutputFormat.Svg).Result;

                var path = $"{Path.GetDirectoryName(outputPath)}\\{Path.GetFileNameWithoutExtension(outputPath)}.svg";
                var p = Path.GetFullPath(path);
                File.WriteAllBytes($"{Path.GetDirectoryName(outputPath)}\\{Path.GetFileNameWithoutExtension(outputPath)}.svg", bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the PlantUML dependency diagram.");
            }
        }

        internal void GenerateClassInteractionDiagram(ProjectInfoModel project, string outputPath)
        {

            // Ensure the output directory exists
            string? directoryPath = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var factory = new RendererFactory();

            var renderer = factory.CreateRenderer(new PlantUmlSettings());

            var bytes = renderer.RenderAsync(project.ClassInteractionDiagram, OutputFormat.Svg).Result;
            var path = $"{Path.GetDirectoryName(outputPath)}\\{Path.GetFileNameWithoutExtension(outputPath)}.svg";
            var p = Path.GetFullPath(path);
            File.WriteAllBytes(path, bytes);
        }

        private Dictionary<string, string> GetProjectGuidToNameMap(IEnumerable<ProjectInfoModel> projectInfos)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (ProjectInfoModel project in projectInfos)
            {
                if (project.Guid == null)
                {
                    continue;
                }
                map[project.Guid] = project.ProjectName;
            }
            return map;
        }
    }
}
