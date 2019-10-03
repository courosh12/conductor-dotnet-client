using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Swagger.Api;
using System;
using ConductorDotnetClient.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConductorTask = ConductorDotnetClient.Swagger.Api.Task;
using Task = System.Threading.Tasks.Task;

namespace ConductorDotnetClient.Demo
{
    public class SampleWorkerTaskTheSecond : IWorkflowTask
    {
        public string TaskType { get; } = "test_task_2";

        public int? Priority { get; } 

        public Task<TaskResult> Execute(ConductorTask task)
        {
            Console.WriteLine("Doing some work");
            return Task.FromResult(task.Completed());
        }
    }
}
