using ConductorDotnetClient.Exceptions;
using ConductorDotnetClient.Extensions;
using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Swagger.Api;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ConductorDotnetClient.Worker
{
    internal class WorkflowTaskExecutor : IWorkflowTaskExecutor
    {
        private List<Type> _workers;
        private ILogger<WorkflowTaskExecutor> _logger;
        private int _sleepInterval;
        private readonly string _domain;
        private ITaskClient _taskClient;
        private IServiceProvider _serviceProvider;
        private string _workerId = Guid.NewGuid().ToString();

        public WorkflowTaskExecutor(ITaskClient taskClient,
            IServiceProvider serviceProvider,
            ILogger<WorkflowTaskExecutor> logger,
            int sleepInterval,
            string domain)
        {
            _taskClient = taskClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _sleepInterval = sleepInterval;
            _domain = domain;
        }

        private string GetWorkerName()
        {
            return $"Worker : {_workerId} ";
        }

        public async Task StartPoller(List<Type> workers)
        {
            _workers = workers;
            _logger.LogInformation(GetWorkerName() + "Starting poller");
            if (_workers is null || _workers.Count == 0) throw new NullReferenceException("Workers not set");

            while (true)
            {
                await PollCyclus();
            }
        }

        private async Task PollCyclus()
        {
            _logger.LogInformation(GetWorkerName() + "Pollcyclus started");
            var workersToBePolled = DeterminOrderOfPolling(_workers);
            foreach (var workerToBePolled in workersToBePolled)
            {
                _logger.LogTrace(GetWorkerName() + $"Polling for task type: {workerToBePolled.TaskType}");

                var task = await PollForTask(workerToBePolled.TaskType);

                if (task != null)
                {
                    await ProcessTask(task, workerToBePolled);
                    break;
                }
            }
            _logger.LogInformation(GetWorkerName() + "Pollcyclus ended");
            await Task.Delay(_sleepInterval);
        }

        private List<IWorkflowTask> DeterminOrderOfPolling(List<Type> workersToBePolled)
        {
            var workflowTasks = new List<IWorkflowTask>();

            foreach (var taskType in workersToBePolled)
            {
                var worklfowTask = _serviceProvider.GetService(taskType) as IWorkflowTask;
                if (worklfowTask is null)
                    throw new WorkerNotFoundException(taskType.GetType().Name);
                workflowTasks.Add(worklfowTask);
            }

            var prio =  workflowTasks.Where(p => p.Priority != null).OrderByDescending(p => p.Priority).ToList();
            var random = workflowTasks.Where(p => p.Priority == null).ToList();
            ShuffleList(random);
            return prio.Concat(random).ToList();
        }

        public Task<Swagger.Api.Task> PollForTask(string taskType)
        {
            return _taskClient.PollTask(taskType, _workerId, _domain);
        }

        private async Task ProcessTask(Swagger.Api.Task task, IWorkflowTask workflowTask)
        {
            _logger.LogInformation(GetWorkerName() + $"Processing task:{task.TaskDefName} id:{task.TaskId}");

            //TODO: we need to think about this try-catch

            try
            {
                await AckTask(task);
                var result = await workflowTask.Execute(task);
                result.WorkerId = _workerId;
                await UpdateTask(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, GetWorkerName() + "Failed to execute task");
                await UpdateTask(task.FailedWithTerminalError(e.ToString()));
            }
        }

        private async Task UpdateTask(TaskResult taskResult)
        {
            var result = await _taskClient.UpdateTask(taskResult);
            _logger.LogInformation(GetWorkerName() + $"Update respone {result}");
        }

        private async Task AckTask(Swagger.Api.Task task)
        {
            var result = await _taskClient.AckTask(task.TaskId, task.WorkerId);
            _logger.LogInformation(GetWorkerName() + $"Update respone {result}");
        }

        private void ShuffleList<T>(List<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
