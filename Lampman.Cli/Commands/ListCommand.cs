using System.CommandLine;
using Lampman.Core.Services;

namespace Lampman.Cli.Commands;

public class ListCommand : Command
{
    private readonly HttpClient HttpClient;

    private readonly StackManager Manager = new();

    public ListCommand(string name, string? description = null, HttpClient? _httpClient = null)
        : base(name, description)
    {
        HttpClient = _httpClient ?? new();

        SetAction(parseResult => Execute());
    }

    public void Execute()
    {
        Manager.ListServices();
    }
}

