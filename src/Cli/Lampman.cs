using System.CommandLine;

using Lampman.Cli.Commands;
using Lampman.Core.Utils;

namespace Lampman.Cli;

public class LampmanApp
{
    private readonly HttpClient _httpClient;

    public LampmanApp(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new BrowserClient();
    }

    public async Task<int> RunAsync(string[] args)
    {
        RootCommand rootCommand = new("Lampman CLI - Manage your local development stack");

        StartCommand startCmd = new();
        StopCommand stopCmd = new();
        RestartCommand restartCmd = new();
        ListCommand listCmd = new();

        rootCommand.Subcommands.Add(startCmd);
        rootCommand.Subcommands.Add(stopCmd);
        rootCommand.Subcommands.Add(restartCmd);
        rootCommand.Subcommands.Add(listCmd);

        // Service manager commands
        ServiceCommand serviceCmd = new(httpClient: _httpClient);

        rootCommand.Subcommands.Add(serviceCmd);

        // Registry manager commands
        RegistryCommand registryCmd = new(httpClient: _httpClient);

        rootCommand.Subcommands.Add(registryCmd);

        return await rootCommand.Parse(args).InvokeAsync();
    }
}