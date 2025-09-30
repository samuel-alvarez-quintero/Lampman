using System.CommandLine;

using Lampman.Core.Services.Stack;

namespace Lampman.Cli.Commands;

/// <summary>
/// Lists all installed services
/// </summary>
public class ListCommand : Command
{
    private readonly StackManager _manager = new();

    /// <summary>
    /// Change the default name and description of the command
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
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