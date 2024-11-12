using Newtonsoft.Json;

namespace ProjectDocumentationTool.Utilities
{
    public class PlantUmlConverter
    {
        public string ConvertPlantUmlToJson(string plantUmlFilePath)
        {
            // Read the PlantUML file
            string[] lines = File.ReadAllLines(plantUmlFilePath);

            HashSet<string> nodes = new HashSet<string>();
            List<Link> links = new List<Link>();

            foreach (string line in lines)
            {
                if (line.StartsWith("@startuml") || line.StartsWith("@enduml"))
                    continue;

                if (line.Contains("-->"))
                {
                    string[] parts = line.Split(new[] { "-->" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        string source = parts[0].Trim();
                        string target = parts[1].Split(':')[0].Trim(); // Get target before colon
                        nodes.Add(source);
                        nodes.Add(target);
                        links.Add(new Link { Source = source, Target = target, Relationship = "references" });
                    }
                }
                else if (line.StartsWith("component"))
                {
                    string componentName = line.Substring("component ".Length).Trim();
                    nodes.Add(componentName);
                }
            }

            // Create the JSON structure
            var jsonStructure = new
            {
                layout = new
                {
                    force = new
                    {
                        charge = -200,
                        linkDistance = 150,
                        centerX = 400,
                        centerY = 300
                    }
                },
                nodes = nodes.Select(n => new { id = n }).ToList(),
                links
            };

            // Serialize to JSON and return as string
            return JsonConvert.SerializeObject(jsonStructure, Formatting.Indented);
        }
    }
}
