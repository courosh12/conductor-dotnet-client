using ConductorDotnetClient.Enum;
using ConductorDotnetClient.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Worker
{
    public class WorkflowTaskExecutor : IWorkflowTaskExecutor
    {
        private List<IWorker> _workers;
        private ILogger<WorkflowTaskExecutor> _logger;
        private int _sleepInterval;
        private PollPriority _priority;
        private ITaskClient _taskClient;
        private IServiceProvider _serviceProvider;
        private string _workerId = "worker1";//TODO make this read from machine

        public WorkflowTaskExecutor(List<IWorker> workers,
            ITaskClient taskClient,
            IServiceProvider serviceProvider,
            ILogger<WorkflowTaskExecutor> logger,
            int sleepInterval,
            PollPriority priority = PollPriority.RANDOM)
        {
            _workers = workers;
            _taskClient = taskClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _sleepInterval = sleepInterval;
            _priority = priority;
        }

        public async Task StartPoller()
        {
            _logger.LogInformation("Starting poller");
            if (_workers is null || _workers.Count==0) throw new NullReferenceException("Workers not set");

            while (true)
            {
                await PollCyclus();
            }
        }

        private async Task PollCyclus()
        {
            var workersToBePolled = DeterminOrderOfPolling(_workers);
            foreach (var taskType in workersToBePolled)
            {
                var task = await PollForTask(taskType.TaskType);

                if (task != null)
                {
                    ProcessTask(task);
                    break;
                }
            }

            await Task.Delay(_sleepInterval);
        }

        private List<IWorker> DeterminOrderOfPolling(ICollection<IWorker> workers)
        {
            //TODO implement real logic
            return (List<IWorker>)workers;
        }


        public Task<Swagger.Api.Task> PollForTask(string taskType)
        {
            return _taskClient.PollTask(taskType, _workerId,null);
        }

        private void ProcessTask(Swagger.Api.Task task)
        {
            _logger.LogInformation($"Processing task:{task.TaskDefName}");

            var workerDef= _workers.Where(p => p.TaskType == task.TaskType).Single();
            var worker =_serviceProvider.GetService(workerDef.GetType());
        }

    }
}
