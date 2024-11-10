using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using ProjectDocumentationTool.Models;
using ProjectDocumentationTool.Interfaces;
using System.Xml.Linq;

namespace ProjectDocumentationTool.Implementation
{
    public class SourceAnalyser : ISourceAnalyser
    {
        public SourceAnalyser()
        {
            MSBuildLocator.RegisterDefaults();
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

            // Load and parse solution configurations
            LoadSolutionConfigurations(solutionPath, solutionInfo);

            foreach (Microsoft.CodeAnalysis.Project project in solution.Projects)
            {
                ProjectInfoModel projectInfo = new ProjectInfoModel
                {
                    ProjectName = project.Name,
                    ProjectPath = project.FilePath
                };

                // Parse individual project configurations and properties
                ParseProjectBuildProperties(projectInfo);
                // Analyze project dependencies (other projects referenced by this one)
                foreach (Microsoft.CodeAnalysis.ProjectReference projectReference in project.ProjectReferences)
                {
                    // Add project dependencies in the format {ProjectName: [List of Dependencies]}
                    string projectName = projectReference.ProjectId.Id.ToString(); // You can adjust to use the project name for readability
                    if (!projectInfo.ProjectDependencies.ContainsKey(projectName))
                    {
                        projectInfo.ProjectDependencies[projectName] = new List<string>();
                    }
                    projectInfo.ProjectDependencies[projectName].Add(projectName);
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

                // Process ProjectConfigurationPlatforms lines
                if (isProjectConfigSection)
                {
                    string[] parts = line.Split('.');
                    if (parts.Length >= 3)
                    {
                        string projectGuid = parts[0].Trim();
                        string config = parts[1].Trim();
                        string platform = line.Split('=')[1].Trim();

                        if (!solutionInfo.ProjectConfigurationPlatforms.ContainsKey(projectGuid))
                        {
                            solutionInfo.ProjectConfigurationPlatforms[projectGuid] = new Dictionary<string, string>();
                        }
                        solutionInfo.ProjectConfigurationPlatforms[projectGuid][config] = platform;
                    }
                }
            }
        }

        private void ParseProjectBuildProperties(ProjectInfoModel projectInfo)
        {
            var projectFilePath = projectInfo.ProjectPath;
            if (File.Exists(projectFilePath))
            {
                var projectXml = XDocument.Load(projectFilePath);
                projectInfo.TargetFramework = projectXml.Descendants("TargetFramework").FirstOrDefault()?.Value;
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
