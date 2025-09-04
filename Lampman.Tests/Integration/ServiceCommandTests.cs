using System.Diagnostics;
using Lampman.Core;
using Lampman.Core.Services;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Integration;

[Trait("Category", "Integration"), TestCaseOrderer(typeof(PriorityOrderer))]
public class ServiceCommandTests
{
    private readonly string serviceInput = "mariadb";

    /** Test the 'lampman service -h' commands via command line interface **/
    [Fact, Trait("Category", "ServiceHelpCommand"), TestPriority(4)]
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
    [Fact, Trait("Category", "ServiceInstallCommand"), TestPriority(5)]
    public async Task ServiceInstall_ShouldCreateServiceEntry()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"lampman.dll service install {serviceInput}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = PathResolver.RootDir
            }
        };

        process.Start();
        await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        var (serviceName, version, meta) = ServiceResolver.Resolve(serviceInput);

        Assert.Equal(0, process.ExitCode);
        Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
        Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
    }

    /** Test the 'lampman service update' commands via command line interface **/
    [Fact, Trait("Category", "ServiceUpdateCommand"), TestPriority(6)]
    public async Task ServiceUpdate_ShouldFetchServices()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"lampman.dll service update {serviceInput}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = PathResolver.RootDir
            }
        };

        process.Start();
        await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        var (serviceName, version, meta) = ServiceResolver.Resolve(serviceInput);

        Assert.Equal(0, process.ExitCode);
        Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
        Assert.True(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
    }

    /** Test the 'lampman service remove' commands via command line interface **/
    [Fact, Trait("Category", "ServiceRemoveCommand"), TestPriority(7)]
    public async Task ServiceRemove_ShouldDeleteServiceEntry()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"lampman.dll service remove {serviceInput}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = PathResolver.RootDir
            }
        };

        process.Start();
        await process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        process.WaitForExit();

        var (serviceName, version, meta) = ServiceResolver.Resolve(serviceInput);

        Assert.Equal(0, process.ExitCode);
        Assert.True(Directory.Exists(PathResolver.ServicesInstallDir));
        Assert.False(Directory.Exists(PathResolver.ServicePath(serviceName, version)));
    }
}