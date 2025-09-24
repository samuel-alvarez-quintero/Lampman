namespace Lampman.Core.Models;

public class ServiceProfile
{
    public Dictionary<string, string> Configuration { get; set; } = new();
    public Dictionary<string, string>? Requirements { get; set; } = null;
}