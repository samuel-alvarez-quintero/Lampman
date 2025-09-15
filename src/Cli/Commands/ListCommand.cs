using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class ListCommand : Command
{
    private readonly StackManager _manager = new();

    public ListCommand(string name, string? description = null)
        : base(name, description)
    {
        SetAction(parseResult => Execute());
    }

    private void Execute()
    {
        _manager.ListServices();
    }
}