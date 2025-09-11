using System.CommandLine;
using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class StopCommand : Command
{
    private readonly HttpClient HttpClient;

    private readonly StackManager Manager = new();

    private readonly Argument<string[]> servicesArgument;

    public StopCommand(string name, string? description = null, HttpClient? _httpClient = null)
        : base(name, description)
    {
        HttpClient = _httpClient ?? new();

        servicesArgument = new("services")
        {
            Description = "Optional list of services"
        };

        SetAction(parseResult => Execute(parseResult.GetValue(servicesArgument)));
    }

    public void Execute(string[]? services)
    {
        Manager.StopServices(services);
    }
}

