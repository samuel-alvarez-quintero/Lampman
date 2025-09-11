using System.CommandLine;
using System.Threading.Tasks;
using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class RegistryCommand : Command
{
    private readonly HttpClient HttpClient;

    private readonly RegistryManager Manager;

    private readonly Argument<string> urlArgument;

    private readonly Command ListRegistryCmd;

    private readonly Command AddRegistryCmd;

    private readonly Command RemoveRegistryCmd;

    private readonly Command UpdateRegistryCmd;

    public RegistryCommand(string name, string? description = null, HttpClient? _httpClient = null)
        : base(name, description)
    {
        HttpClient = _httpClient ?? new();

        Manager = new(HttpClient);

        urlArgument = new("url")
        {
            Description = "Registry URL"
        };

        ListRegistryCmd = new("list", "List configured registries");
        ListRegistryCmd.SetAction(parseResult => ListExecute());

        Subcommands.Add(ListRegistryCmd);

        AddRegistryCmd = new("add", "Add a registry source")
        {
            urlArgument
        };
        AddRegistryCmd.SetAction(parseResult => AddExecute(parseResult.GetValue(urlArgument) ?? string.Empty));

        Subcommands.Add(AddRegistryCmd);

        RemoveRegistryCmd = new("remove", "Remove a registry source")
        {
            urlArgument
        };
        RemoveRegistryCmd.SetAction(parseResult => RemoveExecute(parseResult.GetValue(urlArgument) ?? string.Empty));

        Subcommands.Add(RemoveRegistryCmd);

        UpdateRegistryCmd = new("update", "Update local services.json from remote sources");
        UpdateRegistryCmd.SetAction(parseResult => UpdateExecute());

        Subcommands.Add(UpdateRegistryCmd);
    }

    public void ListExecute()
    {
        Manager.ListRegistries();
    }

    public void AddExecute(string url)
    {
        Manager.AddRegistry(url);
    }

    public void RemoveExecute(string url)
    {
        Manager.RemoveRegistry(url);
    }

    public async Task UpdateExecute()
    {
        await Manager.UpdateServices();
    }
}

