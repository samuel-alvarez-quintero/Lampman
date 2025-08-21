namespace Lampman.Core.Models
{
    public class ServiceSource
    {
        public string Url { get; set; } = string.Empty;
        public string? ExtractTo { get; set; } = null;
        public string? Checksum { get; set; } = null;
        public ServiceProcess? ServiceProcess { get; set; } = null;
    }
}