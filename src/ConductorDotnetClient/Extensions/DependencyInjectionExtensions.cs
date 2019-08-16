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
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddConductorWorkflowTask<T>(this IServiceCollection services) where T : IWorkflowTask
        {
            services.AddTransient(typeof(IWorkflowTask), typeof(T));
            services.AddTransient(typeof(T));

            return services;
        }

        public static IServiceCollection AddConductorWorker(this IServiceCollection services, 
            Func<IServiceProvider, string> serverUrl, 
            int concurrentWorkers = 1, 
            int sleepInterval = 1000,
            string domain = default(string))
        {
            services.AddHttpClient<IConductorRestClient, CustomConductorRestClient>((provider, client) =>
            {
                client.BaseAddress = new Uri(serverUrl(provider));
            });

            services.AddSingleton<IWorkflowTaskCoordinator>(p => {
                return new WorkflowTaskCoordinator(p, p.GetService<ILogger<WorkflowTaskCoordinator>>(), concurrentWorkers);
            });

            services.AddTransient<ITaskClient, RestTaskClient>();

            services.AddTransient<IWorkflowTaskExecutor>(provider =>
            {
                return new WorkflowTaskExecutor(provider.GetService<ITaskClient>(),
                    provider,
                    provider.GetService<ILogger<WorkflowTaskExecutor>>(),
                    sleepInterval,
                    domain);
            });

            return services;
        }

        public static IServiceCollection AddConductorClient(this IServiceCollection services, Func<IServiceProvider, string> serverUrl)
        {
            services.AddHttpClient<IConductorRestClient, CustomConductorRestClient>((provider, client) =>
            {
                client.BaseAddress = new Uri(serverUrl(provider));
            });

            return services;
        }
    }
}
