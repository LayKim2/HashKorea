using HashKorea.Models;
using Microsoft.EntityFrameworkCore;

namespace HashKorea.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }


    public DbSet<SystemLog> SystemLogs { get; set; }

}
