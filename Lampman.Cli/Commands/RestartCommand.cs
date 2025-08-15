using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class RestartCommand
    {
        public static void Execute(string[]? services)
        {
            var manager = new StackManager();
            manager.RestartServices(services);
        }
    }
}
