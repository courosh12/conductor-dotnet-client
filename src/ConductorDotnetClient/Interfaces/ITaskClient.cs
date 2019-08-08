using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorDotnetClient.Interfaces
{
    public interface ITaskClient
    {
        void PollTask(string taskType, string workerId, string domain); 
    }
}
