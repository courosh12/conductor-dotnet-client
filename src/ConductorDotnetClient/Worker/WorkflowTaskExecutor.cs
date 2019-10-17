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
        private readonly ConductorClientSettings _conductorClientSettings;
        private readonly ITaskClient _taskClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _workerId = Guid.NewGuid().ToString();

        private int _sleepMultiplier = 1;        

        public WorkflowTaskExecutor(ITaskClient taskClient,
            IServiceProvider serviceProvider,
            ILogger<WorkflowTaskExecutor> logger,
            ConductorClientSettings conductorClientSettings)
        {
            _taskClient = taskClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _conductorClientSettings = conductorClientSettings;
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
                    _sleepMultiplier = 0;
                    break;
                }
            }
            _logger.LogInformation(GetWorkerName() + "Pollcyclus ended");
            await Sleep();
        }

        private async Task Sleep()
        {
            var delay = _conductorClientSettings.SleepInterval;

            switch (_conductorClientSettings.IntervalStrategy)
            {
                case ConductorClientSettings.IntervalStrategyType.Linear:
                    delay = Math.Min(_conductorClientSettings.MaxSleepInterval, _conductorClientSettings.SleepInterval * _sleepMultiplier++);
                    break;
                case ConductorClientSettings.IntervalStrategyType.Exponential:
                    var multiplier = (int)Math.Pow(2, _sleepMultiplier++);
                    delay = Math.Min(_conductorClientSettings.MaxSleepInterval, _conductorClientSettings.SleepInterval * multiplier);
                    break;
            }            

            _logger.LogDebug($"Waiting for {delay}ms");

            await Task.Delay(delay);
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
            return _taskClient.PollTask(taskType, _workerId, _conductorClientSettings.Domain);
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
