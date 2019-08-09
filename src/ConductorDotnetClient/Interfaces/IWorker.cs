using ConductorDotnetClient.Swagger.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorDotnetClient.Interfaces
{
    public interface IWorker
    {
        string TaskType { get; set; }
        int Priority { get; set; }
        TaskResult Execute(Task task);
    }
}
    