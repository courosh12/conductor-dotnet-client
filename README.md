


# conductor-dotnet-client

The rest api client is based on the swagger.json file. The client is generated with NSwag and the .nswag config file is also available.

## Usage
Register the client in you di with the following method:
 

    services.AddConductorClient( service => "http://localhost:8080/api/");


To use the generated rest api ask for the IConductorRestClient interface.

## Worker
When configuring the client u have the option to set the amount of workes and the polling interval

	services.AddConductorClient(service => "http://localhost:8080/api/", 1, 1000);

This will start x workes who will poll every x second for new tasks.

Your worker has to implement the IWorkflowTask interface. 

       class SampleWorker : IWorkflowTask
       {
           public string TaskType { get; set; } = "test_task"; 
    
            public int Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    
            public TaskResult Execute(Task task)
            {
                Console.WriteLine("Doing some work");
                return task.Completed();
                //return task.Completed(new Dictionary<string, object>() { }); with ouputdata
                //return task.Failed("error message ")//error
            }
        }

And also be regsiterd with the workerclient:

    var workflowTaskCoordinator= serviceProvider.GetRequiredService<IWorkflowTaskCoordinator>();
    workflowTaskCoordinator.RegisterWorker<SampleWorker>();

Also make sure to register your worker with the di client as it will be resloved there at runtime.

After that u can start the client:

	await workflowTaskCoordinator.Start();

Make sure to await it as it is an never ending task.




## Package

    Install-Package ConductorDotnetClient

## TODO

 - shutdown
 - priority polling
 - implement response timeout ping

