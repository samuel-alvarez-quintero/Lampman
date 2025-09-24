namespace Lampman.Core.Models;

public class RegistryServices
{
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, Dictionary<string, ServiceSource>> Services { get; set; } = new();
}