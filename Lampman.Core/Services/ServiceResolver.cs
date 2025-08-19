using System.Text.Json;
using Lampman.Core.Models;

namespace Lampman.Core.Services
{
    public static class ServiceResolver
    {
        private static readonly string ServicesConfigFile = PathResolver.ServicesFile;

        public static (string name, string version, ServiceInfo meta) Resolve(string input)
        {
            if (!File.Exists(ServicesConfigFile))
                throw new Exception("Local services registry not found. Run `lampman registry update` first.");

            var services = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ServiceInfo>>>(
                File.ReadAllText(ServicesConfigFile));

            // Parse input
            var parts = input.Split(':', 2);
            var name = parts[0];
            var version = parts.Length > 1 ? parts[1] : null;

            if (services is null)
                throw new Exception("Local services registry is empty. Run `lampman registry update` first.");

            if (!services.TryGetValue(name, out var versions))
                throw new Exception($"Service `{name}` not found in registry.");

            // Get latest by semantic order
            version ??= versions.Keys
                .Select(v => new Version(v))
                .OrderByDescending(v => v)
                .First()
                .ToString();

            if (!versions.TryGetValue(version, out ServiceInfo? value))
                throw new Exception($"Service `{name}` does not have version `{version}`.");

            return (name, version, value);
        }
    }
}
