using ConductorDotnetClient.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Worker
{
    public class WorkflowTaskCoordinator : IWorkflowTaskCoordinator
    {
        public int ConcurrentWorkers { get; set; } = 1;
        public List<IWorker> Workers { get; set; }

        public ITaskClient TaskClient { get; set; }
        
        public Task Start()
        {
            var pollers = new List<Task>();
            for (var i = 0; i < ConcurrentWorkers; i++)
            {
                pollers.Add(Task.Factory.StartNew(StartPoller));
            }

            return Task.Factory.StartNew(() => Task.WaitAll(pollers.ToArray()));
        }

    }
}
