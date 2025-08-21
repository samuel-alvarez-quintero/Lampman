using System.Threading.Tasks;
using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class RegistryCommand
    {
        public static void ListExecute()
        {
            var manager = new RegistryManager();
            manager.ListRegistries();
        }

        public static void AddExecute(string url)
        {
            var manager = new RegistryManager();
            manager.AddRegistry(url);
        }

        public static void RemoveExecute(string url)
        {
            var manager = new RegistryManager();
            manager.RemoveRegistry(url);
        }

        public static async Task UpdateExecute()
        {
            var manager = new RegistryManager();
            await manager.UpdateServices();
        }
    }
}
