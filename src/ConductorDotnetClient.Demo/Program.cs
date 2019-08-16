using ConductorDotnetClient.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ConductorDotnetClient.Extensions;

namespace ConductorDotnetClient.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(p => p.AddConsole())
                .AddConductorWorkflowTask<SampleWorkerTask>()
                .AddConductorWorker(service => "http://localhost:8080/api/")
                .BuildServiceProvider();

            var workflowTaskCoordinator = serviceProvider.GetRequiredService<IWorkflowTaskCoordinator>();
            foreach(var worker in serviceProvider.GetServices<IWorkflowTask>())
            {
                workflowTaskCoordinator.RegisterWorker(worker);
            }
            
            await workflowTaskCoordinator.Start();

        }        
    }
}
