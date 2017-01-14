#r "Newtonsoft.Json"
#r "System.Configuration"

using System;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    string jsonContent = await req.Content.ReadAsStringAsync();
    dynamic data = JsonConvert.DeserializeObject(jsonContent);

    return req.CreateResponse(HttpStatusCode.OK, new {
        greeting = "Hello, we received your data",
		data 
    });
}