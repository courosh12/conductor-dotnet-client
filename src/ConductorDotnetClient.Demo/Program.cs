using ConductorDotnetClient.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ConductorDotnetClient.Extensions;
using System;

namespace ConductorDotnetClient.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(p => p.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddConductorWorkflowTask<SampleWorkerTask>()
                .AddConductorWorkflowTask<SampleWorkerTaskTheSecond>()
                .AddConductorWorker(new ConductorClientSettings()
                {
                    ConcurrentWorkers = 1,
                    Domain = "SampleDomain",
                    IntervalStrategy = ConductorClientSettings.IntervalStrategyType.Linear,
                    MaxSleepInterval = 15_000,
                    SleepInterval = 1_000,
                    ServerUrl = new Uri("http://10.40.80.180:22095/api/")
                })
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
