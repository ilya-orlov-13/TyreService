using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Areas.Customer.Models;
using TyreServiceApp.Areas.Worker.Models;
using TyreServiceApp.Models;

namespace TyreServiceApp.Data
{
    /// <summary>
    /// Контекст базы данных для приложения шиномонтажа.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Инициализирует новый экземпляр контекста базы данных.
        /// </summary>
        /// <param name="options">Параметры конфигурации контекста.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Client> Clients { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Master> Masters { get; set; }
        public DbSet<Tire> Tires { get; set; }
        public DbSet<CompletedWork> CompletedWorks { get; set; }
        public DbSet<CustomerUser> CustomerUsers { get; set; }
        public DbSet<MasterUser> MasterUsers { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<StaffPosition> StaffPositions { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<OwnerUser> OwnerUsers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostActiveSession> PostActiveSessions { get; set; }
        public DbSet<CompletedJobsPayout> CompletedJobsPayouts { get; set; }
        public DbSet<CarClass> CarClasses { get; set; }
        public DbSet<ComplexityCoefficient> ComplexityCoefficients { get; set; }
        public DbSet<ServiceTariff> ServiceTariffs { get; set; }
        public DbSet<Consumable> Consumables { get; set; }
        public DbSet<OrderConsumable> OrderConsumables { get; set; }
        public DbSet<OrderComplexity> OrderComplexities { get; set; }
        public DbSet<OwnerSetting> OwnerSettings { get; set; }
        public DbSet<SpeedBonus> SpeedBonuses { get; set; }
        public DbSet<WorkTimeLog> WorkTimeLogs { get; set; }
        public DbSet<CustomerReview> CustomerReviews { get; set; }

        /// <summary>
        /// Настраивает модель данных и связи между сущностями.
        /// </summary>
        /// <param name="modelBuilder">Построитель модели.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Cars)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Car)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CarId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Tire)
                .WithMany()
                .HasForeignKey(o => o.TireId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Master)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MasterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Tire>()
                .HasOne(t => t.Car)
                .WithMany(c => c.Tires)
                .HasForeignKey(t => t.CarId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Tire>()
                .HasOne(t => t.Client)
                .WithMany(cl => cl.Tires)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompletedWork>()
                .HasOne(cw => cw.Order)
                .WithMany(o => o.CompletedWorks)
                .HasForeignKey(cw => cw.OrderNumber)
                .HasPrincipalKey(o => o.OrderNumber)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompletedWork>()
                .HasOne(cw => cw.Service)
                .WithMany(s => s.CompletedWorks)
                .HasForeignKey(cw => cw.ServiceCode)
                .HasPrincipalKey(s => s.ServiceCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompletedWork>()
                .HasOne(cw => cw.Master)
                .WithMany(m => m.CompletedWorks)
                .HasForeignKey(cw => cw.MasterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CustomerUser>(entity =>
            {
                entity.HasIndex(u => u.Phone).IsUnique();
                entity.Property(u => u.PinHash).IsRequired();
                entity.HasOne(u => u.Client)
                    .WithMany()
                    .HasForeignKey(u => u.ClientId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CustomerReview>(entity =>
            {
                entity.HasIndex(r => r.CustomerId);
                entity.HasOne(r => r.Customer)
                    .WithMany()
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MasterUser>(entity =>
            {
                entity.HasIndex(u => u.Login).IsUnique();
                entity.HasOne(u => u.Master)
                    .WithMany()
                    .HasForeignKey(u => u.MasterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Master>(entity =>
            {
                entity.HasOne(m => m.Position)
                    .WithMany(p => p.Masters)
                    .HasForeignKey(m => m.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AdminUser>(entity =>
            {
                entity.HasIndex(u => u.Login).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.HasOne(u => u.StaffPosition)
                    .WithMany(sp => sp.AdminUsers)
                    .HasForeignKey(u => u.StaffPositionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<OwnerUser>(entity =>
            {
                entity.HasIndex(u => u.Login).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex(p => p.Name).IsUnique();
            });

            modelBuilder.Entity<PostActiveSession>(entity =>
            {
                entity.HasOne(s => s.Post)
                    .WithMany(p => p.ActiveSessions)
                    .HasForeignKey(s => s.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Master)
                    .WithMany(m => m.PostActiveSessions)
                    .HasForeignKey(s => s.MasterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CompletedJobsPayout>(entity =>
            {
                entity.HasOne(p => p.Order)
                    .WithMany(o => o.CompletedJobsPayouts)
                    .HasForeignKey(p => p.OrderNumber)
                    .HasPrincipalKey(o => o.OrderNumber)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Master)
                    .WithMany(m => m.CompletedJobsPayouts)
                    .HasForeignKey(p => p.MasterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WorkTimeLog>(entity =>
            {
                entity.HasOne(w => w.CompletedWork)
                    .WithMany(cw => cw.WorkTimeLogs)
                    .HasForeignKey(w => w.WorkId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Master)
                    .WithMany(m => m.WorkTimeLogs)
                    .HasForeignKey(w => w.MasterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasOne(c => c.CarClass)
                    .WithMany(cc => cc.Cars)
                    .HasForeignKey(c => c.CarClassId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ServiceTariff>(entity =>
            {
                entity.HasOne(st => st.Service)
                    .WithMany(s => s.ServiceTariffs)
                    .HasForeignKey(st => st.ServiceCode)
                    .HasPrincipalKey(s => s.ServiceCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(st => st.CarClass)
                    .WithMany(cc => cc.ServiceTariffs)
                    .HasForeignKey(st => st.CarClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(st => new { st.ServiceCode, st.CarClassId }).IsUnique();
            });

            modelBuilder.Entity<OrderConsumable>(entity =>
            {
                entity.HasOne(oc => oc.Order)
                    .WithMany(o => o.OrderConsumables)
                    .HasForeignKey(oc => oc.OrderNumber)
                    .HasPrincipalKey(o => o.OrderNumber)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oc => oc.Consumable)
                    .WithMany(c => c.OrderConsumables)
                    .HasForeignKey(oc => oc.ConsumableId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(oc => new { oc.OrderNumber, oc.ConsumableId }).IsUnique();
            });

            modelBuilder.Entity<OrderComplexity>(entity =>
            {
                entity.HasOne(oc => oc.Order)
                    .WithMany(o => o.OrderComplexities)
                    .HasForeignKey(oc => oc.OrderNumber)
                    .HasPrincipalKey(o => o.OrderNumber)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oc => oc.ComplexityCoefficient)
                    .WithMany(cc => cc.OrderComplexities)
                    .HasForeignKey(oc => oc.ComplexityCoefficientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(oc => new { oc.OrderNumber, oc.ComplexityCoefficientId }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}