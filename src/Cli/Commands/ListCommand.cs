using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class ListCommand : Command
{
    private readonly StackManager _manager = new();

    public ListCommand(string? name = null, string? description = null)
        : base(name ?? "list", description ?? "List configured services")
    {
        SetAction(parseResult => Execute());
    }

    private void Execute()
    {
        _manager.ListServices();
    }
}