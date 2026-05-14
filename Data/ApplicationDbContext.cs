using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechMove.Models;
using Microsoft.AspNetCore.Identity;

namespace TechMove.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

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
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Region);
                entity.HasIndex(e => new { e.Name, e.Region }).HasDatabaseName("IX_Clients_Name_Region");
            });

            // Contract Configuration
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Draft");
                entity.Property(e => e.ServiceLevel).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ContractValueUSD).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ContractValueZAR).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SignedAgreementPath).HasMaxLength(500);
                entity.Property(e => e.SignedAgreementFileName).HasMaxLength(255);
                entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastModifiedDate).IsRequired(false);
                
                entity.HasOne(e => e.Client)
                      .WithMany(c => c.Contracts)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.StartDate, e.EndDate });
            });

            // ServiceRequest Configuration
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostInZAR).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.RequestDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Priority).HasMaxLength(50).HasDefaultValue("Normal");
                entity.Property(e => e.RequestNumber).HasMaxLength(100);
                entity.Property(e => e.AdminNotes).HasMaxLength(500);
                
                entity.HasOne(e => e.Contract)
                      .WithMany(c => c.ServiceRequests)
                      .HasForeignKey(e => e.ContractId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasIndex(e => e.ContractId);
                entity.HasIndex(e => e.Status);
            });
        }
    }
}