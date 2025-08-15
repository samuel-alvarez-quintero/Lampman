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

            rootCommand.Subcommands.Add(startCmd);
            rootCommand.Subcommands.Add(stopCmd);
            rootCommand.Subcommands.Add(restartCmd);
            rootCommand.Subcommands.Add(listCmd);

            return rootCommand.Parse(args).Invoke();
        }
    }
}