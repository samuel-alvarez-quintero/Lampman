using DotNetEnv;

using Lampman.Cli;
using Lampman.Tests.Fixtures;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Integration;

[Trait("Category", "Integration"), Trait("Category", "StackCommand"), TestCaseOrderer(typeof(PriorityOrderer))]
public class StackCommandTests : IClassFixture<MockRegistryFixture>
{
    private readonly string[]? servicesToManage;

    private readonly MockRegistryFixture _fixture;

    private readonly LampmanApp _app;

    private readonly StringWriter _testingOutputWriter;

    public StackCommandTests(MockRegistryFixture fixture)
    {
        Env.TraversePath().Load();

        _fixture = fixture;

        _app = new LampmanApp(_fixture.FakeClient);

        string? services = Environment.GetEnvironmentVariable("TESTING_SERVICES_TO_MANAGE");

        if (!string.IsNullOrEmpty(services))
        {
            servicesToManage = services.Split(';', StringSplitOptions.RemoveEmptyEntries);
            servicesToManage = [.. servicesToManage.Select(_service => _service.Trim())];
        }

        _testingOutputWriter = new();
        Console.SetOut(_testingOutputWriter);
    }

    /** Test the 'lampman -h' commands via command line interface **/
    [Fact, Trait("Category", "Command_StackHelp"), TestPriority(500)]
    public async Task StackHelp_ShouldDisplayHelpInformation()
    {
        var exitCode = await _app.RunAsync(["-h"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Description", _testingOutputWriter.ToString());
        Assert.Contains("Lampman CLI - Manage your local development stack", _testingOutputWriter.ToString());
        Assert.Contains("Commands", _testingOutputWriter.ToString());
        Assert.Contains("start", _testingOutputWriter.ToString());
        Assert.Contains("stop", _testingOutputWriter.ToString());
        Assert.Contains("restart", _testingOutputWriter.ToString());
        Assert.Contains("list", _testingOutputWriter.ToString());
        Assert.Contains("service", _testingOutputWriter.ToString());
        Assert.Contains("registry", _testingOutputWriter.ToString());
    }

    /** Test the 'lampman list' commands via command line interface **/
    [Fact, Trait("Category", "Command_StackList"), TestPriority(501)]
    public async Task StackList_ShouldDisplayServicesListing()
    {
        var exitCode = await _app.RunAsync(["list"]);

        Assert.Equal(0, exitCode);
    }

    /** Test the 'lampman start [service-key]' commands via command line interface **/
    [Fact, Trait("Category", "Command_StackStart"), TestPriority(502)]
    public async Task StackStart_ShouldRunServiceInstalled()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            foreach (var service in servicesToManage)
            {
                var exitCode = await _app.RunAsync(["start", service]);

                // Return an error code because the service is not installed
                Assert.Equal(1, exitCode);
            }
        }
    }

    /** Test the 'lampman restart [service-key]' commands via command line interface **/
    [Fact, Trait("Category", "Command_StackRestart"), TestPriority(503)]
    public async Task StackRestart_ShouldRestartServiceInstalled()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            foreach (var service in servicesToManage)
            {
                var exitCode = await _app.RunAsync(["restart", service]);

                // Return an error code because the service is not installed
                Assert.Equal(1, exitCode);
            }
        }
    }

    /** Test the 'lampman stop [service-key]' commands via command line interface **/
    [Fact, Trait("Category", "Command_StackStop"), TestPriority(504)]
    public async Task StackStop_ShouldStopServiceInstalled()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            foreach (var service in servicesToManage)
            {
                var exitCode = await _app.RunAsync(["stop", service]);

                // Return an error code because the service is not installed
                Assert.Equal(1, exitCode);
            }
        }
    }
}