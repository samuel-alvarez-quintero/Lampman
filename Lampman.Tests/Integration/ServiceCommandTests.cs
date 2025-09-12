using DotNetEnv;
using Lampman.Cli;
using Lampman.Core;
using Lampman.Core.Services;
using Lampman.Tests.Fixtures;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Integration;

[Trait("Category", "Integration"), Trait("Category", "ServiceCommand"), TestCaseOrderer(typeof(PriorityOrderer))]
public class ServiceCommandTests : IClassFixture<MockRegistryFixture>
{
    private readonly string[]? servicesToManage;

    private readonly MockRegistryFixture _fixture;

    private readonly LampmanApp App;

    private readonly StringWriter TestingOutputWriter;

    public ServiceCommandTests(MockRegistryFixture fixture)
    {
        Env.TraversePath().Load();

        _fixture = fixture;

        App = new LampmanApp(_fixture.FakeClient);

        string? services = Environment.GetEnvironmentVariable("TESTING_SERVICES_TO_MANAGE");

        if (!string.IsNullOrEmpty(services))
        {
            servicesToManage = services.Split(';', StringSplitOptions.RemoveEmptyEntries);
            servicesToManage = [.. servicesToManage.Select(_service => _service.Trim())];
        }

        TestingOutputWriter = new();
        Console.SetOut(TestingOutputWriter);
    }

    /** Test the 'lampman service -h' commands via command line interface **/
    [Fact, Trait("Category", "Command_ServiceHelp"), TestPriority(400)]
    public async Task ServiceHelp_ShouldDisplayHelpInformation()
    {
        var exitCode = await App.RunAsync(["service", "-h"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("Description", TestingOutputWriter.ToString());
        Assert.Contains("Commands", TestingOutputWriter.ToString());
        Assert.Contains("install", TestingOutputWriter.ToString());
        Assert.Contains("update", TestingOutputWriter.ToString());
        Assert.Contains("remove", TestingOutputWriter.ToString());
    }

    /** Test the 'lampman service install [service-key]' commands via command line interface **/
    [Fact, Trait("Category", "Command_ServiceInstall"), TestPriority(401)]
    public async Task ServiceInstall_ShouldCreateServiceEntry()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            foreach (var service in servicesToManage)
            {
                var exitCode = await App.RunAsync(["service", "install", service]);

                var (serviceName, version, _) = ServiceResolver.Resolve(service);

                Assert.Equal(0, exitCode);
                Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
                Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
            }
        }
    }

    /** Test the 'lampman service update [service-key]' commands via command line interface **/
    [Fact, Trait("Category", "Command_ServiceUpdate"), TestPriority(402)]
    public async Task ServiceUpdate_ShouldFetchServices()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            foreach (var service in servicesToManage)
            {
                var exitCode = await App.RunAsync(["service", "update", service]);

                var (serviceName, version, _) = ServiceResolver.Resolve(service);

                Assert.Equal(0, exitCode);
                Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
                Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
            }
        }
    }

    /** Test the 'lampman service remove [service-key]'' commands via command line interface **/
    [Fact, Trait("Category", "Command_ServiceRemove"), TestPriority(403)]
    public async Task ServiceRemove_ShouldDeleteServiceEntry()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            foreach (var service in servicesToManage)
            {
                var exitCode = await App.RunAsync(["service", "remove", service]);

                var (serviceName, version, _) = ServiceResolver.Resolve(service);

                Assert.Equal(0, exitCode);
                Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
                Assert.False(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
            }
        }
    }
}