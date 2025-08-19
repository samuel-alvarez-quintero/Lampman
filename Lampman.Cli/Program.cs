using Lampman.Cli.Commands;
using Lampman.Core.Services;
using System.CommandLine;

namespace Lampman.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            RootCommand rootCommand = new("Lampman CLI - Manage your local development stack");

            /**
            * stack commands
            */
            Argument<string[]> servicesArgument = new("services")
            {
                Description = "Optional list of services"
            };

            Command startCmd = new("start", "Start all or selected services")
            {
                servicesArgument
            };
            startCmd.SetAction(parseResult => StartCommand.Execute(parseResult.GetValue(servicesArgument)));
            rootCommand.Subcommands.Add(startCmd);

            Command stopCmd = new("stop", "Stop all or selected services")
            {
                servicesArgument
            };
            stopCmd.SetAction(parseResult => StopCommand.Execute(parseResult.GetValue(servicesArgument)));
            rootCommand.Subcommands.Add(stopCmd);

            Command restartCmd = new("restart", "Restart all or selected services")
            {
                servicesArgument
            };
            restartCmd.SetAction(parseResult => RestartCommand.Execute(parseResult.GetValue(servicesArgument)));
            rootCommand.Subcommands.Add(restartCmd);

            Command listCmd = new("list", "List configured services");
            listCmd.SetAction(parseResult => ListCommand.Execute());
            rootCommand.Subcommands.Add(listCmd);

            /**
            * Service manager commands
            */
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
            rootCommand.Subcommands.Add(serviceCmd);

            /**
            * Registry manager commands
            */
            Command registryCmd = new("registry", "Manage service registries");

            Argument<string> urlArgument = new("url")
            {
                Description = "Registry URL"
            };

            Command listRegistryCmd = new("list", "List configured registries");
            listRegistryCmd.SetAction(parseResult => RegistryCommand.ListExecute());

            Command addRegistryCmd = new("add", "Add a registry source")
            {
                urlArgument
            };
            addRegistryCmd.SetAction(parseResult => RegistryCommand.AddExecute(parseResult.GetValue(urlArgument) ?? string.Empty));

            Command removeRegistryCmd = new("remove", "Remove a registry source")
            {
                urlArgument
            };
            removeRegistryCmd.SetAction(parseResult => RegistryCommand.RemoveExecute(parseResult.GetValue(urlArgument) ?? string.Empty));

            Command updateRegistryCmd = new("update", "Update local services.json from remote sources");
            updateRegistryCmd.SetAction(parseResult => RegistryCommand.UpdateExecute());

            registryCmd.Subcommands.Add(listRegistryCmd);
            registryCmd.Subcommands.Add(addRegistryCmd);
            registryCmd.Subcommands.Add(removeRegistryCmd);
            registryCmd.Subcommands.Add(updateRegistryCmd);
            rootCommand.Subcommands.Add(registryCmd);

            return rootCommand.Parse(args).Invoke();
        }
    }
}