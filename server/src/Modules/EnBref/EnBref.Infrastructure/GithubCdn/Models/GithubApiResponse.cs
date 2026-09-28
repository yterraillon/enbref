namespace EnBref.Infrastructure.GithubCdn.Models;

public class GithubApiResponse
{
    public required Content Content { get; set; }
    public required Commit Commit { get; set; }
}

public class Content
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Sha { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class Commit
{
    public string Sha { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}