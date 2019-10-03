using ConductorDotnetClient.Swagger.Api;
using System.Threading.Tasks;

namespace ConductorDotnetClient.Interfaces
{
    public interface IWorkflowTask
    {
        string TaskType { get; }
        int? Priority { get; }
        Task<TaskResult> Execute(Swagger.Api.Task task);
    }
}
    