namespace ProjectDocumentationTool.Models
{
    public class SolutionInfoModel
    {
        public string Name { get; set; }
        public string SolutionPath { get; internal set; }

        public List<ProjectInfoModel> ProjectInfos { get; set; }
        public List<ServiceFabricProjectInfoModel> ServiceFabricProjects { get; set; }

        // New properties for solution-level build configurations
        public List<string> SolutionConfigurationPlatforms { get; set; }
        public Dictionary<string, Dictionary<string, string>> ProjectConfigurationPlatforms { get; set; }

        // New properties to capture active and build configurations
        public Dictionary<string, Dictionary<string, ConfigurationDetail>> ProjectConfigDetails { get; set; }
        public int SolutionRepoId { get; internal set; }

        public SolutionInfoModel()
        {
            SolutionConfigurationPlatforms = new List<string>();
            ProjectConfigurationPlatforms = new Dictionary<string, Dictionary<string, string>>();
            ProjectConfigDetails = new Dictionary<string, Dictionary<string, ConfigurationDetail>>();
        }
    }
}