using System.Text.Json;
using Lampman.Core.Models;
using Lampman.Core.Utils;

namespace Lampman.Core.Services
{
    public static class ServiceResolver
    {
        private static readonly string ServicesConfigFile = PathResolver.ServicesFile;

        public static (string serviceName, string version, ServiceSource metadata) Resolve(string input)
        {
            if (!File.Exists(ServicesConfigFile))
                throw new Exception("Local services registry not found. Run `lampman registry update` first.");

            var services = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ServiceSource>>>(
                File.ReadAllText(ServicesConfigFile));

            // Parse input
            var (serviceName, version) = Parse(input);

            if (services is null)
                throw new Exception("Local services registry is empty. Run `lampman registry update` first.");

            if (!services.TryGetValue(serviceName, out var serviceSelected))
                throw new Exception($"Service `{serviceName}` not found in registry.");

            // Get latest by semantic order
            if (version is null)
            {
                version = serviceSelected.Keys
                .OrderByDescending(v => v)
                .First()
                .ToString();
            }
            else
            {
                version = serviceSelected.Keys
                .Where(v => v.Contains(version))
                .OrderByDescending(v => v)
                .First()
                .ToString();
            }

            if (!serviceSelected.TryGetValue(version, out ServiceSource? metadata))
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
}
