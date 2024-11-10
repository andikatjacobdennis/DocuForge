using Microsoft.Extensions.DependencyInjection;
using ProjectDocumentationTool.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
