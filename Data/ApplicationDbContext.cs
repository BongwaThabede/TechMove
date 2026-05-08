using Microsoft.EntityFrameworkCore;
using TechMove.Models;

namespace TechMove.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client Configuration
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContactDetails).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Region).IsRequired().HasMaxLength(100);
            });

            // Contract Configuration
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ServiceLevel).IsRequired().HasMaxLength(100);
                
                entity.HasOne(e => e.Client)
                      .WithMany(c => c.Contracts)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ServiceRequest Configuration
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostInZAR).HasColumnType("decimal(18,2)");
                
                entity.HasOne(e => e.Contract)
                      .WithMany(c => c.ServiceRequests)
                      .HasForeignKey(e => e.ContractId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}