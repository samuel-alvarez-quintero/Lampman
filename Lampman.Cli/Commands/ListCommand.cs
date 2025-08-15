using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class ListCommand
    {
        public static void Execute()
        {
            var manager = new StackManager();
            manager.ListServices();
        }
    }
}
