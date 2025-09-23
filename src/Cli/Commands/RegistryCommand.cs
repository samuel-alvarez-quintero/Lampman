using System.CommandLine;

using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

/// <summary>
/// The Registry Command allows users to manage registries from which services can be installed. 
/// This command will interact with the local registry.json file and the services.json file that stores available services from those registries.
/// </summary>
public class RegistryCommand : Command
{
    private readonly HttpClient _httpClient;

    private readonly RegistryManager _manager;

    private readonly Argument<string> _urlArgument;

    /// <summary>
    /// Lists all registry namespaces and their URLs from the local registry.json
    /// </summary>
    private readonly Command _listRegistryCmd;

    /// <summary>
    /// Adds a new registry URL under the specified namespace to the local registry.json
    /// </summary>
    private readonly Command _addRegistryCmd;

    /// <summary>
    /// Removes a registry namespace (and its URL) from the local registry.json
    /// </summary>
    private readonly Command _removeRegistryCmd;

    /// <summary>
    /// Downloads service definitions from all registry namespaces and updates the local services.json
    /// </summary>
    private readonly Command _updateRegistryCmd;

    public RegistryCommand(string? name = null, string? description = null, HttpClient? httpClient = null)
        : base(name ?? "registry", description ?? "Manage service registries")
    {
        _httpClient = httpClient ?? new();

        _manager = new(_httpClient);

        _urlArgument = new("url")
        {
            Description = "Registry URL"
        };

        // definition of the list command
        _listRegistryCmd = new("list", "List configured registries");
        _listRegistryCmd.SetAction(parseResult => ListExecute());

        Subcommands.Add(_listRegistryCmd);

        // definition of the add command
        _addRegistryCmd = new("add", "Add a registry source")
        {
            _urlArgument
        };
        _addRegistryCmd.SetAction(parseResult => AddExecute(parseResult.GetValue(_urlArgument) ?? string.Empty));

        Subcommands.Add(_addRegistryCmd);

        // definition of the remove command
        _removeRegistryCmd = new("remove", "Remove a registry source")
        {
            _urlArgument
        };
        _removeRegistryCmd.SetAction(parseResult => RemoveExecute(parseResult.GetValue(_urlArgument) ?? string.Empty));

        Subcommands.Add(_removeRegistryCmd);

        // definition of the update command
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