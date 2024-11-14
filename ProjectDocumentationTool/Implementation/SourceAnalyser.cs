using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using ProjectDocumentationTool.Models;
using ProjectDocumentationTool.Interfaces;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ProjectDocumentationTool.Utilities;

namespace ProjectDocumentationTool.Implementation
{
    public class SourceAnalyser : ISourceAnalyser
    {
        private readonly IDiagramGenerator _diagramGenerator;
        private readonly ILogger<SourceAnalyser> _logger;

        public SourceAnalyser(IDiagramGenerator diagramGenerator, ILogger<SourceAnalyser> logger)
        {
            MSBuildLocator.RegisterDefaults();
            _diagramGenerator = diagramGenerator;
            _logger = logger;
        }

        public SolutionInfoModel AnalyzeSolution(string solutionPath)
        {
            SolutionInfoModel solutionInfo = new SolutionInfoModel
            {
                Name = Path.GetFileName(solutionPath),
                ProjectInfos = new List<ProjectInfoModel>(),
                ServiceFabricProjects = new List<ServiceFabricProjectInfoModel>()
            };

            using MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Microsoft.CodeAnalysis.Solution solution = workspace.OpenSolutionAsync(solutionPath).Result;

            // Parse the solution file to extract project GUIDs and paths
            Dictionary<string, string> projectGuidMap = ExtractProjectGuidsFromSolution(solutionPath);

            // Load and parse solution configurations
            LoadSolutionConfigurations(solutionPath, solutionInfo);

            foreach (Microsoft.CodeAnalysis.Project project in solution.Projects)
            {
                ProjectInfoModel projectInfo = new ProjectInfoModel
                {
                    ProjectName = project.Name,
                    ProjectPath = project.FilePath,
                    ProjectFolder = Path.GetDirectoryName(project.FilePath)
                };

                // Retrieve the Project GUID from the solution file (if available)
                if (projectGuidMap.TryGetValue(project.FilePath, out string projectGuid))
                {
                    projectInfo.Guid = projectGuid;
                }
                else
                {
                    // Handle the case where the GUID is not found in the solution file
                    Console.WriteLine($"Warning: GUID not found for project {project.Name}.");
                }

                // Parse individual project configurations and properties
                ParseProjectBuildProperties(projectInfo);

                // Analyze project dependencies (other projects referenced by this one)
                foreach (Microsoft.CodeAnalysis.ProjectReference projectReference in project.ProjectReferences)
                {
                    // Get the name of the referenced project
                    string projectId = projectReference.ProjectId.ToString(); // You can adjust to use the project name for readability
                    string referencedProjectFilePath = ExtractFilePathFromProjectId(projectId);
                    string referencedProjectGuid = string.Empty;
                    string referencedProjectName = Path.GetFileNameWithoutExtension(referencedProjectFilePath);

                    // Retrieve the Project GUID from the solution file (if available)
                    if (projectGuidMap.TryGetValue(referencedProjectFilePath, out string referencedProject))
                    {
                        referencedProjectGuid = referencedProject;
                    }
                    else
                    {
                        // Handle the case where the GUID is not found in the solution file
                        Console.WriteLine($"Warning: GUID not found for referenced project {project.Name}.");
                    }

                    // Avoid adding self-dependency (i.e., the project should not be listed as its own dependency)
                    if (referencedProjectName != projectInfo.ProjectName)
                    {
                        if (!projectInfo.ProjectDependencies.ContainsKey(referencedProjectName))
                        {
                            projectInfo.ProjectDependencies[referencedProjectName] = new List<string>();
                        }
                        projectInfo.ProjectDependencies[referencedProjectName].Add(referencedProjectGuid); // Add the current project to the referenced project's list of dependencies
                    }
                }


                // Analyze NuGet package references
                Dictionary<string, string> nugetPackages = GetNuGetPackageReferences(project.FilePath);
                foreach (KeyValuePair<string, string> package in nugetPackages)
                {
                    if (!projectInfo.PackageReferences.ContainsKey(package.Key))
                    {
                        projectInfo.PackageReferences[package.Key] = new List<string>();
                    }
                    projectInfo.PackageReferences[package.Key].Add(package.Value);
                }

                // Generate class interaction diagram

                string classInteractionDiagram = string.Empty;

                // Define the path to the executable
                var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                           @"..\..\..\..\diagram-generator-master\diagram-generator-master\DiagramGenerator\bin\Debug\DiagramGenerator.exe");

                projectInfo.ClassInteractionDiagram = _diagramGenerator.GenerateClassDiagramAsync(exePath, projectInfo.ProjectFolder, 1000).Result;

                solutionInfo.ProjectInfos.Add(projectInfo);
            }

            // Handle .sfproj files separately
            List<string> sfprojPaths = GetSfProjPaths(solutionPath);
            foreach (string sfprojPath in sfprojPaths)
            {
                ServiceFabricProjectInfoModel serviceFabricProjectInfo = new ServiceFabricProjectInfoModel();
                serviceFabricProjectInfo.PopulateFromSfproj(sfprojPath);
                solutionInfo.ServiceFabricProjects.Add(serviceFabricProjectInfo);
            }

            return solutionInfo;
        }

        // Method to extract the file path
        private string ExtractFilePathFromProjectId(string input)
        {
            // Regular expression to match the file path
            string pattern = @"-\s([A-Za-z]:\\(?:[^\\\r\n]+\\)*[^\\\r\n]+\.csproj)";

            // Match the pattern
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                // Return the file path from the match group
                return match.Groups[1].Value;
            }
            else
            {
                // Return null if no match is found
                return null;
            }
        }

        private Dictionary<string, string> ExtractProjectGuidsFromSolution(string solutionPath)
        {
            Dictionary<string, string> projectGuidMap = new Dictionary<string, string>();

            // Get the directory path of the solution file
            string? solutionDirectory = Path.GetDirectoryName(solutionPath);

            // Read the .sln file to extract project paths and GUIDs
            string[] lines = File.ReadAllLines(solutionPath);

            foreach (string line in lines)
            {
                // Check if this is a "Project" line
                if (line.StartsWith("Project(") && line.Contains("= "))
                {
                    // Split the line by commas, taking care to handle the part that includes the GUID
                    string[] parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 3)
                    {
                        // Extract project path and GUID, making sure braces are preserved
                        string relativeProjectPath = parts[1].Trim().Trim('"'); // Get relative path without quotes
                        string absoluteProjectPath = Path.Combine(solutionDirectory, relativeProjectPath); // Make path absolute
                        string projectGuid = parts[2].Trim().Trim('"'); // Retain braces around the GUID

                        // Add the mapping to the dictionary
                        projectGuidMap[absoluteProjectPath] = projectGuid;

                        // Log the extracted values for debugging
                        Console.WriteLine($"Absolute Project Path: {absoluteProjectPath}, GUID: {projectGuid}");
                    }
                }
            }

            return projectGuidMap;
        }

        private void LoadSolutionConfigurations(string solutionPath, SolutionInfoModel solutionInfo)
        {
            string[] solutionFile = File.ReadAllLines(solutionPath);
            bool isSolutionConfigSection = false;
            bool isProjectConfigSection = false;

            foreach (string line in solutionFile)
            {
                // Start of SolutionConfigurationPlatforms section
                if (line.Trim().StartsWith("GlobalSection(SolutionConfigurationPlatforms)"))
                {
                    isSolutionConfigSection = true;
                    continue;
                }

                // End of SolutionConfigurationPlatforms section
                if (isSolutionConfigSection && line.Trim().StartsWith("EndGlobalSection"))
                {
                    isSolutionConfigSection = false;
                    continue;
                }

                // Start of ProjectConfigurationPlatforms section
                if (line.Trim().StartsWith("GlobalSection(ProjectConfigurationPlatforms)"))
                {
                    isProjectConfigSection = true;
                    continue;
                }

                // End of ProjectConfigurationPlatforms section
                if (isProjectConfigSection && line.Trim().StartsWith("EndGlobalSection"))
                {
                    isProjectConfigSection = false;
                    continue;
                }

                // Process SolutionConfigurationPlatforms lines
                if (isSolutionConfigSection)
                {
                    string config = line.Split('=')[0].Trim();
                    solutionInfo.SolutionConfigurationPlatforms.Add(config);
                }

                if (isProjectConfigSection)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string projectGuidWithConfig = parts[0].Trim();  // e.g., {63B50589-0460-4849-8F15-CB04F4B5F8A7}.Debug|x64.Build.0
                        string configDetails = parts[1].Trim();  // e.g., Debug|x64
                        configDetails = parts[1].Replace("|", "\\|");  // e.g., Debug|x64 --> Debug\|x64


                        // Debug: Log the configDetails to inspect its content
                        // Console.WriteLine($"Processing configuration for project {projectGuidWithConfig}: {configDetails}");

                        // Step 1: Split the project GUID and config part by '.'
                        string[] guidAndConfigParts = projectGuidWithConfig.Split('.');

                        // Check if the GUID and config part format is valid
                        if (guidAndConfigParts.Length >= 3 && guidAndConfigParts.Length <= 4) // Handle both cases with and without .0
                        {
                            string projectGuid = guidAndConfigParts[0].Trim();  // e.g., {63B50589-0460-4849-8F15-CB04F4B5F8A7}
                            string configPart = guidAndConfigParts[1].Trim();  // e.g., Debug|x64 or Debug|x64.Build.0

                            // Step 2: Split configPart by '|' to get the configuration and platform
                            string[] configPlatformParts = configPart.Split('|');
                            if (configPlatformParts.Length == 2)
                            {
                                string config = configPlatformParts[0].Trim();  // e.g., Debug
                                string platform = configPlatformParts[1].Trim();  // e.g., x64

                                string configType = string.Empty;

                                // Step 3: Handle the configType, checking for the case with .0 (e.g., Build.0)
                                if (guidAndConfigParts.Length == 3)
                                {
                                    // If there's a .0 or other suffix, split by '.' to extract the configType (e.g., Build or Deploy)
                                    string[] configTypeParts = guidAndConfigParts[2].Split('.');
                                    if (configTypeParts.Length > 1)
                                    {
                                        // Handle the case where `.0` exists: Treat `.0` as part of the configuration type (e.g., Build.0 becomes Build)
                                        configType = configTypeParts[0].Trim();  // e.g., Build or Deploy
                                    }
                                    else
                                    {
                                        configType = guidAndConfigParts[2].Trim(); // e.g., ActiveCfg if only one part remains
                                    }
                                }
                                else
                                {
                                    // If there's no .0 part, it's likely ActiveCfg
                                    configType = "ActiveCfg";
                                }

                                // Ensure the project exists in ProjectConfigDetails
                                if (!solutionInfo.ProjectConfigDetails.ContainsKey(projectGuid))
                                {
                                    solutionInfo.ProjectConfigDetails[projectGuid] = new Dictionary<string, ConfigurationDetail>();
                                }

                                if (!solutionInfo.ProjectConfigDetails[projectGuid].ContainsKey(config))
                                {
                                    solutionInfo.ProjectConfigDetails[projectGuid][config] = new ConfigurationDetail();
                                }

                                ConfigurationDetail configDetail = solutionInfo.ProjectConfigDetails[projectGuid][config];

                                // Step 4: Set the correct configuration type (ActiveCfg, BuildCfg, DeployCfg)
                                if (configType.Equals("ActiveCfg"))
                                {
                                    configDetail.ActiveCfg = configDetails;  // Store ActiveCfg
                                }
                                else if (configType.Equals("Build"))
                                {
                                    configDetail.BuildCfg = configDetails;  // Store BuildCfg
                                }
                                else if (configType.Equals("Deploy"))
                                {
                                    configDetail.DeployCfg = configDetails;  // Store DeployCfg
                                }
                                else
                                {
                                    // Log unexpected configType
                                    Console.WriteLine($"Unexpected configType: {configType}");
                                }

                                // Debug: Log the updated configuration detail to verify it is being set correctly
                                // Console.WriteLine($"Updated configDetail for project {projectGuid}, config {config}: {configType} -> {configDetails}");
                            }
                            else
                            {
                                // Handle case where the platform isn't present
                                Console.WriteLine($"Warning: Unexpected config|platform format for project {projectGuid}: {configPart}");
                            }
                        }
                        else
                        {
                            // Handle case where the GUID and config format isn't as expected
                            Console.WriteLine($"Warning: Invalid format for project configuration line: {line}");
                        }
                    }
                }
            }
        }

        private void ParseProjectBuildProperties(ProjectInfoModel projectInfo)
        {
            string projectFilePath = projectInfo.ProjectPath;
            if (File.Exists(projectFilePath))
            {
                // Load the project file as XML
                XDocument projectXml = XDocument.Load(projectFilePath);

                // Extract TargetFramework
                projectInfo.TargetFramework = projectXml.Descendants("TargetFramework").FirstOrDefault()?.Value;

                // Extract PlatformTarget
                projectInfo.PlatformTarget = projectXml.Descendants("PlatformTarget").FirstOrDefault()?.Value;
            }
        }

        private List<string> GetSfProjPaths(string solutionPath)
        {
            List<string> sfprojPaths = new List<string>();
            string[] solutionFile = File.ReadAllLines(solutionPath);

            foreach (string line in solutionFile)
            {
                if (line.Trim().StartsWith("Project(") && line.Contains(".sfproj"))
                {
                    string projectPath = line.Split(',')[1].Trim().Trim('"');
                    string fullProjectPath = Path.Combine(Path.GetDirectoryName(solutionPath), projectPath);
                    if (File.Exists(fullProjectPath))
                    {
                        sfprojPaths.Add(fullProjectPath);
                    }
                }
            }

            return sfprojPaths;
        }

        private Dictionary<string, string> GetNuGetPackageReferences(string projectFilePath)
        {
            Dictionary<string, string> packageReferences = new Dictionary<string, string>();

            try
            {
                // Load the .csproj file to extract NuGet package references
                XDocument projectXml = XDocument.Load(projectFilePath);

                // Look for <PackageReference> elements in the project file
                IEnumerable<XElement> packageElements = projectXml.Descendants("PackageReference");

                foreach (XElement packageElement in packageElements)
                {
                    string? packageName = packageElement.Attribute("Include")?.Value;
                    string? packageVersion = packageElement.Attribute("Version")?.Value;

                    if (packageName != null && packageVersion != null)
                    {
                        packageReferences[packageName] = packageVersion;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the error, if necessary
                Console.WriteLine($"Error reading NuGet packages for project {projectFilePath}: {ex.Message}");
            }

            return packageReferences;
        }
    }
}
