using Lampman.Cli.Commands;
using System.CommandLine;

namespace Lampman.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            Argument<string[]> servicesArgument = new("services")
            {
                Description = "Optional list of services"
            };

            RootCommand rootCommand = new("Lampman CLI - Manage your local development stack");

            Command startCmd = new("start", "Start all or selected services")
            {
                servicesArgument
            };
            startCmd.SetAction(parseResult => StartCommand.Execute(parseResult.GetValue(servicesArgument)));

            Command stopCmd = new("stop", "Stop all or selected services")
            {
                servicesArgument
            };
            stopCmd.SetAction(parseResult => StopCommand.Execute(parseResult.GetValue(servicesArgument)));

            Command restartCmd = new("restart", "Restart all or selected services")
            {
                servicesArgument
            };
            restartCmd.SetAction(parseResult => RestartCommand.Execute(parseResult.GetValue(servicesArgument)));

            Command listCmd = new("list", "List configured services");
            listCmd.SetAction(parseResult => ListCommand.Execute());

            Argument<string> serviceArgument = new("service")
            {
                Description = "Service and version (e.g. php:8.3)"
            };

            Command installCmd = new("install", "Install a service")
            {
                serviceArgument
            };
            installCmd.SetAction(parseResult => ServiceCommand.InstallExecute(parseResult.GetValue(serviceArgument) ?? string.Empty));

            Command updateCmd = new("update", "Update a service")
            {
                serviceArgument
            };
            updateCmd.SetAction(parseResult => ServiceCommand.UpdateExecute(parseResult.GetValue(serviceArgument) ?? string.Empty));

            Command removeCmd = new("remove", "Remove a service")
            {
                serviceArgument
            };
            removeCmd.SetAction(parseResult => ServiceCommand.RemoveExecute(parseResult.GetValue(serviceArgument) ?? string.Empty));

            Command serviceCmd = new("service", "Manage Lampman services");
            serviceCmd.Subcommands.Add(installCmd);
            serviceCmd.Subcommands.Add(updateCmd);
            serviceCmd.Subcommands.Add(removeCmd);

            rootCommand.Subcommands.Add(startCmd);
            rootCommand.Subcommands.Add(stopCmd);
            rootCommand.Subcommands.Add(restartCmd);
            rootCommand.Subcommands.Add(listCmd);
            rootCommand.Subcommands.Add(serviceCmd);

            return rootCommand.Parse(args).Invoke();
        }
    }
}