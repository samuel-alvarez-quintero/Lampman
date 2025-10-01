namespace Lampman.Core.Models;

public class RegistryNamespace
{
    public string Url { get; set; } = string.Empty;
    public string? Version { get; set; } = null;
    public string? Description { get; set; } = null;
    public Dictionary<string, string>? Checksum { get; set; } = null;
    public DateTime? LastRequest { get; set; } = null;
}