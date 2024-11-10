using System.Xml.Linq;

namespace ProjectDocumentationTool.Models
{
    public class ServiceFabricProjectInfoModel
    {
        // Path to the .sfproj file
        public string ProjectFilePath { get; set; }

        // List of services defined in the Service Fabric project
        public List<string> Services { get; set; }

        // Application Manifest (for Service Fabric)
        public string ApplicationManifest { get; set; }

        // Application Parameters for different configurations (Cloud/Local)
        public List<string> ApplicationParameters { get; set; }

        // List of publish profiles (Cloud/Local)
        public List<string> PublishProfiles { get; set; }

        // Deploy script path (if available)
        public string DeployScript { get; set; }

        // Framework and versioning info
        public string TargetFrameworkVersion { get; set; }
        public string ProjectVersion { get; set; }

        // Project Reference information
        public List<string> ProjectReferences { get; set; }

        // Constructor to initialize the lists to prevent null reference errors
        public ServiceFabricProjectInfoModel()
        {
            Services = new List<string>();
            ApplicationParameters = new List<string>();
            PublishProfiles = new List<string>();
            ProjectReferences = new List<string>();
        }

        // Method to populate the Service Fabric project info from the .sfproj XML
        public void PopulateFromSfproj(string sfprojFilePath)
        {
            // Load the .sfproj file
            XDocument sfprojXml = XDocument.Load(sfprojFilePath);
            XElement root = sfprojXml.Root;

            // Define the namespace
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";

            // Extract Target Framework Version
            var targetFrameworkVersionElement = root.Descendants(ns + "TargetFrameworkVersion").FirstOrDefault();
            TargetFrameworkVersion = targetFrameworkVersionElement?.Value;

            // Extract Project Version
            var projectVersionElement = root.Descendants(ns + "ProjectVersion").FirstOrDefault();
            ProjectVersion = projectVersionElement?.Value;

            // Extract Services (if applicable)
            var serviceElements = root.Descendants(ns + "ItemGroup")
                                      .Descendants(ns + "Service")
                                      .Select(e => e.Value).ToList();
            Services.AddRange(serviceElements);

            // Extract Application Manifest
            var applicationManifestElement = root.Descendants(ns + "ItemGroup")
                                                 .Descendants(ns + "None")
                                                 .Where(e => e.Attribute("Include")?.Value.Contains("ApplicationManifest.xml") == true)
                                                 .FirstOrDefault();
            ApplicationManifest = applicationManifestElement?.Attribute("Include")?.Value;

            // Extract Application Parameters (Cloud/Local)
            var applicationParamsElements = root.Descendants(ns + "ItemGroup")
                                                 .Descendants(ns + "None")
                                                 .Where(e => e.Attribute("Include")?.Value.Contains("ApplicationParameters") == true)
                                                 .Select(e => e.Attribute("Include")?.Value)
                                                 .ToList();
            ApplicationParameters.AddRange(applicationParamsElements);

            // Extract Publish Profiles
            var publishProfilesElements = root.Descendants(ns + "ItemGroup")
                                               .Descendants(ns + "None")
                                               .Where(e => e.Attribute("Include")?.Value.Contains("PublishProfiles") == true)
                                               .Select(e => e.Attribute("Include")?.Value)
                                               .ToList();
            PublishProfiles.AddRange(publishProfilesElements);

            // Extract Deploy Script (if available)
            var deployScriptElement = root.Descendants(ns + "ItemGroup")
                                          .Descendants(ns + "None")
                                          .Where(e => e.Attribute("Include")?.Value.Contains("Deploy-FabricApplication.ps1") == true)
                                          .FirstOrDefault();
            DeployScript = deployScriptElement?.Attribute("Include")?.Value;

            // Extract Project References (e.g., references to other projects like Ir.ActivityStream.Microservice)
            var projectReferenceElements = root.Descendants(ns + "ItemGroup")
                                               .Descendants(ns + "ProjectReference")
                                               .Select(e => e.Attribute("Include")?.Value)
                                               .ToList();
            ProjectReferences.AddRange(projectReferenceElements);
        }

        // Private method to generate a human-readable summary
        private string GenerateSummary()
        {
            var summary = "Service Fabric Project Summary:\n";
            summary += $"Project File: {ProjectFilePath}\n";
            summary += $"Project Version: {ProjectVersion ?? "Not specified"}\n";
            summary += $"Target Framework Version: {TargetFrameworkVersion ?? "Not specified"}\n\n";

            // Services (if available)
            summary += "Services:\n";
            if (Services.Any())
            {
                foreach (var service in Services)
                {
                    summary += $"- {service}\n";
                }
            }
            else
            {
                summary += "- No services defined.\n";
            }

            // Application Manifest
            summary += $"\nApplication Manifest: {(ApplicationManifest ?? "Not specified")}\n";

            // Application Parameters
            summary += "\nApplication Parameters:\n";
            if (ApplicationParameters.Any())
            {
                foreach (var param in ApplicationParameters)
                {
                    summary += $"- {param}\n";
                }
            }
            else
            {
                summary += "- No application parameters defined.\n";
            }

            // Publish Profiles
            summary += "\nPublish Profiles:\n";
            if (PublishProfiles.Any())
            {
                foreach (var profile in PublishProfiles)
                {
                    summary += $"- {profile}\n";
                }
            }
            else
            {
                summary += "- No publish profiles defined.\n";
            }

            // Deploy Script
            summary += $"\nDeploy Script: {(DeployScript ?? "Not specified")}\n";

            // Project References
            summary += "\nProject References:\n";
            if (ProjectReferences.Any())
            {
                foreach (var projectRef in ProjectReferences)
                {
                    summary += $"- {projectRef}\n";
                }
            }
            else
            {
                summary += "- No project references found.\n";
            }

            return summary;
        }

        // Public method to get the human-readable summary
        public string GetSummary()
        {
            return GenerateSummary();
        }
    }
}
