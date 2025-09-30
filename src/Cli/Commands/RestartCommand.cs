using System.CommandLine;

using Lampman.Core.Services.Stack;

namespace Lampman.Cli.Commands;

/// <summary>
/// Equivalent to stop followed by start
/// </summary>
public class RestartCommand : Command
{
    private readonly StackManager _manager = new();

    private readonly Argument<string[]> _servicesArgument;

    /// <summary>
    /// Change the default name and description of the command
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    public RestartCommand(string? name = null, string? description = null)
        : base(name ?? "restart", description ?? "Restart all or selected services")
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

        _manager.RestartServices(services);
    }
}