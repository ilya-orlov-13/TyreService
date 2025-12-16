using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Models;

namespace TyreServiceApp.Data
{
    /// <summary>
    /// Контекст базы данных для приложения "Шиномонтаж".
    /// Наследуется от <see cref="DbContext"/> и обеспечивает доступ к таблицам базы данных.
    /// </summary>
    /// <remarks>
    /// Этот контекст использует Entity Framework Core для маппинга объектов C# на таблицы PostgreSQL.
    /// Конфигурация связей между сущностями выполняется в методе <see cref="OnModelCreating"/>.
    /// </remarks>
    /// <example>
    /// Пример использования в контроллере:
    /// <code>
    /// public class CarsController : Controller
    /// {
    ///     private readonly ApplicationDbContext _context;
    ///     
    ///     public CarsController(ApplicationDbContext context)
    ///     {
    ///         _context = context;
    ///     }
    ///     
    ///     public async Task<IActionResult> Index()
    ///     {
    ///         var cars = await _context.Cars.ToListAsync();
    ///         return View(cars);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">Параметры конфигурации контекста базы данных.</param>
        /// <remarks>
        /// Конструктор используется для внедрения зависимостей через DI-контейнер ASP.NET Core.
        /// Параметры конфигурации обычно включают строку подключения к PostgreSQL.
        /// </remarks>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        /// <summary>
        /// Представляет таблицу <c>Clients</c> (Клиенты) в базе данных.
        /// </summary>
        /// <value>
        /// Набор данных типа <see cref="Client"/>, позволяющий выполнять операции CRUD
        /// (создание, чтение, обновление, удаление) с клиентами шиномонтажа.
        /// </value>
        /// <remarks>
        /// Каждый клиент может иметь несколько автомобилей (<see cref="Cars"/>).
        /// </remarks>
        public DbSet<Client> Clients { get; set; }
        
        /// <summary>
        /// Представляет таблицу <c>Cars</c> (Автомобили) в базе данных.
        /// </summary>
        /// <value>
        /// Набор данных типа <see cref="Car"/>, содержащий информацию об автомобилях клиентов.
        /// </value>
        /// <remarks>
        /// Каждый автомобиль принадлежит одному клиенту (<see cref="Client"/>)
        /// и может иметь несколько заказов (<see cref="Orders"/>) и шин (<see cref="Tires"/>).
        /// </remarks>
        public DbSet<Car> Cars { get; set; }
        
        /// <summary>
        /// Представляет таблицу <c>Orders</c> (Заказы) в базе данных.
        /// </summary>
        /// <value>
        /// Набор данных типа <see cref="Order"/>, содержащий информацию о заказах на обслуживание.
        /// </value>
        /// <remarks>
        /// Каждый заказ связан с одним автомобилем (<see cref="Car"/>), 
        /// может быть назначен на мастера (<see cref="Master"/>) и содержит выполненные работы (<see cref="CompletedWorks"/>).
        /// </remarks>
        public DbSet<Order> Orders { get; set; }
        
        /// <summary>
        /// Представляет таблицу <c>Services</c> (Услуги) в базе данных.
        /// </summary>
        /// <value>
        /// Набор данных типа <see cref="Service"/>, содержащий прайс-лист услуг шиномонтажа.
        /// </value>
        /// <remarks>
        /// Услуги включают: шиномонтаж, балансировку, ремонт шин и т.д.
        /// Каждая услуга может использоваться в нескольких выполненных работах (<see cref="CompletedWork"/>).
        /// </remarks>
        public DbSet<Service> Services { get; set; }
        
        /// <summary>
        /// Представляет таблицу <c>Masters</c> (Мастера) в базе данных.
        /// </summary>
        /// <value>
        /// Набор данных типа <see cref="Master"/>, содержащий информацию о сотрудниках шиномонтажа.
        /// </value>
        /// <remarks>
        /// Мастера имеют разряд, почасовую ставку и могут быть назначены на заказы (<see cref="Orders"/>)
        /// и выполненные работы (<see cref="CompletedWorks"/>).
        /// </remarks>
        public DbSet<Master> Masters { get; set; }
        
        /// <summary>
        /// Представляет таблицу <c>CompletedWorks</c> (Выполненные работы) в базе данных.
        /// </summary>
        /// <value>
        /// Набор данных типа <see cref="CompletedWork"/>, содержащий детализацию выполненных работ в рамках заказов.
        /// </value>
        /// <remarks>
        /// Каждая выполненная работа относится к одному заказу (<see cref="Order"/>),
        /// одной услуге (<see cref="Service"/>) и одному мастеру (<see cref="Master"/>).
        /// </remarks>
        public DbSet<CompletedWork> CompletedWorks { get; set; }
        
        /// <summary>
        /// Представляет таблицу <c>Tires</c> (Шины) в базе данных.
        /// </summary>
        /// <value>
        /// Набор данных типа <see cref="Tire"/>, содержащий информацию о шинах автомобилей.
        /// </value>
        /// <remarks>
        /// Каждая шина может быть установлена на одном автомобиле (<see cref="Car"/>)
        /// или находиться на складе (без привязки к автомобилю).
        /// </remarks>
        public DbSet<Tire> Tires { get; set; }
        
        /// <summary>
        /// Настраивает модель базы данных и связи между сущностями при создании контекста.
        /// </summary>
        /// <param name="modelBuilder">
        /// Построитель моделей, используемый для настройки модели базы данных.
        /// </param>
        /// <remarks>
        /// Этот метод вызывается автоматически при первом использовании контекста.
        /// Он настраивает:
        /// <list type="bullet">
        /// <item><description>Связь один-ко-многим между <see cref="Client"/> и <see cref="Car"/> с каскадным удалением</description></item>
        /// <item><description>Связь один-ко-многим между <see cref="Car"/> и <see cref="Order"/></description></item>
        /// <item><description>Связь один-ко-многим между <see cref="Master"/> и <see cref="Order"/> с установкой NULL при удалении</description></item>
        /// <item><description>Связь один-ко-многим между <see cref="Order"/> и <see cref="CompletedWork"/> с каскадным удалением</description></item>
        /// <item><description>Связь один-к-одному между <see cref="Service"/> и <see cref="CompletedWork"/> с ограничением удаления</description></item>
        /// <item><description>Связь один-ко-многим между <see cref="Master"/> и <see cref="CompletedWork"/> с ограничением удаления</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// Пример настройки каскадного удаления:
        /// <code>
        /// modelBuilder.Entity<Car>()
        ///     .HasOne(c => c.Client)
        ///     .WithMany(cl => cl.Cars)
        ///     .HasForeignKey(c => c.ClientId)
        ///     .OnDelete(DeleteBehavior.Cascade);
        /// </code>
        /// Это означает, что при удалении клиента будут удалены все его автомобили.
        /// </example>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Настройка связей
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Cars)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Car)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CarId);
                
            // Добавляем связь Order ↔ Master
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Master)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MasterId)
                .OnDelete(DeleteBehavior.SetNull);
                
            modelBuilder.Entity<CompletedWork>()
                .HasOne(cw => cw.Order)
                .WithMany(o => o.CompletedWorks)
                .HasForeignKey(cw => cw.OrderNumber)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<CompletedWork>()
                .HasOne(cw => cw.Service)
                .WithMany()
                .HasForeignKey(cw => cw.ServiceCode)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<CompletedWork>()
                .HasOne(cw => cw.Master)
                .WithMany(m => m.CompletedWorks)
                .HasForeignKey(cw => cw.MasterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}