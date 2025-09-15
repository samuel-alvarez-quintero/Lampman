using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class ServiceCommand : Command
{
    private readonly HttpClient HttpClient;

    private readonly ServiceManager Manager;

    private readonly Argument<string> serviceArgument;

    private readonly Command InstallCmd;

    private readonly Command UpdateCmd;

    private readonly Command RemoveCmd;

    public ServiceCommand(string name, string? description = null, HttpClient? _httpClient = null)
        : base(name, description)
    {
        HttpClient = _httpClient ?? new();

        Manager = new(HttpClient);

        serviceArgument = new("service")
        {
            Description = "Service and version (e.g. php:8.3)"
        };

        InstallCmd = new("install", "Install a service")
        {
            serviceArgument
        };
        InstallCmd.SetAction(parseResult => InstallExecute(parseResult.GetValue(serviceArgument) ?? string.Empty));

        Subcommands.Add(InstallCmd);

        UpdateCmd = new("update", "Update a service")
        {
            serviceArgument
        };
        UpdateCmd.SetAction(parseResult => UpdateExecute(parseResult.GetValue(serviceArgument) ?? string.Empty));

        Subcommands.Add(UpdateCmd);

        RemoveCmd = new("remove", "Remove a service")
            {
                serviceArgument
            };
        RemoveCmd.SetAction(parseResult => RemoveExecute(parseResult.GetValue(serviceArgument) ?? string.Empty));

        Subcommands.Add(RemoveCmd);
    }

    public void InstallExecute(string service)
    {
        Task.Run(() => Manager.InstallService(service)).Wait();
    }

    public void UpdateExecute(string service)
    {
        Task.Run(() => Manager.UpdateService(service)).Wait();
    }

    public void RemoveExecute(string service)
    {
        Manager.RemoveService(service);
    }
}