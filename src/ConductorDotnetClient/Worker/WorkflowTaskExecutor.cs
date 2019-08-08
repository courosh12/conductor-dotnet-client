using ConductorDotnetClient.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorDotnetClient.Worker
{
    public class WorkflowTaskExecutor
    {
        public List<IWorker> Workers { get; set; }

        public void StartPoller()
        {
            //some checks //cancelation token
            while (true)
            {
                PollCyclus();
            }
        }

        private void PollCyclus()
        {
            foreach (var taskType in Workers)
            {
                var task = PollForTask(taskType.TaskType);

                if (task != null)
                {
                    //execute task
                    ProcessTask(task);
                    break;
                }
            }
        }

        public ConductorTask PollForTask(string taskType)
        {
            throw new NotImplementedException();
        }

        private void ProcessTask(object task)
        {

        }

    }
}
