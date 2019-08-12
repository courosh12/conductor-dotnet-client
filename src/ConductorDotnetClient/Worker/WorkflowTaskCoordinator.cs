using ConductorDotnetClient.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Worker
{
    public class WorkflowTaskCoordinator : IWorkflowTaskCoordinator
    {
        private int _concurrentWorkers;
        private IServiceProvider _serviceProvider;
        private ILogger<WorkflowTaskCoordinator> _logger;
        private HashSet<Type> _workerDefinitions = new HashSet<Type>();
        
        public WorkflowTaskCoordinator(IServiceProvider serviceProvider,
            ILogger<WorkflowTaskCoordinator> logger,
            int concurrentWorkers)
         {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _concurrentWorkers = concurrentWorkers;
        }

        public async Task Start()
        {
            _logger.LogInformation("Starting WorklfowCoordinator");

            var pollers = new List<Task>();
            for (var i = 0; i < _concurrentWorkers; i++)
            {
                var executor = _serviceProvider.GetService(typeof(IWorkflowTaskExecutor)) as IWorkflowTaskExecutor;
                pollers.Add(executor.StartPoller(_workerDefinitions.ToList()));
            }

            await Task.WhenAll(pollers);
        }

        public void RegisterWorker<T>() where T : IWorkflowTask
        {
            _workerDefinitions.Add(typeof(T));
        }
    }
}
