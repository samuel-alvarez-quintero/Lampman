using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class ServiceCommand : Command
{
    private readonly HttpClient _httpClient;

    private readonly ServiceManager _manager;

    private readonly Argument<string> _serviceArgument;

    private readonly Command _installCmd;

    private readonly Command _updateCmd;

    private readonly Command _removeCmd;

    public ServiceCommand(string name, string? description = null, HttpClient? httpClient = null)
        : base(name, description)
    {
        this._httpClient = httpClient ?? new();

        _manager = new(this._httpClient);

        _serviceArgument = new("service")
        {
            Description = "Service and version (e.g. php:8.3)"
        };

        _installCmd = new("install", "Install a service")
        {
            _serviceArgument
        };
        _installCmd.SetAction(parseResult => InstallExecute(parseResult.GetValue(_serviceArgument) ?? string.Empty));

        Subcommands.Add(_installCmd);

        _updateCmd = new("update", "Update a service")
        {
            _serviceArgument
        };
        _updateCmd.SetAction(parseResult => UpdateExecute(parseResult.GetValue(_serviceArgument) ?? string.Empty));

        Subcommands.Add(_updateCmd);

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