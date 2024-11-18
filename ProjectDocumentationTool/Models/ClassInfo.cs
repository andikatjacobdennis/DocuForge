namespace ProjectDocumentationTool.Models
{
    public class ClassInfo
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string BaseClass { get; set; }
        public List<string> Interfaces { get; set; } = new();
        public List<string> GenericParameters { get; set; } = new();
        public List<PropertyInfo> Properties { get; set; } = new();
        public List<MethodInfo> Methods { get; set; } = new();
        public List<DelegateInfo> Delegates { get; set; } = new();
        public List<EventInfo> Events { get; set; } = new();
        public string XmlComment { get; set; }
    }
}