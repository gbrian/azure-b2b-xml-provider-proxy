using System.Net;
using System.Net.Http.Formatting;
using System.Xml;
using System.Linq;
using Newtonsoft.Json;

// http://stackoverflow.com/questions/2973208/automatically-decompress-gzip-response-via-webclient-downloaddata
// Sure this should be available on
class MyWebClient : WebClient
{
    protected override WebRequest GetWebRequest(Uri address)
    {
        HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
        request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        return request;
    }
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");

    // Get destiniation url
    var url = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "url", true) == 0)
        .Value;
    // Try headres
    if (url == null)
        url = req.Headers.GetValues("url").FirstOrDefault();
    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    var wc = new MyWebClient();
    var xml = wc.DownloadString(url);
    // if(url != null)
    var doc = new XmlDocument();
    doc.LoadXml(xml);
    string json = JsonConvert.SerializeXmlNode(doc);

    // Return JSON format
    var code = HttpStatusCode.OK;
    HttpResponseMessage res = req.CreateResponse(code, json, JsonMediaTypeFormatter.DefaultMediaType);
    // Set headers
    foreach(string key in wc.ResponseHeaders.Keys)
        res.Headers.Add("Src-"+ key, string.Join(",", wc.ResponseHeaders.GetValues(key)));
    res.Headers.Add("xml-length", xml.Length.ToString());
    res.Headers.Add("json-length", json.Length.ToString());
    return res;
}