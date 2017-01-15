using System.Net;
using System.Net.Http.Formatting;
using System.Xml;
using System.Linq;
using Newtonsoft.Json;
using System.Text;

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

class MetricsManager
{
    public Dictionary<string, string> Metrics = new Dictionary<string, string>();

    public void TimeTaken(string key, Action val)
    {
        key = "fn" + key;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        val.Invoke();
        Metrics.Add(key, sw.Elapsed.TotalMilliseconds.ToString());
    }
}

public static Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    return Task.Run(() =>
    {
        try
        {
            return InternalRun(req, log);
        }
        catch (Exception ex)
        {
            log.Error(ex.ToString());
            var json = new { error = ex.ToString() };
            var code = HttpStatusCode.OK;
            return req.CreateResponse(code, json, JsonMediaTypeFormatter.DefaultMediaType);
        }
    });    
}
static HttpResponseMessage InternalRun(HttpRequestMessage req, TraceWriter log)
{
    var metrics = new MetricsManager();
    MyWebClient wc = new MyWebClient();
    string url = null;
    string xml = null;
    XmlDocument doc = null;
    string json = null;
    bool areEqual = false;
    HttpResponseMessage res = null;

    log.Info("C# HTTP trigger function processed a request.");

    metrics.TimeTaken("GetUrl", () => GetUrl(req, out url));

    metrics.TimeTaken("DownloadXml", () => DownloadXml(url, wc, out xml));

    metrics.TimeTaken("ConvertToJson", () => ConvertToJson(xml, out doc, out json));

    // JSON to XML to check data consistency
    metrics.TimeTaken("CheckDataConsistency", () => CheckDataConsistency(doc, json, out areEqual));

    // Return JSON format
    metrics.TimeTaken("CreateResponse", () => CreateResponse(req, json, out res));

    // Set headers
    metrics.TimeTaken("SetMetrics", () => SetMetrics(wc, xml, json, areEqual, res, metrics));
    return res;
}

static void CreateResponse(HttpRequestMessage req, string json, out HttpResponseMessage res)
{
    var code = HttpStatusCode.OK;
    res = req.CreateResponse(code, json, JsonMediaTypeFormatter.DefaultMediaType);
}

static void SetMetrics(MyWebClient wc, string xml, string json, bool areEqual, HttpResponseMessage res, MetricsManager metrics)
{
    foreach (string key in wc.ResponseHeaders.Keys)
        res.Headers.Add("Src-" + key, string.Join(",", wc.ResponseHeaders.GetValues(key)));
    // Get compressed size
    var xmlZipSize = Zip(xml).Length;
    var jsonZipSize = Zip(json).Length;

    res.Headers.Add("xml-length", string.Format("flat {0} compressed {1}",
                                                xml.Length,
                                                xmlZipSize.ToString()));
    res.Headers.Add("json-length", string.Format("flat {0} compressed {1} vs xml {2} vs xml compressed {3}",
                                    json.Length,
                                    jsonZipSize,
                                    100 - ((100.0 / (double)xml.Length) * (double)json.Length),
                                    100 - ((100.0 / (double)xmlZipSize) * (double)jsonZipSize)));
    res.Headers.Add("json-xml-equal", areEqual.ToString());

    foreach (var kv in metrics.Metrics)
        res.Headers.Add(kv.Key, kv.Value);
}

static void CheckDataConsistency(XmlDocument doc, string json, out bool areEqual)
{
    XmlDocument deserializedDoc = JsonConvert.DeserializeXmlNode(json);
    areEqual = doc.OuterXml == deserializedDoc.OuterXml;
}

static void ConvertToJson(string xml, out XmlDocument doc, out string json)
{
    doc = new XmlDocument();
    doc.LoadXml(xml);
    json = JsonConvert.SerializeXmlNode(doc);
}

static void DownloadXml(string url, MyWebClient wc, out string xml)
{
    xml = wc.DownloadString(url);
}

static void GetUrl(HttpRequestMessage req, out string url)
{
    // Get destiniation url
    url = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "url", true) == 0)
        .Value;
    // Try headres
    if (url == null)
        url = req.Headers.GetValues("url").FirstOrDefault();
}

static byte[] Zip(string str)
{
    var bytes = Encoding.UTF8.GetBytes(str);

    using (var msi = new MemoryStream(bytes))
    using (var mso = new MemoryStream())
    {
        using (var gs = new System.IO.Compression.GZipStream(mso, System.IO.Compression.CompressionMode.Compress))
        {
            msi.CopyTo(gs);
            //System.IO.Stream.CopyTo(msi, gs);
        }

        return mso.ToArray();
    }
}
