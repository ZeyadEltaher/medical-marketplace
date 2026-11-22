namespace Medical.DAL.Entities;

public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public string? Describtion { get; set; }
    public ICollection<License> Licenses { get; set; } = new List<License>();
}