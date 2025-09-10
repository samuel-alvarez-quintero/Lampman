using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using DotNetEnv;

namespace Lampman.Tests.Fixtures;

public class MockRegistryFixture : IDisposable
{
  public HttpClient Client { get; }
  // private readonly IHost _host;
  private readonly WebApplication _app;
  public string TempDir { get; }

  public MockRegistryFixture()
  {
    Env.TraversePath().Load();

    string? BaseAddress = Environment.GetEnvironmentVariable("MOCK_REGISTRY_BASE_ADDRESS");

    TempDir = Path.Combine(Path.GetTempPath(), "LampmanMockRegistry_" + Guid.NewGuid());
    Directory.CreateDirectory(TempDir);

    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();

    _app = builder.Build();

    // Registry endpoint
    _app.MapGet("/registry/main.json", async ctx =>
    {
      string Url = "/services/php-8.2.29.zip";

      if (null != BaseAddress && Uri.IsWellFormedUriString(BaseAddress, UriKind.Absolute))
      {
        Url = $"{BaseAddress}services/php-8.2.29.zip";
      }

      var json = @"{
              ""php"": {
                ""php-8.2.29-win32-vs16-x64"": {
                  ""Url"": """ + Url + @""",
                  ""ExtractTo"": null,
                  ""Checksum"": null
                }
              }
            }";

      ctx.Response.ContentType = "application/json";
      await ctx.Response.WriteAsync(json);
    });

    // Services endpoint
    _app.MapGet("/services/{file}", async ctx =>
    {
      var file = ctx.Request.RouteValues["file"]?.ToString() ?? "default.zip";
      var zipPath = Path.Combine(TempDir, file);

      if (!File.Exists(zipPath))
      {
        using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write);
        using var zip = new ZipArchive(fs, ZipArchiveMode.Create, true);
        var entry = zip.CreateEntry("fake-executable.exe");
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream);
        await writer.WriteAsync("This is a fake " + file);
      }

      ctx.Response.ContentType = "application/zip";
      await ctx.Response.SendFileAsync(zipPath);
    });

    _app.Start();
    Client = _app.GetTestClient();

    if (null != BaseAddress && Uri.IsWellFormedUriString(BaseAddress, UriKind.Absolute))
    {
      Client.BaseAddress = new Uri(BaseAddress);
    }

  }

  public void Dispose()
  {
    Client.Dispose();
    // _app.Dispose(); // shuts down TestServer + frees resources

    if (Directory.Exists(TempDir))
      Directory.Delete(TempDir, recursive: true);
  }
}
