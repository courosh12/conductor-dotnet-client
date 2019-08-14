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
        public static IServiceCollection AddConductorClient(this IServiceCollection services,
            Func<IServiceProvider, string> serverUrl,
            int concurrentWorkers = 1,
            int sleepInterval = 1000)
        {
            services.AddSingleton<IWorkflowTaskCoordinator>(p => {
                return new WorkflowTaskCoordinator(p, p.GetService<ILogger<WorkflowTaskCoordinator>>(), concurrentWorkers);
            });

            services.AddHttpClient<IConductorRestClient, CustomConductorRestClient>((provider, client) =>
            {
                client.BaseAddress = new Uri(serverUrl(provider));
            });

            services.AddTransient<ITaskClient, RestTaskClient>();

            services.AddTransient<IWorkflowTaskExecutor>(provider =>
            {
                return new WorkflowTaskExecutor(provider.GetService<ITaskClient>(),
                    provider,
                    provider.GetService<ILogger<WorkflowTaskExecutor>>(),
                    sleepInterval);
            });

            return services;
        }
    }
}
