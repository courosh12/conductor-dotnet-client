
# conductor-dotnet-client

The rest api client is based on the swagger.json file. The client is generated with NSwag and the .nswag config file is also available.

## Usage

        services.AddTransient<IConductorRestClient>(
                serviceProvider =>
                {
                    var httpClient= new HttpClient();
                    return new ConductorRestClient("http://localhost:8080/api", httpClient);
                }
            );
## TODO
Working on a WorkerHost based on the RestClient

