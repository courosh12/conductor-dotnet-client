using ConductorDotnetClient.Swagger.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConductorDotnetClient.Extensions
{
    public static class TaskExtensions
    {
        public static TaskResult Completed(this Task task, IDictionary<string, object> outputData = null, ICollection<TaskExecLog> logs = null)
        {
            return new TaskResult
            {
                WorkflowInstanceId = task.WorkflowInstanceId,
                TaskId = task.TaskId,
                Status = TaskResultStatus.COMPLETED,
                OutputData = outputData,
                Logs = logs
            };
        }

        public static TaskResult Failed(this Task task, string errorMessage, IDictionary<string, object> outputData = null, ICollection<TaskExecLog> logs = null)
        {
            return new TaskResult
            {
                WorkflowInstanceId = task.WorkflowInstanceId,
                TaskId = task.TaskId,
                Status = TaskResultStatus.FAILED,
                ReasonForIncompletion = errorMessage,
                OutputData = outputData,
                Logs = logs
            };
        }

        public static TaskResult FailedWithTerminalError(this Task task, string errorMessage, IDictionary<string, object> outputData = null, ICollection<TaskExecLog> logs = null)
        {
            return new TaskResult
            {
                WorkflowInstanceId = task.WorkflowInstanceId,
                TaskId = task.TaskId,
                Status = TaskResultStatus.FAILED_WITH_TERMINAL_ERROR,
                ReasonForIncompletion = errorMessage,
                OutputData = outputData,
                Logs = logs
            };
        }
    }
}
