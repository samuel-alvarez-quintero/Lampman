using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class RestartCommand : Command
{
    private readonly StackManager Manager = new();

    private readonly Argument<string[]> servicesArgument;

    public RestartCommand(string name, string? description = null)
        : base(name, description)
    {
        servicesArgument = new("services")
        {
            Description = "Optional list of services"
        };

        SetAction(parseResult => Execute(parseResult.GetValue(servicesArgument)));
    }

    public void Execute(string[]? services)
    {
        Manager.RestartServices(services);
    }
}