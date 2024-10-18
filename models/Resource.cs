namespace models;

public record Resource
{
    public required Guid ID { get; init; }
    public required string Name { get; init; }
}


