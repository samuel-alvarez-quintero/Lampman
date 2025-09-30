using System.CommandLine;

using Lampman.Core.Services;
using Lampman.Core.Utils;

namespace Lampman.Cli.Commands;

/// <summary>
/// The Registry Command allows users to manage registries from which services can be installed. 
/// This command will interact with the local registry.json file and the services.json file that stores available services from those registries.
/// </summary>
public class RegistryCommand : Command
{
    private readonly RegistryManager _manager;

    private readonly Argument<string> _nsArgument;
    private readonly Argument<string> _urlArgument;
    private readonly Option<string> _descriptionOption;
    private readonly Option<string> _checksumOption;
    private readonly Option<bool> _verboseOption;

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
        _manager = new(httpClient ?? new BrowserClient());

        _nsArgument = new("ns")
        {
            Description = "Namespace key"
        };

        _urlArgument = new("url")
        {
            Description = "Registry URL"
        };

        _descriptionOption = new("--description")
        {
            Description = "Description of the new registry",
            Required = false
        };

        _checksumOption = new("--checksum")
        {
            Description = "Checksum in format HASHFUNC:HASHVALUE",
            Required = false
        };

        _verboseOption = new("--verbose", ["-v"])
        {
            Description = "The output provides detailed logs and error messages",
            Required = false,
            DefaultValueFactory = _ => false
        };

        // definition of the list command
        _listRegistryCmd = new("list", "List configured registries")
        {
            _verboseOption
        };
        _listRegistryCmd.SetAction(parseResult => ListExecute(parseResult));

        Subcommands.Add(_listRegistryCmd);

        // definition of the add command
        _addRegistryCmd = new("add", "Add a registry source")
        {
            _nsArgument,
            _urlArgument,
            _descriptionOption,
            _checksumOption,
            _verboseOption
        };
        _addRegistryCmd.SetAction(parseResult => AddExecute(parseResult));

        Subcommands.Add(_addRegistryCmd);

        // definition of the remove command
        _removeRegistryCmd = new("remove", "Remove a registry source")
        {
            _nsArgument,
            _verboseOption
        };
        _removeRegistryCmd.SetAction(parseResult => RemoveExecute(parseResult));

        Subcommands.Add(_removeRegistryCmd);

        // definition of the update command
        _updateRegistryCmd = new("update", "Update local services.json from remote sources")
        {
            _verboseOption
        };
        _updateRegistryCmd.SetAction(parseResult => UpdateExecute(parseResult));

        Subcommands.Add(_updateRegistryCmd);
    }

    public void ListExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        _manager.ListRegistries(verbose);
    }

    public void AddExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        string? ns = parseResult.GetValue(_nsArgument);

        if (null == ns)
            throw new Exception("The namespace parameter is required");

        string? url = parseResult.GetValue(_urlArgument);

        if (null == url)
            throw new Exception("The URL parameter is required");

        string? description = parseResult.GetValue(_descriptionOption);
        string? checksum = parseResult.GetValue(_checksumOption);

        if (!string.IsNullOrEmpty(checksum))
        {
            var parts = checksum.Split(':', 2);
            if (parts.Length != 2)
            {
                Console.WriteLine("[ERROR] --checksum must be HASHFUNC:HASHVALUE");
                return;
            }

            _manager.AddRegistry(ns, url, description, hashFunc: parts[0], hashValue: parts[1], verbose: verbose);
        }
        else
        {
            _manager.AddRegistry(ns, url, description, verbose: verbose);
        }
    }

    public void RemoveExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        string ns = parseResult.GetValue(_nsArgument) ?? string.Empty;

        _manager.RemoveRegistry(ns, verbose);
    }

    public async Task UpdateExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        await _manager.UpdateServices(verbose);
    }
}