using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Swagger.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace demo
{
    class SampleWorker : IWorker
    {
        public string TaskType { get; set; } = "test_task"; 

        public int Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TaskResult Execute(Task task)
        {
            Console.WriteLine("Doing some work");
            return new TaskResult();
        }
    }
}
