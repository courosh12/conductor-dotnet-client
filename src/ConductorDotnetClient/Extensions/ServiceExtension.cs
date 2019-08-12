using ConductorDotnetClient.Client;
using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Swagger.Api;
using ConductorDotnetClient.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ConductorDotnetClient.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddConductorClient(this IServiceCollection serviceProvider,
            string serverUrl,
            int concurrentWorkers=1,
            int sleepInterval = 1000)
        {
            serviceProvider.AddSingleton<IWorkflowTaskCoordinator>(p=>{
                return new WorkflowTaskCoordinator(p,p.GetService<ILogger<WorkflowTaskCoordinator>>(), concurrentWorkers);
            });

            serviceProvider.AddTransient<IConductorRestClient>(p => {
                return new ConductorRestClient(serverUrl, new HttpClient());
            });

            serviceProvider.AddTransient<ITaskClient, RestTaskClient>();

            serviceProvider.AddTransient<IWorkflowTaskExecutor>(p =>
            {
                return new WorkflowTaskExecutor(p.GetService<ITaskClient>(), 
                    p,
                    p.GetService<ILogger<WorkflowTaskExecutor>>(),
                    sleepInterval);
            });

            return serviceProvider;
        }
    }
}
