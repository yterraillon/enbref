namespace EnBref.Application.Contracts;

public interface IRssReader
{
    IEnumerable<string> GetRssFeedTitles(string url);
}