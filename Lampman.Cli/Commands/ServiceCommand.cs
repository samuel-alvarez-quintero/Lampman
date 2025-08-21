using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class ServiceCommand
    {
        public static void InstallExecute(string service)
        {
            var manager = new ServiceManager();
            Task.Run(() => manager.InstallService(service)).Wait();
        }

        public static void UpdateExecute(string service)
        {
            var manager = new ServiceManager();
            Task.Run(() => manager.UpdateService(service)).Wait();
        }

        public static void RemoveExecute(string service)
        {
            var manager = new ServiceManager();
            manager.RemoveService(service);
        }
    }
}
