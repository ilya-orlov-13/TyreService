using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers
{
    /// <summary>
    /// Контроллер для управления услугами шиномонтажа.
    /// Предоставляет CRUD-операции для работы с услугами.
    /// </summary>
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера ServicesController.
        /// </summary>
        /// <param name="context">Контекст базы данных для работы с услугами.</param>
        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех услуг.
        /// </summary>
        /// <returns>Представление со списком услуг.</returns>
        // GET: Services
        public async Task<IActionResult> Index()
        {
            return View(await _context.Services.ToListAsync());
        }

        /// <summary>
        /// Отображает детальную информацию об услуге.
        /// </summary>
        /// <param name="id">Идентификатор услуги.</param>
        /// <returns>
        /// Представление с деталями услуги, если услуга найдена.
        /// В противном случае возвращает NotFound.
        /// </returns>
        // GET: Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServiceCode == id);
                
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        /// <summary>
        /// Отображает форму для создания новой услуги.
        /// </summary>
        /// <returns>Представление формы создания услуги.</returns>
        // GET: Services/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Обрабатывает данные формы и создает новую услугу.
        /// </summary>
        /// <param name="service">Данные услуги из формы.</param>
        /// <returns>
        /// Перенаправляет на список услуг при успешном создании.
        /// В случае ошибок валидации возвращает форму с сообщениями об ошибках.
        /// </returns>
        // POST: Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceName,ServiceCost")] Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        /// <summary>
        /// Отображает форму для редактирования существующей услуги.
        /// </summary>
        /// <param name="id">Идентификатор редактируемой услуги.</param>
        /// <returns>
        /// Представление формы редактирования услуги, если услуга найдена.
        /// В противном случае возвращает NotFound.
        /// </returns>
        // GET: Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        /// <summary>
        /// Обрабатывает данные формы и обновляет существующую услугу.
        /// </summary>
        /// <param name="id">Идентификатор обновляемой услуги.</param>
        /// <param name="service">Обновленные данные услуги из формы.</param>
        /// <returns>
        /// Перенаправляет на список услуг при успешном обновлении.
        /// В случае ошибок валидации возвращает форму с сообщениями об ошибках.
        /// В случае несоответствия идентификаторов возвращает NotFound.
        /// </returns>
        // POST: Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceCode,ServiceName,ServiceCost")] Service service)
        {
            if (id != service.ServiceCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceCode))
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
            return View(service);
        }

        /// <summary>
        /// Отображает форму подтверждения удаления услуги.
        /// </summary>
        /// <param name="id">Идентификатор удаляемой услуги.</param>
        /// <returns>
        /// Представление подтверждения удаления, если услуга найдена.
        /// В противном случае возвращает NotFound.
        /// </returns>
        // GET: Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServiceCode == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        /// <summary>
        /// Выполняет удаление услуги из базы данных.
        /// </summary>
        /// <param name="id">Идентификатор удаляемой услуги.</param>
        /// <returns>Перенаправляет на список услуг.</returns>
        // POST: Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование услуги с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор проверяемой услуги.</param>
        /// <returns>
        /// true - если услуга с таким идентификатором существует.
        /// false - в противном случае.
        /// </returns>
        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceCode == id);
        }
    }
}