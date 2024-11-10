using ProjectDocumentationTool.Interfaces;

namespace ProjectDocumentationTool.Implementation
{
    public class DiagramGenerator : IDiagramGenerator
    {
        public void GenerateDiagram()
        {
            // Use PlantUML or another approach to generate diagram from ProjectInfo
            Console.WriteLine("Generating PlantUML diagram...");
        }
    }
}
