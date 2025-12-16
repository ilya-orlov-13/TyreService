using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers;

/// <summary>
/// Контроллер для обработки запросов главной страницы и системных операций.
/// Предоставляет статистику, общую информацию о системе и обработку ошибок.
/// </summary>
/// <remarks>
/// Этот контроллер отвечает за:
/// - Отображение главной страницы с аналитикой шиномонтажа
/// - Отображение страницы конфиденциальности
/// - Обработку глобальных ошибок приложения
/// </remarks>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="HomeController"/>.
    /// </summary>
    /// <param name="logger">Экземпляр логгера для записи событий и ошибок.</param>
    /// <param name="context">Контекст базы данных для доступа к сущностям приложения.</param>
    /// <remarks>
    /// Использует внедрение зависимостей для получения необходимых сервисов.
    /// </remarks>
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Возвращает представление главной страницы с аналитическими данными системы.
    /// </summary>
    /// <returns>
    /// Представление <see cref="IActionResult"/>, содержащее панель управления с ключевыми метриками:
    /// - Общее количество клиентов, автомобилей, заказов, услуг, мастеров, шин и выполненных работ
    /// - Активные заказы (без выполненных работ)
    /// - Завершенные заказы (с выполненными работами)
    /// - Заказы за текущий день
    /// - Неоплаченные заказы
    /// - Заказы с назначенными мастерами
    /// - Топ-3 клиентов по количеству заказов
    /// - Последние 5 заказов с подробной информацией
    /// </returns>
    /// <example>
    /// GET: /Home/Index
    /// Возвращает панель управления с текущей статистикой шиномонтажа.
    /// </example>
    /// <remarks>
    /// Метод выполняет несколько запросов к базе данных для сбора аналитики.
    /// Все данные передаются во View через ViewBag.
    /// </remarks>
    public IActionResult Index()
    {
        // Базовые счетчики
        ViewBag.ClientsCount = _context.Clients.Count();
        ViewBag.CarsCount = _context.Cars.Count();
        ViewBag.OrdersCount = _context.Orders.Count();
        ViewBag.ServicesCount = _context.Services.Count();
        ViewBag.MastersCount = _context.Masters.Count();
        ViewBag.TiresCount = _context.Tires.Count();
        ViewBag.CompletedWorksCount = _context.CompletedWorks.Count();

        // Активные заказы (без выполненных работ)
        var allOrders = _context.Orders.ToList();
        var ordersWithCompletedWorks = _context.CompletedWorks
            .Select(cw => cw.OrderNumber)
            .Distinct()
            .ToList();
        
        ViewBag.ActiveOrdersCount = allOrders
            .Where(o => !ordersWithCompletedWorks.Contains(o.OrderNumber))
            .Count();

        // Завершенные заказы (с выполненными работами)
        ViewBag.CompletedOrdersCount = ordersWithCompletedWorks.Count;

        // Заказы за сегодня
        ViewBag.TodayOrdersCount = _context.Orders
            .Where(o => o.OrderDate.Date == DateTime.Today)
            .Count();

        // Неоплаченные заказы
        ViewBag.UnpaidOrdersCount = _context.Orders
            .Where(o => o.PaymentDate == null)
            .Count();

        // Заказы с назначенными мастерами
        ViewBag.OrdersWithMastersCount = _context.Orders
            .Where(o => o.MasterId != null)
            .Count();

        // Статистика по клиентам
        var topClients = _context.Clients
            .Select(c => new
            {
                Client = c,
                OrderCount = _context.Cars
                    .Where(car => car.ClientId == c.ClientId)
                    .SelectMany(car => car.Orders)
                    .Count()
            })
            .OrderByDescending(x => x.OrderCount)
            .Take(3)
            .ToList();

        ViewBag.TopClients = topClients;

        // Последние 5 заказов для отображения
        ViewBag.RecentOrders = _context.Orders
            .Include(o => o.Car)
                .ThenInclude(c => c.Client)
            .Include(o => o.Master)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToList();

        return View();
    }

    /// <summary>
    /// Возвращает представление страницы политики конфиденциальности.
    /// </summary>
    /// <returns>
    /// Представление <see cref="IActionResult"/> с информацией о политике конфиденциальности.
    /// </returns>
    /// <example>
    /// GET: /Home/Privacy
    /// Возвращает страницу с политикой конфиденциальности приложения.
    /// </example>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Обрабатывает и отображает страницу ошибок приложения.
    /// </summary>
    /// <returns>
    /// Представление <see cref="IActionResult"/> с информацией об ошибке.
    /// </returns>
    /// <remarks>
    /// Метод использует атрибут <see cref="ResponseCacheAttribute"/> для отключения кэширования страницы ошибок.
    /// Включает идентификатор запроса для отладки.
    /// </remarks>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}