using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class RestartCommand : Command
{
    private readonly StackManager _manager = new();

    private readonly Argument<string[]> _servicesArgument;

    public RestartCommand(string name, string? description = null)
        : base(name, description)
    {
        _servicesArgument = new("services")
        {
            Description = "Optional list of services"
        };

        SetAction(parseResult => Execute(parseResult.GetValue(_servicesArgument)));
    }

    public void Execute(string[]? services)
    {
        _manager.RestartServices(services);
    }
}