using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class StartCommand
    {
        public static void Execute(string[]? services)
        {
            var manager = new StackManager();
            manager.StartServices(services);
        }
    }
}
