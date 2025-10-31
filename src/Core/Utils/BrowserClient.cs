using System.Reflection;

namespace Lampman.Core.Utils;

public class BrowserClient : HttpClient
{
    public BrowserClient() : base()
    {
        DefaultConfig();
    }

    public BrowserClient(LoggingHandler log) : base(log)
    {
        DefaultConfig();
    }
    private void DefaultConfig()
    {
        DefaultRequestHeaders.Clear();
        DefaultRequestHeaders.UserAgent.ParseAdd(AppInfo.GetUserAgent());
        DefaultRequestHeaders.Accept.ParseAdd("*/*");
        DefaultRequestHeaders.Connection.Add("keep-alive");
    }

    public static BrowserClient CreateVerboseClient()
    {
        LoggingHandler log = new(new HttpClientHandler());

        return new BrowserClient(log);
    }
}