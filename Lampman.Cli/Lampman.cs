using System.CommandLine;
using Lampman.Cli.Commands;

namespace Lampman.Cli;

public class LampmanApp(HttpClient? _httpClient = null)
{
    private readonly HttpClient HttpClient = _httpClient ?? new();

    public async Task<int> RunAsync(string[] args)
    {
        RootCommand rootCommand = new("Lampman CLI - Manage your local development stack");

        StartCommand startCmd = new("start", "Start all or selected services");
        StopCommand stopCmd = new("stop", "Stop all or selected services");
        RestartCommand restartCmd = new("restart", "Restart all or selected services");
        ListCommand listCmd = new("list", "List configured services");

        rootCommand.Subcommands.Add(startCmd);
        rootCommand.Subcommands.Add(stopCmd);
        rootCommand.Subcommands.Add(restartCmd);
        rootCommand.Subcommands.Add(listCmd);

        /**
        * Service manager commands
        */
        ServiceCommand serviceCmd = new("service", "Manage Lampman services", HttpClient);

        rootCommand.Subcommands.Add(serviceCmd);

        /**
        * Registry manager commands
        */
        RegistryCommand registryCmd = new("registry", "Manage service registries", HttpClient);

        rootCommand.Subcommands.Add(registryCmd);

        return await rootCommand.Parse(args).InvokeAsync();
    }
}
