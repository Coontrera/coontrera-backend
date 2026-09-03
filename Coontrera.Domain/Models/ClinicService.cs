namespace Coontrera.Domain.Models;

public class ClinicService
{
    public string Id { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public List<string> Benefits { get; private set; } = new();
    public string ImageUrl { get; private set; } = string.Empty;
    public string ImageAlt { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime DateRegistered {get; private set;} = DateTime.UtcNow;

    protected ClinicService() { }

    public ClinicService(string title, string description, string imageUrl, string imageAlt = "", List<string>? benefits = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("ImageUrl cannot be empty.", nameof(imageUrl));

        Id = Guid.NewGuid().ToString();
        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        ImageAlt = imageAlt ?? string.Empty;
        Benefits = benefits ?? new List<string>();
        IsActive = true;
        DateRegistered = DateTime.UtcNow;
    }

    public void Update(string title, string description, string imageUrl, string imageAlt, List<string>? benefits)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("ImageUrl cannot be empty.", nameof(imageUrl));

        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        ImageAlt = imageAlt ?? string.Empty;
        Benefits = benefits ?? new List<string>();
    }

    public void SetId(string id)
    {
        Id = id;
    }

    public void SetDateRegistered(DateTime dateRegistered)
    {
        DateRegistered = dateRegistered;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }
}
