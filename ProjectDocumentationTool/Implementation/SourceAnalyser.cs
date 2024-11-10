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
            // Register MSBuild instance to make MSBuild APIs accessible
            MSBuildLocator.RegisterDefaults();
        }

        public SolutionInfoModel AnalyzeSolution(string solutionPath)
        {
            // Create an empty SolutionInfo object to hold all project data
            SolutionInfoModel solutionInfo = new SolutionInfoModel
            {
                Name = Path.GetFileName(solutionPath),
                ProjectInfos = new List<ProjectInfoModel>(),
                ServiceFabricProjects = new List<ServiceFabricProjectInfoModel>()
            };

            // Load the solution using Roslyn's MSBuild API
            using MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Microsoft.CodeAnalysis.Solution solution = workspace.OpenSolutionAsync(solutionPath).Result;

            foreach (Microsoft.CodeAnalysis.Project project in solution.Projects)
            {
                // For each project, create a ProjectInfo object
                ProjectInfoModel projectInfo = new ProjectInfoModel
                {
                    ProjectName = project.Name,
                    ProjectDependencies = new Dictionary<string, List<string>>(),
                    PackageReferences = new Dictionary<string, List<string>>()
                };

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

                // Add the populated ProjectInfo object to the SolutionInfo list
                solutionInfo.ProjectInfos.Add(projectInfo);
            }

            // Manually load .sfproj files not handled by MSBuildWorkspace
            List<string> sfprojPaths = GetSfProjPaths(solutionPath);
            foreach (string sfprojPath in sfprojPaths)
            {
                ServiceFabricProjectInfoModel serviceFabricProjectInfo = new ServiceFabricProjectInfoModel();
                serviceFabricProjectInfo.PopulateFromSfproj(sfprojPath);
                solutionInfo.ServiceFabricProjects.Add(serviceFabricProjectInfo);
            }

            // Return the populated SolutionInfo object
            return solutionInfo;
        }

        // Manually find and analyze .sfproj files
        private List<string> GetSfProjPaths(string solutionPath)
        {
            List<string> sfprojPaths = new List<string>();

            // Read the solution file (.sln) to find .sfproj references
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
