namespace AuctionService.Data.Entities;

public abstract class Entity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

}