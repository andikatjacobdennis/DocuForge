namespace ProjectDocumentationTool.Models
{
    public class SolutionInfoModel
    {
        public string Name { get; set; }
        public List<ProjectInfoModel> ProjectInfos { get; set; }
        public List<ServiceFabricProjectInfoModel> ServiceFabricProjects { get; internal set; }
    }
}