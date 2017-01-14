using System.Net;
using System.Net.Http.Formatting;

// Azure functions will lkeep static data in memory
public class StaticData{
	public static int Counter = 0;
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    name = name ?? data?.name;
	var json = new {
		message = name == null
				? "Please pass a name on the query string or in the request body"
				: "Hello " + name + (StaticData.Counter++).ToString()
	};
	var code = name == null ? HttpStatusCode.BadRequest: HttpStatusCode.OK;
	// Return JSON format
    return req.CreateResponse(code, json, JsonMediaTypeFormatter.DefaultMediaType);
}