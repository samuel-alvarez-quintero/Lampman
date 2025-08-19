namespace Lampman.Core.Models
{
    public class ServiceInfo
    {
        public string Url { get; set; } = string.Empty;
        public string? ExtractTo { get; set; } = null;
        public string? Checksum { get; set; } = null;
    }
}