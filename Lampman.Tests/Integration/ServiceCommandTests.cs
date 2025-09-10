using System.Diagnostics;
using DotNetEnv;
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

    public ServiceCommandTests(MockRegistryFixture fixture)
    {
        Env.TraversePath().Load();

        _fixture = fixture;

        string? services = Environment.GetEnvironmentVariable("TESTING_SERVICES_TO_MANAGE");

        if (!string.IsNullOrEmpty(services))
        {
            servicesToManage = services.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /** Test the 'lampman service -h' commands via command line interface **/
    [Fact, Trait("Category", "Command_ServiceHelp"), TestPriority(300)]
    public async Task ServiceHelp_ShouldDisplayHelpInformation()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "lampman.dll service -h",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = PathResolver.RootDir
            }
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        Assert.Equal(0, process.ExitCode);
        Assert.Contains("Description", output);
        Assert.Contains("Commands", output);
        Assert.Contains("install", output);
        Assert.Contains("update", output);
        Assert.Contains("remove", output);
    }

    /** Test the 'lampman service install' commands via command line interface **/
    [Fact, Trait("Category", "Command_ServiceInstall"), TestPriority(301)]
    public async Task ServiceInstall_ShouldCreateServiceEntry()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            foreach (var service in servicesToManage)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"lampman.dll service install {service}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = PathResolver.RootDir
                    }
                };

                process.Start();
                await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
                process.WaitForExit();

                var (serviceName, version, meta) = ServiceResolver.Resolve(service);

                Assert.Equal(0, process.ExitCode);
                Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
                Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
            }
        }
    }

    /** Test the 'lampman service update' commands via command line interface **/
    [Fact, Trait("Category", "Command_ServiceUpdate"), TestPriority(302)]
    public async Task ServiceUpdate_ShouldFetchServices()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            var firstService = servicesToManage.First();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"lampman.dll service update {firstService}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = PathResolver.RootDir
                }
            };

            process.Start();
            await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            process.WaitForExit();

            var (serviceName, version, meta) = ServiceResolver.Resolve(firstService);

            Assert.Equal(0, process.ExitCode);
            Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
            Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
        }
    }

    /** Test the 'lampman service remove' commands via command line interface **/
    [Fact, Trait("Category", "Command_ServiceRemove"), TestPriority(303)]
    public async Task ServiceRemove_ShouldDeleteServiceEntry()
    {
        if (null != servicesToManage && servicesToManage.Length > 0)
        {
            var lastService = servicesToManage.Last();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"lampman.dll service remove {lastService}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = PathResolver.RootDir
                }
            };

            process.Start();
            await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            process.WaitForExit();

            var (serviceName, version, meta) = ServiceResolver.Resolve(lastService);

            Assert.Equal(0, process.ExitCode);
            Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
            Assert.False(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
        }
    }
}