using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

/// <summary>
/// The Service Command allows users to install, update, and remove services available in registry.json file. 
/// This command interacts with the stack.json file to track installed services and their versions.
/// </summary>
public class ServiceCommand : Command
{
    private readonly HttpClient _httpClient;

    private readonly ServiceManager _manager;

    private readonly Argument<string> _serviceArgument;


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
        this._httpClient = httpClient ?? new();

        _manager = new(this._httpClient);

        _serviceArgument = new("service")
        {
            Description = "Service and version (e.g. php:8.3)"
        };

        // definition of the install command
        _installCmd = new("install", "Install a service")
        {
            _serviceArgument
        };
        _installCmd.SetAction(parseResult => InstallExecute(parseResult.GetValue(_serviceArgument) ?? string.Empty));

        Subcommands.Add(_installCmd);

        // definition of the update command
        _updateCmd = new("update", "Update a service")
        {
            _serviceArgument
        };
        _updateCmd.SetAction(parseResult => UpdateExecute(parseResult.GetValue(_serviceArgument) ?? string.Empty));

        Subcommands.Add(_updateCmd);

        // definition of the remove command
        _removeCmd = new("remove", "Remove a service")
            {
                _serviceArgument
            };
        _removeCmd.SetAction(parseResult => RemoveExecute(parseResult.GetValue(_serviceArgument) ?? string.Empty));

        Subcommands.Add(_removeCmd);
    }

    public void InstallExecute(string service)
    {
        Task.Run(() => _manager.InstallService(service)).Wait();
    }

    public void UpdateExecute(string service)
    {
        Task.Run(() => _manager.UpdateService(service)).Wait();
    }

    public void RemoveExecute(string service)
    {
        _manager.RemoveService(service);
    }
}