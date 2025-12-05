using System.Text.Json;

using Lampman.Core.Models;
using Lampman.Core.Utils;

namespace Lampman.Core.Services;

public static class ServiceResolver
{
    public static (string serviceName, string version, ServiceSource metadata) Resolve(string input, string? osTarget = null)
    {
        if (!Directory.Exists(PathResolver.RegistriesDir))
            throw new Exception("Local registry files not found. Run `lampman registry update` first.");

        string osDirectoryName = osTarget ?? AppInfo.GetOSDirectoryName();

        // Parse input
        var (serviceName, version) = Parse(input);

        RegistryServices? registrySources = null;

        string registryNsPath = Path.Combine(PathResolver.RegistriesDir, "namespaces");
        if (Directory.Exists(registryNsPath))
        {
            foreach (var nsPath in Directory.GetDirectories(registryNsPath, "*", SearchOption.TopDirectoryOnly))
            {
                string registryServicePath = Path.Combine(nsPath, osDirectoryName, serviceName, "registry.json");

                if (File.Exists(registryServicePath))
                {
                    registrySources = JsonSerializer.Deserialize<RegistryServices>(File.ReadAllText(registryServicePath));
                    break;
                }
                else
                {
                    throw new Exception($"The registry.json file doesn't exist for the {serviceName} service");
                }
            }
        }

        if (registrySources is null)
            throw new Exception("Local services registry is empty. Run `lampman registry update` first.");

        if (!registrySources.Services.TryGetValue(serviceName, out var selectedServices))
            throw new Exception($"Service `{serviceName}` not found in registry.");

        // Get latest by semantic order
        if (version is null)
        {
            version = selectedServices
            .OrderByDescending(item => item.Key)
            .First(item => item.Value.Url is not null)
            .Key.ToString();
        }
        else
        {
            version = selectedServices
            .Where(item => item.Key.Contains(version))
            .OrderByDescending(item => item.Key)
            .First(item => item.Value.Url is not null)
            .Key.ToString();
        }

        if (!selectedServices.TryGetValue(version, out ServiceSource? metadata))
            throw new Exception($"Service `{serviceName}` does not have version `{version}`.");

        return (serviceName, version, metadata);
    }

    public static (string serviceName, string? version) Parse(string input)
    {
        string[] parts = input.Split(':', 2);
        string serviceName = SlugHelper.GenerateSlug(parts[0]);
        string? version = parts.Length > 1 ? SlugHelper.GenerateSlug(parts[1], true) : null;

        return (serviceName, version);
    }
}