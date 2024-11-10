namespace ProjectDocumentationTool.Models
{
    public class ProjectInfo
    {
        public string SolutionName { get; set; }
        public List<string> ProjectNames { get; set; }
        public Dictionary<string, List<string>> ProjectDependencies { get; set; }
        public Dictionary<string, List<string>> PackageReferences { get; set; }
    }
}
