using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class RegistryCommand : Command
{
    private readonly HttpClient _httpClient;

    private readonly RegistryManager _manager;

    private readonly Argument<string> _urlArgument;

    private readonly Command _listRegistryCmd;

    private readonly Command _addRegistryCmd;

    private readonly Command _removeRegistryCmd;

    private readonly Command _updateRegistryCmd;

    public RegistryCommand(string name, string? description = null, HttpClient? httpClient = null)
        : base(name, description)
    {
        _httpClient = httpClient ?? new();

        _manager = new(_httpClient);

        _urlArgument = new("url")
        {
            Description = "Registry URL"
        };

        _listRegistryCmd = new("list", "List configured registries");
        _listRegistryCmd.SetAction(parseResult => ListExecute());

        Subcommands.Add(_listRegistryCmd);

        _addRegistryCmd = new("add", "Add a registry source")
        {
            _urlArgument
        };
        _addRegistryCmd.SetAction(parseResult => AddExecute(parseResult.GetValue(_urlArgument) ?? string.Empty));

        Subcommands.Add(_addRegistryCmd);

        _removeRegistryCmd = new("remove", "Remove a registry source")
        {
            _urlArgument
        };
        _removeRegistryCmd.SetAction(parseResult => RemoveExecute(parseResult.GetValue(_urlArgument) ?? string.Empty));

        Subcommands.Add(_removeRegistryCmd);

        _updateRegistryCmd = new("update", "Update local services.json from remote sources");
        _updateRegistryCmd.SetAction(parseResult => UpdateExecute());

        Subcommands.Add(_updateRegistryCmd);
    }

    public void ListExecute()
    {
        _manager.ListRegistries();
    }

    public void AddExecute(string url)
    {
        _manager.AddRegistry(url);
    }

    public void RemoveExecute(string url)
    {
        _manager.RemoveRegistry(url);
    }

    public async Task UpdateExecute()
    {
        await _manager.UpdateServices();
    }
}