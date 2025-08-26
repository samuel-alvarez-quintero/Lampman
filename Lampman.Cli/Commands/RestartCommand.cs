using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class RestartCommand
    {
        private static readonly StackManager manager = new();

        public static void Execute(string[]? services)
        {
            manager.RestartServices(services);
        }
    }
}
