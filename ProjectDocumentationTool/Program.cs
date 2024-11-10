using Microsoft.Extensions.DependencyInjection;
using ProjectDocumentationTool.Implementation;
using ProjectDocumentationTool.Interfaces;
using Serilog;

namespace ProjectDocumentationTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Configure the logger to write to both the console and a log file, and enrich logs with additional context
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()           // Enrich log with context like thread ID, timestamp, etc.
                .Enrich.WithMachineName()          // Add machine name to the log entries
                .Enrich.WithThreadId()             // Add thread ID to the log entries
                .WriteTo.Console()                 // Log to the console
                .WriteTo.File("logs/log.txt",      // Log to a file
                    rollingInterval: RollingInterval.Day, // Create a new log file every day
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10 MB file size limit per file
                    retainedFileCountLimit: 7,     // Keep the last 7 log files
                    shared: true)                  // Make the file accessible across multiple processes (optional)
                .CreateLogger();

            // Create the service provider and configure dependency injection
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog()) // Add Serilog for logging
                .AddTransient<IDiagramGenerator, DiagramGenerator>()
                .AddTransient<ISourceAnalyser, SourceAnalyser>()
                .AddTransient<IMenuService, MenuService>()
                .BuildServiceProvider();

            // Resolve the menu service and call the method to display the menu
            var menuService = serviceProvider.GetService<IMenuService>();
            menuService?.DisplayMenu();

            // Ensure all log entries are flushed before the application exits
            Log.CloseAndFlush();
        }
    }
}
