namespace Lampman.Core.Models;

public class ServiceProcess
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Start { get; set; } = null;
    public string? Stop { get; set; } = null;
    public string? Restart { get; set; } = null;
}