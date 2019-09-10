﻿using ConductorDotnetClient.Enum;
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
        private PollPriority _priority;
        private readonly string _domain;
        private ITaskClient _taskClient;
        private IServiceProvider _serviceProvider;
        private string _workerId = Guid.NewGuid().ToString();

        public WorkflowTaskExecutor(ITaskClient taskClient,
            IServiceProvider serviceProvider,
            ILogger<WorkflowTaskExecutor> logger,
            int sleepInterval,
            string domain,
            PollPriority priority = PollPriority.RANDOM)
        {
            _taskClient = taskClient;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _sleepInterval = sleepInterval;
            _priority = priority;
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
            foreach (var taskType in workersToBePolled)
            {
                //var taskObj=(IWorkflowTask)Activator.CreateInstance(taskType);
                var worklfowTask = _serviceProvider.GetService(taskType) as IWorkflowTask;
                if (worklfowTask is null)
                    throw new WorkerNotFoundException(taskType.GetType().Name);

                _logger.LogTrace(GetWorkerName() + $"Polling for task type: {worklfowTask.TaskType}");

                var task = await PollForTask(worklfowTask.TaskType);

                if (task != null)
                {
                    await ProcessTask(task, worklfowTask);
                    break;
                }
            }
            _logger.LogInformation(GetWorkerName() + "Pollcyclus ended");
            await Task.Delay(_sleepInterval);
        }

        private List<Type> DeterminOrderOfPolling(List<Type> workers)
        {
            //TODO implement real logic
            return workers;
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
    }
}
