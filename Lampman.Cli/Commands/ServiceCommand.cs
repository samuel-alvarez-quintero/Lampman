using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class ServiceCommand
    {
        public static async void InstallExecute(string service)
        {
            var manager = new ServiceManager();
            await manager.InstallService(service);
        }

        public static async void UpdateExecute(string service)
        {
            var manager = new ServiceManager();
            await manager.UpdateService(service);
        }

        public static void RemoveExecute(string service)
        {
            var manager = new ServiceManager();
            manager.RemoveService(service);
        }
    }
}
