


# conductor-dotnet-client

This packages provides both an abstration for the [Conductor](https://github.com/Netflix/conductor) REST API and a way to start a worker that polls for certain tasks.

The REST API client is based on the swagger.json file as provided by Conductor. The client is generated with NSwag and the nswag.json config and swagger data is provided in the repo.

## Client usage
Register the client in your DI with the following method:
 
```csharp
services.AddConductorClient( service => "http://localhost:8080/api/");
```

To use the generated REST API ask for the IConductorRestClient interface.

```csharp
public Sample
{
    public Sample(IConductorRestClient conductorRestClient)
    {
        var workflowInstanceId = conductorRestClient.StartWorkflowAsync(startWorkflowRequest).GetAwaiter().GetResult()
    }
}
```

## Worker usage

To use the worker register the client and worker in your DI with the following method, you have the option to set the amount of workers, polling interval and domain.

```csharp
services.AddConductorWorker(service => "http://localhost:8080/api/", 1, 1000, "SampleDomain");
```

This will start __x__ workes who will poll every __y__ second for new tasks.

Your worker has to implement the IWorkflowTask interface; 

```csharp
public class SampleWorker : IWorkflowTask
{
    public string TaskType { get; } = "test_task"; 

    public int Priority { get => throw new NotImplementedException(); }

    public Task<TaskResult> Execute(ConductorTask task)
    {
        Console.WriteLine("Doing some work");
        return Task.FromResult(task.Completed());
        //return Task.FromResult(task.Completed(new Dictionary<string, object>() { })); // with ouputdata
        //return Task.FromResult(task.Failed("error message ")); //error
        //return Task.FromResult(task.FailedWithTerminalError("error message")); // terminal failure
    }
}
```

be registerd in the DI;

```csharp
services.AddConductorWorkflowTask<SampleWorkerTask>();
```

and also be regsiterd with the worker:

```csharp
var workflowTaskCoordinator = serviceProvider.GetRequiredService<IWorkflowTaskCoordinator>();
foreach(var worker in serviceProvider.GetServices<IWorkflowTask>())
{
    workflowTaskCoordinator.RegisterWorker(worker);
}
```

After that you can start the worker:

```csharp
await workflowTaskCoordinator.Start();
```

Make sure to await it as it is an never ending task.

## Installation

```ps
Install-Package ConductorDotnetClient
```

## TODO

 - Shutdown
 - Priority polling
 - Implement response timeout ping