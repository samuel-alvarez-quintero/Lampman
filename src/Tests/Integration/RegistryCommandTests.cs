using System.Text.Json;

using DotNetEnv;

using Lampman.Cli;
using Lampman.Core;
using Lampman.Tests.Fixtures;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Integration;

[Trait("Category", "Integration"), Trait("Category", "RegistryCommand"), TestCaseOrderer(typeof(PriorityOrderer))]
public class RegistryCommandTests : IClassFixture<MockRegistryFixture>
{
    private readonly string[]? _registryUrls;

    private readonly MockRegistryFixture _fixture;

    private readonly LampmanApp _app;

    private readonly StringWriter _testingOutputWriter;

    public RegistryCommandTests(MockRegistryFixture fixture)
    {
        Env.TraversePath().Load();

        _fixture = fixture;

        _app = new LampmanApp(_fixture.FakeClient);

        string? registrySources = Environment.GetEnvironmentVariable("TESTING_REGISTRY_SOURCES");

        if (!string.IsNullOrEmpty(registrySources))
        {
            _registryUrls = registrySources.Split(';', StringSplitOptions.RemoveEmptyEntries);
            _registryUrls = [.. _registryUrls.Select(_url => _url.Trim())];
        }

        _testingOutputWriter = new();
        Console.SetOut(_testingOutputWriter);
    }

    /** Test the 'lampman registry -h' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryHelp"), TestPriority(300)]
    public async Task RegistryHelp_ShouldDisplayHelpInformation()
    {
        var exitCode = await _app.RunAsync(["registry", "-h"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Description", _testingOutputWriter.ToString());
        Assert.Contains("Commands", _testingOutputWriter.ToString());
        Assert.Contains("list", _testingOutputWriter.ToString());
        Assert.Contains("add", _testingOutputWriter.ToString());
        Assert.Contains("remove", _testingOutputWriter.ToString());
        Assert.Contains("update", _testingOutputWriter.ToString());
    }

    /** Test the 'lampman registry list' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryList"), TestPriority(301)]
    public async Task RegistryList_ShouldReturnRegistryEntries()
    {
        var exitCode = await _app.RunAsync(["registry", "list"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Configured registries:", _testingOutputWriter.ToString());
    }

    /** Test the 'lampman registry add [URL]' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryAdd"), TestPriority(302)]
    public async Task RegistryAdd_ShouldCreateRegistryEntry()
    {
        if (null != _registryUrls && _registryUrls.Length > 0)
        {
            // Start with a clean registry file
            File.Delete(PathResolver.RegistryFile);

            foreach (var url in _registryUrls)
            {
                var exitCode = await _app.RunAsync(["registry", "add", url]);

                Assert.Equal(0, exitCode);
            }

            var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(PathResolver.RegistryFile));

            if (null != sources)
                foreach (var url in _registryUrls)
                    Assert.Contains(url, sources);
        }
    }

    /** Test the 'lampman registry remove [URL]' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryRemove"), TestPriority(303)]
    public async Task RegistryRemove_ShouldDeleteRegistryEntry()
    {
        if (PathResolver.DefaultRegistrySource.Count > 0)
        {
            var removeUrl = PathResolver.DefaultRegistrySource.First();
            var exitCode = await _app.RunAsync(["registry", "remove", removeUrl]);

            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(PathResolver.RegistryFile));

            var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(PathResolver.RegistryFile));

            if (null != sources)
                Assert.DoesNotContain(removeUrl, sources);
        }
    }

    /** Test the 'lampman registry update' commands via command line interface **/
    [Fact, Trait("Category", "Command_RegistryUpdate"), TestPriority(304)]
    public async Task RegistryUpdate_ShouldFetchServices()
    {
        var exitCode = await _app.RunAsync(["registry", "update"]);

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(PathResolver.ServicesFile));
        Assert.Contains("ServiceProcess", File.ReadAllText(PathResolver.ServicesFile));
    }
}