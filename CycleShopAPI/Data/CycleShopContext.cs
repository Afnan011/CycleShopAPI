using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace CycleShopAPI.Data
{
    public class CycleShopContext : DbContext
    {
        public CycleShopContext(DbContextOptions<CycleShopContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<CycleType> CycleTypes { get; set; }
        public DbSet<Cycle> Cycles { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

        //public DbSet<InventoryHistory> InventoryHistories { get; set; }
        //public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Enums
            modelBuilder.HasPostgresEnum<UserRole>();
            modelBuilder.HasPostgresEnum<OrderStatus>();
            modelBuilder.HasPostgresEnum<PaymentStatus>();
            modelBuilder.HasPostgresEnum<PaymentType>();

            // Configure constraints
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("NOW()");
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();
            });

            modelBuilder.Entity<Cycle>(entity =>
            {
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");
                entity.HasIndex(c => c.SKU).IsUnique();
                entity.Property(c => c.Price).HasColumnType("decimal(10,2)");
                entity.ToTable(t => t.HasCheckConstraint("CK_Cycle_Price", "\"Price\" > 0"));
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(oi => oi.TotalPrice)
                    .HasColumnType("decimal(10,2)")
                    .HasComputedColumnSql("\"Quantity\" * \"UnitPrice\"", stored: true);

            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");
                entity.HasIndex(c => c.Email).IsUnique();
            });

            modelBuilder.Entity<CycleType>().HasKey(ct => ct.CycleTypeId);

            // Configure relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Employee)
                .WithMany()
                .HasForeignKey(o => o.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cycle ↔ Brand/CycleType
            modelBuilder.Entity<Cycle>()
                .HasOne(c => c.Brand)
                .WithMany()
                .HasForeignKey(c => c.BrandId);

            modelBuilder.Entity<Cycle>()
                .HasOne(c => c.CycleType)
                .WithMany()
                .HasForeignKey(c => c.TypeId);

            // Order ↔ Customer
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId);

            // Order ↔ ShippingAddress
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId);

            // Payment ↔ Order
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId);


            // Soft Delete Query Filters
            modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
            modelBuilder.Entity<Cycle>().HasQueryFilter(c => c.DeletedAt == null);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.DeletedAt == null);
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.Customer.DeletedAt == null);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(oi => oi.Cycle.DeletedAt == null);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => p.Order.Customer.DeletedAt == null);

        }
    }
}