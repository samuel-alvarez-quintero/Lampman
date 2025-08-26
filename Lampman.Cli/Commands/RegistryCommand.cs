using System.Threading.Tasks;
using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class RegistryCommand
    {
        private static readonly RegistryManager manager = new();

        public static void ListExecute()
        {
            manager.ListRegistries();
        }

        public static void AddExecute(string url)
        {
            manager.AddRegistry(url);
        }

        public static void RemoveExecute(string url)
        {
            manager.RemoveRegistry(url);
        }

        public static async Task UpdateExecute()
        {
            await manager.UpdateServices();
        }
    }
}
