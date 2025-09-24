namespace Lampman.Core.Models;

public class ServiceProcess
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public string? Args { get; set; } = null;
    public bool isExtention { get; set; } = false;
    public string? ItDependsOn { get; set; } = null;
    public bool MustBeDemonizing { get; set; } = false;
    public bool AvailableToPathEnvVar { get; set; } = true;
}