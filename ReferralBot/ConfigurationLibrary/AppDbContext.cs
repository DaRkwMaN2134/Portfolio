using DataLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace ConfigurationLibrary
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupAccess> GroupAccesses { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(u =>
            {
                u.HasKey(user => user.ChatId);
                u.Property(user => user.ChatId).ValueGeneratedNever();
            });

            modelBuilder.Entity<Referral>().HasKey(r => r.Id);
            modelBuilder.Entity<Referral>(refr =>
            {
                refr.HasOne(r => r.Referrer)
                .WithMany(u => u.ReferralsAsReferrer)
                .HasForeignKey(r => r.ReferrerId);

                refr.HasOne(r => r.InvitedUser)
                .WithOne(u => u.ReferralRecord)
                .HasForeignKey<Referral>(r => r.ReferralId);
            });


            modelBuilder.Entity<Transaction>().HasKey(t => t.Id);
            modelBuilder.Entity<Transaction>(t =>
            {
                t.HasOne(tr => tr.User)
                 .WithMany(u => u.Transactions)
                 .HasForeignKey(tr => tr.UserId);
            });


            modelBuilder.Entity<Group>().HasKey(g => g.Id);


            modelBuilder.Entity<GroupAccess>().HasKey(ga => ga.Id);
            modelBuilder.Entity<GroupAccess>(access =>
            {
                access.HasOne(a => a.User)
                  .WithMany(u => u.GroupAccesses)
                  .HasForeignKey(a => a.UserId);

                access.HasOne(a => a.Group)
                  .WithMany(g => g.GroupAccesses)
                  .HasForeignKey(a => a.GroupId);
            });

        }
    }
}
