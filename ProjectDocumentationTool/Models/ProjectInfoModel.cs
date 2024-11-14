namespace ProjectDocumentationTool.Models
{
    public class ProjectInfoModel
    {
        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public string ClassInteractionDiagram { get; set; }
        public string PlatformTarget { get; set; } // e.g., x64
        public string TargetFramework { get; set; } // e.g., net7.0
        public string Guid { get; set; }

        public Dictionary<string, List<string>> ProjectDependencies { get; set; }
        public Dictionary<string, List<string>> PackageReferences { get; set; }
        public string? ProjectFolder { get; internal set; }
        public string ProjectType { get; internal set; }

        public ProjectInfoModel()
        {
            ProjectDependencies = new Dictionary<string, List<string>>();
            PackageReferences = new Dictionary<string, List<string>>();
        }
    }
}