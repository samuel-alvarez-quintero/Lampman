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
    private readonly Option<string> _branchOption;
    private readonly Option<bool> _forceOption;
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
    /// Downloads service definitions from all registry namespaces and refresh the local services.json
    /// </summary>
    private readonly Command _fetchRegistryCmd;

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

        _branchOption = new("--branch")
        {
            Description = "Branch to fetch at the source",
            Required = true
        };

        _forceOption = new("--force", ["-f"])
        {
            Description = "Force fetching source before the registry manager",
            Required = false,
            DefaultValueFactory = _ => false
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
            _forceOption,
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
            _branchOption,
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

        // definition of the fetch command
        _fetchRegistryCmd = new("fetch", "Refresh the local services.json from remote sources")
        {
            _forceOption,
            _verboseOption
        };
        _fetchRegistryCmd.SetAction(parseResult => FetchExecute(parseResult));

        Subcommands.Add(_fetchRegistryCmd);
    }

    public async Task ListExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);
        bool force = parseResult.GetValue(_forceOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        if (force)
            await _manager.FetchRegistrySources(verbose);

        _manager.ListRegistrySources(verbose);
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

        string? branch = parseResult.GetValue(_branchOption);

        if (null == branch)
            throw new Exception("The branch parameter is required");

        _manager.AddRegistrySource(ns, url, branch);
    }

    public void RemoveExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        string ns = parseResult.GetValue(_nsArgument) ?? string.Empty;

        _manager.RemoveRegistrySource(ns, verbose);
    }

    public async Task FetchExecute(ParseResult parseResult)
    {
        bool verbose = parseResult.GetValue(_verboseOption);
        bool force = parseResult.GetValue(_forceOption);

        if (verbose)
            _manager.HttpBrowserClient = new VerboseBrowserClient();

        await _manager.FetchRegistrySources(force, verbose);
    }
}