using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers
{
    /// <summary>
    /// Контроллер для управления заказами в системе шиномонтажа.
    /// Обеспечивает CRUD-операции с заказами, включая создание, редактирование, просмотр и удаление.
    /// </summary>
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера заказов.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех заказов с информацией о связанных автомобилях и мастерах.
        /// </summary>
        /// <returns>Представление со списком всех заказов.</returns>
        /// <remarks>
        /// Включает связанные данные:
        /// - Автомобиль (Car)
        /// - Клиент автомобиля (Client)
        /// - Ответственный мастер (Master)
        /// - Выполненные работы (CompletedWorks)
        /// </remarks>
        // GET: Orders/Index
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Client)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks)
                .ToListAsync();
                
            return View(orders);
        }

        /// <summary>
        /// Отображает детальную информацию о конкретном заказе.
        /// </summary>
        /// <param name="id">Идентификатор заказа (OrderNumber).</param>
        /// <returns>
        /// Представление с детальной информацией о заказе.
        /// Если заказ не найден, возвращает NotFound.
        /// </returns>
        /// <remarks>
        /// Включает полную информацию:
        /// - Основные данные заказа
        /// - Автомобиль и его владельца
        /// - Ответственного мастера
        /// - Все выполненные работы с деталями услуг
        /// </remarks>
        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Client)
                .Include(o => o.Master)
                .Include(o => o.CompletedWorks)
                    .ThenInclude(cw => cw.Service)
                .FirstOrDefaultAsync(m => m.OrderNumber == id);
                
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// Отображает форму для создания нового заказа.
        /// </summary>
        /// <returns>Представление формы создания заказа.</returns>
        /// <remarks>
        /// Подготавливает данные для выпадающих списков:
        /// - Список автомобилей с информацией о клиентах
        /// - Список мастеров с информацией о должности и ставке
        /// - Данные для JavaScript (фото автомобилей)
        /// </remarks>
        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            // Получаем автомобили
            var cars = await _context.Cars
                .Include(c => c.Client)
                .Select(c => new
                {
                    carId = c.CarId,
                    displayText = $"{c.Brand} {c.Model} ({c.LicensePlate}) - {c.Client.FullName}",
                    brand = c.Brand,
                    model = c.Model,
                    licensePlate = c.LicensePlate,
                    photoPath = c.PhotoPath ?? "",
                    clientName = c.Client.FullName,
                    vin = c.Vin ?? "без VIN"
                })
                .ToListAsync();

            // Получаем мастеров
            var mastersList = await _context.Masters
                .OrderBy(m => m.FullName)
                .ToListAsync();

            var masters = mastersList.Select(m => new
            {
                masterId = m.MasterId,
                displayText = $"{m.FullName} ({m.Position}, {m.Rank} разряд) - {m.HourlyRate:C2}/час",
                fullName = m.FullName,
                position = m.Position,
                rank = m.Rank,
                hourlyRate = m.HourlyRate
            }).ToList();

            ViewBag.CarId = new SelectList(cars, "carId", "displayText");
            ViewBag.MasterId = new SelectList(masters, "masterId", "displayText");
            ViewBag.CarsData = cars;

            return View();
        }

        /// <summary>
        /// Обрабатывает данные формы создания нового заказа.
        /// </summary>
        /// <param name="order">Объект заказа с данными из формы.</param>
        /// <returns>
        /// При успешном создании: перенаправление на список заказов.
        /// При ошибках валидации: повторное отображение формы с сообщениями об ошибках.
        /// </returns>
        /// <remarks>
        /// Проверяет ModelState.IsValid перед сохранением.
        /// При ошибке повторно загружает данные для выпадающих списков.
        /// </remarks>
        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderDate,CarId,MasterId,PaymentDate")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // При ошибке валидации загружаем данные снова
            var cars = await _context.Cars
                .Include(c => c.Client)
                .Select(c => new
                {
                    carId = c.CarId,
                    displayText = $"{c.Brand} {c.Model} ({c.LicensePlate}) - {c.Client.FullName}",
                    brand = c.Brand,
                    model = c.Model,
                    licensePlate = c.LicensePlate,
                    photoPath = c.PhotoPath ?? "",
                    clientName = c.Client.FullName,
                    vin = c.Vin ?? "без VIN"
                })
                .ToListAsync();
            
            // Получаем мастеров
            var mastersList = await _context.Masters
                .OrderBy(m => m.FullName)
                .ToListAsync();
            
            var masters = mastersList.Select(m => new
            {
                masterId = m.MasterId,
                displayText = $"{m.FullName} ({m.Position}, {m.Rank} разряд) - {m.HourlyRate:C2}/час",
                fullName = m.FullName,
                position = m.Position,
                rank = m.Rank,
                hourlyRate = m.HourlyRate
            }).ToList();
                
            ViewBag.CarId = new SelectList(cars, "carId", "displayText", order.CarId);
            ViewBag.MasterId = new SelectList(masters, "masterId", "displayText", order.MasterId);
            ViewBag.CarsData = cars;
            
            return View(order);
        }

        /// <summary>
        /// Отображает форму редактирования существующего заказа.
        /// </summary>
        /// <param name="id">Идентификатор редактируемого заказа.</param>
        /// <returns>
        /// Представление формы редактирования заказа.
        /// Если заказ не найден, возвращает NotFound.
        /// </returns>
        /// <remarks>
        /// Загружает текущие данные заказа и подготавливает выпадающие списки:
        /// - Список автомобилей с предварительно выбранным текущим автомобилем
        /// - Список мастеров с предварительно выбранным текущим мастером
        /// </remarks>
        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            
            // Получаем автомобили
            var cars = await _context.Cars
                .Include(c => c.Client)
                .Select(c => new
                {
                    carId = c.CarId,
                    displayText = $"{c.Brand} {c.Model} ({c.LicensePlate}) - {c.Client.FullName}",
                    brand = c.Brand,
                    model = c.Model,
                    licensePlate = c.LicensePlate,
                    photoPath = c.PhotoPath ?? "",
                    clientName = c.Client.FullName,
                    vin = c.Vin ?? "без VIN"
                })
                .ToListAsync();
            
            // Получаем мастеров
            var mastersList = await _context.Masters
                .OrderBy(m => m.FullName)
                .ToListAsync();
            
            var masters = mastersList.Select(m => new
            {
                masterId = m.MasterId,
                displayText = $"{m.FullName} ({m.Position}, {m.Rank} разряд) - {m.HourlyRate:C2}/час",
                fullName = m.FullName,
                position = m.Position,
                rank = m.Rank,
                hourlyRate = m.HourlyRate
            }).ToList();
                
            ViewBag.CarId = new SelectList(cars, "carId", "displayText", order.CarId);
            ViewBag.MasterId = new SelectList(masters, "masterId", "displayText", order.MasterId);
            ViewBag.CarsData = cars;
            
            return View(order);
        }

        /// <summary>
        /// Обрабатывает данные формы редактирования заказа.
        /// </summary>
        /// <param name="id">Идентификатор редактируемого заказа.</param>
        /// <param name="order">Объект заказа с обновленными данными из формы.</param>
        /// <returns>
        /// При успешном обновлении: перенаправление на список заказов.
        /// При ошибках валидации: повторное отображение формы с сообщениями об ошибках.
        /// Если идентификаторы не совпадают, возвращает NotFound.
        /// </returns>
        /// <remarks>
        /// Проверяет соответствие идентификаторов и ModelState.IsValid.
        /// Обрабатывает исключения DbUpdateConcurrencyException.
        /// При ошибке повторно загружает данные для выпадающих списков.
        /// </remarks>
        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderNumber,OrderDate,CarId,MasterId,PaymentDate")] Order order)
        {
            if (id != order.OrderNumber)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderNumber))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            // При ошибке валидации загружаем данные снова
            var cars = await _context.Cars
                .Include(c => c.Client)
                .Select(c => new
                {
                    carId = c.CarId,
                    displayText = $"{c.Brand} {c.Model} ({c.LicensePlate}) - {c.Client.FullName}",
                    brand = c.Brand,
                    model = c.Model,
                    licensePlate = c.LicensePlate,
                    photoPath = c.PhotoPath ?? "",
                    clientName = c.Client.FullName,
                    vin = c.Vin ?? "без VIN"
                })
                .ToListAsync();
            
            // Получаем мастеров
            var mastersList = await _context.Masters
                .OrderBy(m => m.FullName)
                .ToListAsync();
            
            var masters = mastersList.Select(m => new
            {
                masterId = m.MasterId,
                displayText = $"{m.FullName} ({m.Position}, {m.Rank} разряд) - {m.HourlyRate:C2}/час",
                fullName = m.FullName,
                position = m.Position,
                rank = m.Rank,
                hourlyRate = m.HourlyRate
            }).ToList();
                
            ViewBag.CarId = new SelectList(cars, "carId", "displayText", order.CarId);
            ViewBag.MasterId = new SelectList(masters, "masterId", "displayText", order.MasterId);
            ViewBag.CarsData = cars;
            
            return View(order);
        }

        /// <summary>
        /// Отображает страницу подтверждения удаления заказа.
        /// </summary>
        /// <param name="id">Идентификатор удаляемого заказа.</param>
        /// <returns>
        /// Представление подтверждения удаления с информацией о заказе.
        /// Если заказ не найден, возвращает NotFound.
        /// </returns>
        /// <remarks>
        /// Включает информацию об автомобиле и его владельце для подтверждения удаления.
        /// </remarks>
        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Car)
                    .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(m => m.OrderNumber == id);
                
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// Выполняет удаление заказа из базы данных.
        /// </summary>
        /// <param name="id">Идентификатор удаляемого заказа.</param>
        /// <returns>
        /// При успешном удалении: перенаправление на список заказов.
        /// Если заказ не найден, операция пропускается.
        /// </returns>
        /// <remarks>
        /// Удаляет только запись заказа. Связанные CompletedWorks удаляются каскадно,
        /// если настроено каскадное удаление в контексте базы данных.
        /// </remarks>
        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование заказа с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор заказа для проверки.</param>
        /// <returns>
        /// true - если заказ с указанным идентификатором существует,
        /// false - в противном случае.
        /// </returns>
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderNumber == id);
        }
    }
}