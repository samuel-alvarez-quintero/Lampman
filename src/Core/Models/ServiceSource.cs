namespace Lampman.Core.Models;

public class ServiceSource
{
    public string Url { get; set; } = string.Empty;
    public string? ExtractTo { get; set; } = null;
    public Dictionary<string, string>? Checksum { get; set; } = null;

    // Multiple processes per service version
    public List<ServiceProcess> Processes { get; set; } = new();

    // Profiles: default, dev, prod, etc.
    public Dictionary<string, ServiceProfile> Profiles { get; set; } = new();
}