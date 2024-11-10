namespace ProjectDocumentationTool.Models
{
    public class ProjectInfoModel
    {
        public string ProjectName { get; set; }
        public Dictionary<string, List<string>> ProjectDependencies { get; set; }
        public Dictionary<string, List<string>> PackageReferences { get; set; }
    }
}