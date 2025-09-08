using System.Text.Json;
using DotNetEnv;
using Lampman.Core;
using Lampman.Core.Services;
using Lampman.Tests.TestHelpers;

namespace Lampman.Tests.Unit;

[Trait("Category", "Unit"), Trait("Category", "RegistryManager"), TestCaseOrderer(typeof(PriorityOrderer))]
public class RegistryManagerTests
{
    private readonly RegistryManager _registryManager;
    private readonly string[]? registryUrls;

    public RegistryManagerTests()
    {
        Env.TraversePath().Load();

        _registryManager = new RegistryManager();

        string? registrySources = Environment.GetEnvironmentVariable("TESTING_REGISTRY_SOURCES");

        if (!string.IsNullOrEmpty(registrySources))
        {
            registryUrls = registrySources.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /** Test the 'lampman registry add' commands via RegistryManager class **/
    [Fact, Trait("Category", "Manager_RegistryAdd"), TestPriority(0)]
    public void RegistryAdd_ShouldCreateRegistryEntry()
    {
        if (null != registryUrls && registryUrls.Length > 0)
        {
            // Start with a clean registry file
            File.Delete(PathResolver.RegistryFile);

            foreach (var url in registryUrls)
            {
                Console.WriteLine($"Using registry URL: {url} -- End");
                _registryManager.AddRegistry(url);
            }

            var sources = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(PathResolver.RegistryFile));

            if (null != sources)
                foreach (var url in registryUrls)
                    Assert.Contains(url, sources);
        }
    }

    /** Test the 'lampman registry remove' commands via RegistryManager class **/
    [Fact, Trait("Category", "Manager_RegistryRemove"), TestPriority(1)]
    public void RegistryRemove_ShouldDeleteRegistryEntry()
    {
        if (null != registryUrls && registryUrls.Length > 0)
        {
            var url = registryUrls.First();
            _registryManager.RemoveRegistry(url);
            Assert.DoesNotContain(url, File.ReadAllText(PathResolver.RegistryFile));
        }
    }

    /** Test the 'lampman registry list' commands via RegistryManager class **/
    [Fact, Trait("Category", "Manager_RegistryList"), TestPriority(2)]
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

    /** Test the 'lampman registry update' commands via RegistryManager class **/
    [Fact, Trait("Category", "Manager_RegistryUpdate"), TestPriority(3)]
    public async Task RegistryUpdate_ShouldFetchServices()
    {
        await _registryManager.UpdateServices();

        var servicesContent = File.ReadAllText(PathResolver.ServicesFile);

        Assert.Contains("ServiceProcess", servicesContent);
    }
}
