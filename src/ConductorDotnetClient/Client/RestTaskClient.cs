using ConductorDotnetClient.Interfaces;
using ConductorDotnetClient.Swagger.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Client
{
    public class RestTaskClient : ITaskClient
    {
        private IConductorRestClient _restClient;

        public RestTaskClient(IConductorRestClient restClient)
        {
            _restClient = restClient;
        }

        public Task<Swagger.Api.Task> PollTask(string taskType, string workerId, string domain)
        {
            return _restClient.PollAsync(taskType, workerId, domain);
        }

        public Task<string> UpdateTask(TaskResult result)
        {
            return _restClient.UpdateTaskAsync(result);
        }

        public Task<string> AckTask(string taskId, string workerid)
        {
            return _restClient.AckAsync(taskId, workerid);
        }
    }
}
