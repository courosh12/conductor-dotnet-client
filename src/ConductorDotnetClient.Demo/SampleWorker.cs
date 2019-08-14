using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Swagger.Api;
using System;
using System.Collections.Generic;
using System.Text;
using ConductorDotnetClient.Extensions;

namespace demo
{
    class SampleWorker : IWorkflowTask
    {
        public string TaskType { get; set; } = "test_task"; 

        public int Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TaskResult Execute(Task task)
        {
            Console.WriteLine("Doing some work");
            return task.Completed();
            //return task.Completed(new Dictionary<string, object>() { }); with ouputdata
            //return task.Failed("error message ")//error
        }
    }
}
