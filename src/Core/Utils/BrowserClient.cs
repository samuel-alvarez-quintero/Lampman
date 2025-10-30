using System.Reflection;

namespace Lampman.Core.Utils;

public class BrowserClient : HttpClient
{
    public BrowserClient() : base()
    {
        string? version = Assembly.GetExecutingAssembly()
                      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                      .InformationalVersion;

        DefaultRequestHeaders.Clear();
        DefaultRequestHeaders.UserAgent.ParseAdd($"Lampman/{version}");
        DefaultRequestHeaders.Accept.ParseAdd("*/*");
        DefaultRequestHeaders.Connection.Add("keep-alive");
    }
}

public class VerboseBrowserClient : HttpClient
{
    public VerboseBrowserClient() : base(new LoggingHandler(new HttpClientHandler()))
    {
        string? version = Assembly.GetExecutingAssembly()
                      .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                      .InformationalVersion;

        DefaultRequestHeaders.Clear();
        DefaultRequestHeaders.UserAgent.ParseAdd($"Lampman/{version}");
        DefaultRequestHeaders.Accept.ParseAdd("*/*");
        DefaultRequestHeaders.Connection.Add("keep-alive");
    }
}