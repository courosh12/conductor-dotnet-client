using ConductorDotnetClient.Swagger.Api;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Interfaces
{
    public interface IWorkflowTask
    {
        string TaskType { get; set; }
        int Priority { get; set; }
        Task<TaskResult> Execute(Swagger.Api.Task task);
    }
}
    