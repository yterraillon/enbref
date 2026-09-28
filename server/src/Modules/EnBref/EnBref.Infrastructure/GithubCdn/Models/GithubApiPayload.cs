namespace EnBref.Infrastructure.GithubCdn.Models;

public class GithubApiPayload
{
    public string Message { get; set; } = "Update file"; 
    public string Content { get; set; } = string.Empty; //BASE64_ENCODED_NEW_CONTENT
    public string Sha { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
}