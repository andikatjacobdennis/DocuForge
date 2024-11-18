namespace ProjectDocumentationTool.Models
{
    public class MethodInfo
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<ParameterInfo> Parameters { get; set; } = new();
        public string Accessibility { get; set; }
        public string XmlComment { get; set; }
    }
}