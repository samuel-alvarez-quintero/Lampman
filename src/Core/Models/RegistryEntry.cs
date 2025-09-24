namespace Lampman.Core.Models;

public class RegistryEntry
{
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Checksum { get; set; } = null;
}