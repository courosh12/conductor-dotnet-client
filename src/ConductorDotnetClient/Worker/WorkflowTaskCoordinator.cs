using ConductorDotnetClient.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Worker
{
    public class WorkflowTaskCoordinator : IWorkflowTaskCoordinator
    {
        private int _concurrentWorkers;
        private IServiceProvider _serviceProvider;
        private ILogger<WorkflowTaskCoordinator> _logger;
        
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
                var executor = _serviceProvider.GetService(typeof(IWorkflowTaskExecutor)) as IWorkflowTaskExecutor;//how does this work instance per loop??
                pollers.Add(executor.StartPoller());
            }

            await Task.WhenAll(pollers);
        }
    }
}
