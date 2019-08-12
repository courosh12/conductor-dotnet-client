using ConductorDotnetClient.Swagger.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorDotnetClient.Extensions
{
    public static class TaskExtension
    {
        public static TaskResult Completed(this Task task,IDictionary<string,object> outputData=null)
        {
            return new TaskResult
            {
                WorkflowInstanceId = task.WorkflowInstanceId,
                TaskId = task.TaskId,
                Status = TaskResultStatus.COMPLETED,
                OutputData=outputData
            };
        }

        public static TaskResult Failed(this Task task, string errorMessage, IDictionary<string, object> outputData = null)
        {
            return new TaskResult
            {
                WorkflowInstanceId = task.WorkflowInstanceId,
                TaskId = task.TaskId,
                Status = TaskResultStatus.FAILED,
                ReasonForIncompletion=errorMessage,
                OutputData=outputData
            };
        }
    }
}
