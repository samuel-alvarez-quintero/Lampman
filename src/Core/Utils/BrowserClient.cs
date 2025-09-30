namespace Lampman.Core.Utils;

public class BrowserClient : HttpClient
{
    public BrowserClient() : base()
    {
        DefaultRequestHeaders.Clear();
        DefaultRequestHeaders.UserAgent.ParseAdd("Lampman/0.1");
        DefaultRequestHeaders.Accept.ParseAdd("*/*");
        DefaultRequestHeaders.Connection.Add("keep-alive");
    }
}

public class VerboseBrowserClient : HttpClient
{
    public VerboseBrowserClient() : base(new LoggingHandler(new HttpClientHandler()))
    {
        DefaultRequestHeaders.Clear();
        DefaultRequestHeaders.UserAgent.ParseAdd("Lampman/0.1");
        DefaultRequestHeaders.Accept.ParseAdd("*/*");
        DefaultRequestHeaders.Connection.Add("keep-alive");
    }
}