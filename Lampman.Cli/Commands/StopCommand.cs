using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class StopCommand
    {
        public static void Execute(string[]? services)
        {
            var manager = new StackManager();
            manager.StopServices(services);
        }
    }
}
