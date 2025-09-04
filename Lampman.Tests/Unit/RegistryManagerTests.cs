using Lampman.Core;
using Lampman.Core.Services;

namespace Lampman.Tests;

[Trait("Category", "Unit")]
public class RegistryManagerTests
{
    private readonly RegistryManager _registryManager;

    public RegistryManagerTests()
    {
        _registryManager = new RegistryManager();
    }

    [Fact]
    public void RegistryAdd_ShouldCreateRegistryEntry()
    {
        _registryManager.AddRegistry("https://example.com/registry.json");

        Assert.True(File.Exists(PathResolver.RegistryFile));
        Assert.Contains("https://example.com/registry.json", File.ReadAllText(PathResolver.RegistryFile));
    }

    [Fact]
    public void RegistryRemove_ShouldDeleteRegistryEntry()
    {
        _registryManager.RemoveRegistry("https://example.com/registry.json");

        Assert.DoesNotContain("https://example.com/registry.json", File.ReadAllText(PathResolver.RegistryFile));
    }

    [Fact]
    public void RegistryList_ShouldDisplayRegistryEntries()
    {
        StringWriter sw = new();

        Console.SetOut(sw);

        _registryManager.ListRegistries();

        Assert.Contains(PathResolver.DefaultRegistrySource.First(), sw.ToString());

        // Now you have to restore default output stream
        StreamWriter standardOutput = new(Console.OpenStandardOutput())
        {
            AutoFlush = true
        };

        Console.SetOut(standardOutput);
    }

    [Fact]
    public async Task RegistryUpdate_ShouldFetchServices()
    {
        await _registryManager.UpdateServices();

        var servicesContent = File.ReadAllText(PathResolver.ServicesFile);

        Assert.Contains("ServiceProcess", servicesContent);
    }
}