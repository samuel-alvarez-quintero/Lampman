namespace Lampman.Core.Models;

public class GitHubRegistryNamespace
{
    public string Owner { get; set; } = string.Empty;
    public string RepoName { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string? Tag { get; set; } = null;
    public string? Description { get; set; } = null;
    public DateTime? LastRequest { get; set; } = null;
}