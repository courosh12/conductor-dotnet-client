using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using ConductorDotnetClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var workers = new List<IWorker>
            {
                new SampleWorker()
            };

            var serviceProvider = new ServiceCollection()
                .AddLogging(p => p.AddConsole())
                .AddConductorClient(workers,"http://localhost:8080/api/")
                .BuildServiceProvider();

            var workflowTaskCoordinator= serviceProvider.GetRequiredService<IWorkflowTaskCoordinator>();
            await workflowTaskCoordinator.Start();

        }
    }
}
