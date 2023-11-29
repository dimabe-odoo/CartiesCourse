using AuctionService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data.Connections;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {      
    }

    public DbSet<Auction> Auctions {get; set;}

    public DbSet<Item> Items {get; set;}


}