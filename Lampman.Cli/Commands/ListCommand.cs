using Lampman.Core.Services;

namespace Lampman.Cli.Commands
{
    public static class ListCommand
    {
        private static readonly StackManager manager = new();

        public static void Execute()
        {
            manager.ListServices();
        }
    }
}
