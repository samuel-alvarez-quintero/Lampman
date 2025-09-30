using System.CommandLine;

using Lampman.Core.Services;
using Lampman.Core.Utils;

namespace Lampman.Cli.Commands;

/// <summary>
/// The Service Command allows users to install, update, and remove services available in registry.json file. 
/// This command interacts with the stack.json file to track installed services and their versions.
/// </summary>
public class ServiceCommand : Command
{
    private readonly ServiceManager _manager;

    private readonly Argument<string> _serviceArgument;
    private readonly Option<bool> _verboseOption;

    /// <summary>
    /// Download and install the service in the services directory
    /// </summary>
    private readonly Command _installCmd;

    /// <summary>
    /// Delete the installed service and install the latest available version
    /// </summary>
    private readonly Command _updateCmd;

    /// <summary>
    /// Delete the installed service
    /// </summary>
    private readonly Command _removeCmd;

    public ServiceCommand(string? name = null, string? description = null, HttpClient? httpClient = null)
        : base(name ?? "service", description ?? "Manage Lampman services")
    {
        _manager = new(httpClient ?? new BrowserClient());

        _serviceArgument = new("service")
        {
            Description = "Service and version (e.g. php:8.3)"
        };

        _verboseOption = new("--verbose", ["-v"])
        {
            Description = "The output provides detailed logs and error messages",
            Required = false,
            DefaultValueFactory = _ => false
        };

        // definition of the install command
        _installCmd = new("install", "Install a service")
        {
            _serviceArgument,
            _verboseOption
        };
        _installCmd.SetAction(parseResult => InstallExecute(parseResult));

        Subcommands.Add(_installCmd);

        // definition of the update command
        _updateCmd = new("update", "Update a service")
        {
            _serviceArgument,
            _verboseOption
        };
        _updateCmd.SetAction(parseResult => UpdateExecute(parseResult));

        Subcommands.Add(_updateCmd);

        // definition of the remove command
        _removeCmd = new("remove", "Remove a service")
            {
                _serviceArgument,
                _verboseOption
            };
        _removeCmd.SetAction(parseResult => RemoveExecute(parseResult));

        Subcommands.Add(_removeCmd);
    }

    public void InstallExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        string service = parseResult.GetValue(_serviceArgument) ?? string.Empty;

        Task.Run(() => _manager.InstallService(service)).Wait();
    }

    public void UpdateExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        string service = parseResult.GetValue(_serviceArgument) ?? string.Empty;

        Task.Run(() => _manager.UpdateService(service)).Wait();
    }

    public void RemoveExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        string service = parseResult.GetValue(_serviceArgument) ?? string.Empty;

        _manager.RemoveService(service);
    }
}