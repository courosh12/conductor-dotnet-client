using ConductorDotnetClient.Enum;
using ConductorDotnetClient.Exceptions;
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
        private List<Type> _workers;
        private ILogger<WorkflowTaskExecutor> _logger;
        private int _sleepInterval;
        private PollPriority _priority;
        private ITaskClient _taskClient;
        private IServiceProvider _serviceProvider;
        private string _workerId = "worker1";//TODO make this read from machine

        public WorkflowTaskExecutor(ITaskClient taskClient,
            IServiceProvider serviceProvider,
            ILogger<WorkflowTaskExecutor> logger,
            int sleepInterval,
            PollPriority priority = PollPriority.RANDOM)
        {
            _taskClient = taskClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _sleepInterval = sleepInterval;
            _priority = priority;
        }

        public async Task StartPoller(List<Type> workers)
        {
            _workers = workers;
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
                var taskObj=(IWorker)Activator.CreateInstance(taskType);
                var task = await PollForTask(taskObj.TaskType);

                if (task != null)
                {
                    ProcessTask(task,taskObj);
                    break;
                }
            }

            await Task.Delay(_sleepInterval);
        }

        private List<Type> DeterminOrderOfPolling(List<Type> workers)
        {
            //TODO implement real logic
            return workers;
        }


        public Task<Swagger.Api.Task> PollForTask(string taskType)
        {
            return _taskClient.PollTask(taskType, _workerId,null);
        }

        private void ProcessTask(Swagger.Api.Task task,IWorker taskType)
        {
            _logger.LogInformation($"Processing task:{task.TaskDefName}");

            var worker =_serviceProvider.GetService(taskType.GetType());

            if (worker is null)
                throw new WorkerNotFoundException(taskType.GetType().Name);

            try
            {
                var result = ((IWorker)worker).Execute(task);
            }
            catch(Exception e)
            {
                //TODO ERROR HANDLING
            }
        }

    }
}
