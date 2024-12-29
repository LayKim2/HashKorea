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
    public DbSet<UserPost> UserPosts { get; set; }
    public DbSet<UserPostImage> UserPostImages { get; set; }
    #endregion


    public DbSet<CommonCode> CommonCodes { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }

}
