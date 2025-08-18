using Lampman.Installer.Commands;
using System.CommandLine;

namespace Lampman.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            RootCommand rootCommand = new("Lampman CLI - Manage your local development stack");

            Option<string> pathOption = new(name: "--path")
            {
                Description = "Installation path",
                DefaultValueFactory = _ => @"C:\Lampman"
            };

            Option<bool> repairOption = new(name: "--repair")
            {
                Description = "Repair installation by overwriting files and regenerating stack.json"
            };

            Option<bool> addToPathOption = new(name: "--add-to-path")
            {
                Description = "Add Lampman to system PATH"
            };

            Option<bool> runAfterOption = new(name: "--run-after")
            {
                Description = "Run Lampman after install"
            };

            Option<bool> removeFromPathOption = new(name: "--remove-from-path")
            {
                Description = "Remove Lampman from system PATH"
            };

            Command installCommand = new("install", "Install or update Lampman stack")
            {
                pathOption,
                repairOption,
                addToPathOption,
                runAfterOption
            };
            installCommand.SetAction(parseResult => InstallCommand.Execute(
                parseResult.GetValue(pathOption) ?? "",
                parseResult.GetValue(repairOption),
                parseResult.GetValue(addToPathOption),
                parseResult.GetValue(runAfterOption))
            );

            Command uninstallCommand = new("uninstall", "Uninstall Lampman stack")
            {
                pathOption,
                removeFromPathOption
            };
            uninstallCommand.SetAction(parseResult => UninstallCommand.Execute(
                parseResult.GetValue(pathOption) ?? "",
                parseResult.GetValue(removeFromPathOption))
            );

            rootCommand.Subcommands.Add(installCommand);
            rootCommand.Subcommands.Add(uninstallCommand);

            return rootCommand.Parse(args).Invoke();
        }
    }
}