using System.CommandLine;

using Lampman.Core.Services.Stack;

namespace Lampman.Cli.Commands;

/// <summary>
/// Stops the running daemon process of the specified service
/// </summary>
public class StopCommand : Command
{
    private readonly StackManager _manager = new();

    private readonly Argument<string[]> _servicesArgument;

    /// <summary>
    /// Change the default name and description of the command
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    public StopCommand(string? name = null, string? description = null)
        : base(name ?? "stop", description ?? "Stop all or selected services")
    {
        _servicesArgument = new("services")
        {
            Description = "Optional list of services"
        };

        SetAction(parseResult => Execute(parseResult));
    }

    public void Execute(ParseResult parseResult)
    {
        string[]? services = parseResult.GetValue(_servicesArgument);

        _manager.StopServices(services);
    }
}