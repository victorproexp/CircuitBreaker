namespace kundeAPI.Models;

public class KundeDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string KundeCollectionName { get; set; } = null!;
}