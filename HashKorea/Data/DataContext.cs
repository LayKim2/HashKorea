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

    #region user
    public DbSet<User> Users { get; set; }
    public DbSet<UserAuth> UserAuth { get; set; }
    public DbSet<UserRole> UserRole { get; set; }
    public DbSet<Term> Terms { get; set; }
    #endregion


    public DbSet<SystemLog> SystemLogs { get; set; }

}
