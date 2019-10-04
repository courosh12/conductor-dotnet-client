using ConductorDotnetClient.Client;
using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Swagger.Api;
using ConductorDotnetClient.Worker;
using Microsoft.Extensions.DependencyInjection;
using System;

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

        public static IServiceCollection AddConductorWorker(this IServiceCollection services, ConductorClientSettings conductorClientSettings)
        {
            services.AddSingleton<ConductorClientSettings>(conductorClientSettings);

            services.AddHttpClient<IConductorRestClient, CustomConductorRestClient>((provider, client) =>
            {
                client.BaseAddress = conductorClientSettings.ServerUrl;
            });

            services.AddSingleton<IWorkflowTaskCoordinator, WorkflowTaskCoordinator>();
            services.AddTransient<ITaskClient, RestTaskClient>();
            services.AddTransient<IWorkflowTaskExecutor, WorkflowTaskExecutor>();

            return services;
        }

        public static IServiceCollection AddConductorWorker(this IServiceCollection services, 
            Func<IServiceProvider, string> serverUrl, 
            int concurrentWorkers = 1, 
            int sleepInterval = 1000,
            string domain = default(string))
        {
            var conductorClientSettings = new ConductorClientSettings
            {
                Domain = domain,
                IntervalStrategy = ConductorClientSettings.IntervalStrategyType.Exponential,
                SleepInterval = sleepInterval,
                ConcurrentWorkers = concurrentWorkers,
                ServerUrl = new Uri(serverUrl(services.BuildServiceProvider()))
            };

            return services.AddConductorWorker(conductorClientSettings);
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
