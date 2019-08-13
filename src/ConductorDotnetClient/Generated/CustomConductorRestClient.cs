using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConductorDotnetClient.Swagger.Api
{
    public class CustomConductorRestClient : ConductorRestClient
    {
        public CustomConductorRestClient(string baseUrl, System.Net.Http.HttpClient httpClient) : base(baseUrl, httpClient) { }

        protected override async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(System.Net.Http.HttpResponseMessage response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }

            // TODO: This should most likely be fixed in NSwag. For now we fix it this way 
            // If the response Content-Type header is 'text/plain' there is no need to JSON deserialize this response
            // Cast it to T and return it
            if (headers.ContainsKey("Content-Type") && headers["Content-Type"].Any(s => s == "text/plain"))
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                try
                {
                    var typedBody = (T)Convert.ChangeType(responseText, typeof(T));
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (InvalidCastException exception)
                {
                    var message = "Could not cast the response body string as " + typeof(T).FullName + ".";
                    throw new ConductorRestException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }

            // Otherwise just follow normal flow
            return await base.ReadObjectResponseAsync<T>(response, headers);
        }
    }
}
