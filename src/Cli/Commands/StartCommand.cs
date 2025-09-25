using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

/// <summary>
/// Launches the specified service as a daemon process
/// </summary>
public class StartCommand : Command
{
    private readonly StackManager _manager = new();

    private readonly Argument<string[]> _servicesArgument;

    /// <summary>
    /// Change the default name and description of the command
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    public StartCommand(string? name = null, string? description = null)
        : base(name ?? "start", description ?? "Start all or selected services")
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

        _manager.StartServices(services);
    }
}