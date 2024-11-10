namespace ProjectDocumentationTool.Models
{
    public class SolutionInfoModel
    {
        public string Name { get; set; }
        public List<ProjectInfoModel> ProjectInfos { get; set; }
        public List<ServiceFabricProjectInfoModel> ServiceFabricProjects { get; set; }

        // New properties for solution-level build configurations
        public List<string> SolutionConfigurationPlatforms { get; set; }
        public Dictionary<string, Dictionary<string, string>> ProjectConfigurationPlatforms { get; set; }

        public SolutionInfoModel()
        {
            SolutionConfigurationPlatforms = new List<string>();
            ProjectConfigurationPlatforms = new Dictionary<string, Dictionary<string, string>>();
        }
    }
}