using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using DotNetEnv;
using Lampman.Core.Services;
using Lampman.Core.Models;
using System.Text.Json;

namespace Lampman.Tests.Fixtures;

public class MockRegistryFixture : IDisposable
{
  public HttpClient FakeClient { get; }
  private readonly WebApplication _app;
  public string TempDir { get; }
  public string[] Services { get; } = [];

  public MockRegistryFixture()
  {
    Env.TraversePath().Load();

    TempDir = Path.Combine(Path.GetTempPath(), "LampmanMockRegistry_" + Guid.NewGuid());
    Directory.CreateDirectory(TempDir);

    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();

    _app = builder.Build();

    var baseAddress = Environment.GetEnvironmentVariable("MOCK_REGISTRY_BASE_ADDRESS");
    baseAddress = Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute) ? baseAddress : "http://localhost/";

    string? servicesToManage = Environment.GetEnvironmentVariable("TESTING_SERVICES_TO_MANAGE");

    if (!string.IsNullOrEmpty(servicesToManage))
    {
      Services = servicesToManage.Split(';', StringSplitOptions.RemoveEmptyEntries);
    }

    // Registry endpoint
    _app.MapGet("/registry/{file}", async ctx =>
    {
      var file = ctx.Request.RouteValues["file"]?.ToString() ?? "main.json";

      Dictionary<string, Dictionary<string, ServiceSource>> response = [];

      foreach (var service in Services)
      {
        var (serviceName, version) = ServiceResolver.Parse(service);
        version ??= serviceName;

        if (!response.ContainsKey(serviceName))
          response[serviceName] = [];

        if (!response[serviceName].ContainsKey(version))
          response[serviceName][version] = new ServiceSource();

        response[serviceName][version].Url = $"{baseAddress}services/{serviceName}-{version}.zip";
        response[serviceName][version].Checksum = null;
        response[serviceName][version].ExtractTo = null;
        response[serviceName][version].ServiceProcess = null;
      }

      var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });

      ctx.Response.ContentType = "application/json";
      await ctx.Response.WriteAsync(jsonResponse);
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

    FakeClient = _app.GetTestClient();

    if (Uri.IsWellFormedUriString(baseAddress, UriKind.Absolute))
    {
      FakeClient.BaseAddress = new Uri(baseAddress);
    }
  }

  public void Dispose()
  {
    FakeClient.Dispose();
    // _app.Dispose(); // shuts down TestServer + frees resources

    if (Directory.Exists(TempDir))
      Directory.Delete(TempDir, recursive: true);
  }
}
