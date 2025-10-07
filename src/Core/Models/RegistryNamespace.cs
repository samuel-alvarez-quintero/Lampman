namespace Lampman.Core.Models;

public class RegistryNamespace
{
    public string Source { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string? Tag { get; set; } = null;
    public string? Description { get; set; } = null;
    public DateTime? LastRequest { get; set; } = null;
}