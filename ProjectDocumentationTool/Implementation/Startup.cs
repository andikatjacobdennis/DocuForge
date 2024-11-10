using Microsoft.Extensions.DependencyInjection;
using ProjectDocumentationTool.Interfaces;

namespace ProjectDocumentationTool.Implementation
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDiagramGenerator, DiagramGenerator>();
            services.AddTransient<ISourceAnalyser, SourceAnalyser>();
            services.AddTransient<IMenuService, MenuService>();
        }
    }
}
