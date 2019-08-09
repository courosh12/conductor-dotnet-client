using ConductorDotnetClient.Swagger.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Interfaces
{
    public interface ITaskClient
    {
        Task<Swagger.Api.Task> PollTask(string taskType, string workerId, string domain);
        Task<string> UpdateTask(TaskResult result);
        Task<string> AckTask(string taskId, string workerid);
    }
}
