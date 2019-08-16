using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Swagger.Api;
using System;
using ConductorDotnetClient.Extensions;
using System.Threading.Tasks;
using ConductorTask = ConductorDotnetClient.Swagger.Api.Task;
using Task = System.Threading.Tasks.Task;

namespace ConductorDotnetClient.Demo
{
    public class SampleWorkerTask : IWorkflowTask
    {
        public string TaskType { get; } = "test_task"; 

        public int Priority { get => throw new NotImplementedException(); }

        public Task<TaskResult> Execute(ConductorTask task)
        {
            Console.WriteLine("Doing some work");
            return Task.FromResult(task.Completed());
            //return Task.FromResult(task.Completed(new Dictionary<string, object>() { })); // with ouputdata
            //return Task.FromResult(task.Failed("error message ")); //error
            //return Task.FromResult(task.FailedWithTerminalError("error message")); // terminal failure
        }
    }
}
