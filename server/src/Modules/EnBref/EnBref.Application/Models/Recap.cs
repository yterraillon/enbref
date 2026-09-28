namespace EnBref.Application.Models;

public class Recap
{
    public string Title { get; set; } = string.Empty;
    public List<Section> Sections { get; init; } = [];
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    
    public static Recap EmptyRecap => new()
    {
        Title = string.Empty,
        Sections = []
    };
    
    public void SetCreatedAt()
    {
        CreatedAt = DateTime.UtcNow;;
    }
}

public class Section
{
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}