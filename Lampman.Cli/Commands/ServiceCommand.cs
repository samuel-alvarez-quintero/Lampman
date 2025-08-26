using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class ServiceCommand
    {
        private static readonly ServiceManager manager = new();

        public static void InstallExecute(string service)
        {
            Task.Run(() => manager.InstallService(service)).Wait();
        }

        public static void UpdateExecute(string service)
        {
            Task.Run(() => manager.UpdateService(service)).Wait();
        }

        public static void RemoveExecute(string service)
        {
            manager.RemoveService(service);
        }
    }
}
