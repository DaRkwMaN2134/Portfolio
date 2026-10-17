using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore.Design;
using ShortUrl.Domain.Models;


namespace ShortUrl.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> users {  get; set; }
        public DbSet<Link> links { get; set; }
        public DbSet<Click> clicks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(user => user.Id);
            modelBuilder.Entity<User>(user =>
            {
                user.HasIndex(u => u.UserId)
                .IsUnique();

                user.HasIndex(u => u.DeviceId)
                .IsUnique();
            });


            modelBuilder.Entity<Link>().HasKey(link => link.Id);
            modelBuilder.Entity<Link>(creator =>
            {
                creator.HasOne(crt => crt.Creator)
                .WithMany(u => u.Links)
                .HasForeignKey(crt => crt.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);


                creator.HasIndex(shrl => shrl.ShortCode)
                .IsUnique();
            });

            modelBuilder.Entity<Click>().HasKey(click => click.Id);
            modelBuilder.Entity<Click>(click =>
            {
                click.HasOne(l => l.Link)
                .WithMany(c => c.Clicks)
                .HasForeignKey(l => l.LinkId);
            });
        }
    }
}
