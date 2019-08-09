using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Interfaces
{
    public interface ITaskClient
    {
        Task<Swagger.Api.Task> PollTask(string taskType, string workerId, string domain); 
    }
}
