using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class StartCommand : Command
{
    private readonly StackManager _manager = new();

    private readonly Argument<string[]> _servicesArgument;

    public StartCommand(string? name = null, string? description = null)
        : base(name ?? "start", description ?? "Start all or selected services")
    {
        _servicesArgument = new("services")
        {
            Description = "Optional list of services"
        };

        SetAction(parseResult => Execute(parseResult.GetValue(_servicesArgument)));
    }

    public void Execute(string[]? services)
    {
        _manager.StartServices(services);
    }
}