using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using ConductorDotnetClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConductorDotnetClient.Extensions;

namespace demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var workers = new List<IWorkflowTask>
            {
                new SampleWorker()
            };

            var serviceProvider = new ServiceCollection()
                .AddLogging(p => p.AddConsole())
                .AddTransient<SampleWorker>()
                .AddConductorClient("http://localhost:8080/api/")
                .BuildServiceProvider();

            var workflowTaskCoordinator= serviceProvider.GetRequiredService<IWorkflowTaskCoordinator>();
            workflowTaskCoordinator.RegisterWorker<SampleWorker>();
            await workflowTaskCoordinator.Start();

        }
    }
}
