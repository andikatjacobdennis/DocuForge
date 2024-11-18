namespace ProjectDocumentationTool.Models
{
    public class EnumInfo
    {
        public string Name { get; set; }
        public string Accessibility { get; set; }
        public List<string> Members { get; set; } = new();
        public string XmlComment { get; set; }
    }
}