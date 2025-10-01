namespace Lampman.Core.Models;

public class RegistryServices
{
    public string? Version { get; set; } = null;
    public string? Description { get; set; }
    public DateTime? LastRequest { get; set; } = DateTime.Now;
    public Dictionary<string, Dictionary<string, ServiceSource>> Services { get; set; } = new();
}