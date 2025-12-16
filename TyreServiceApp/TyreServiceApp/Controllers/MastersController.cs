using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers
{
    /// <summary>
    /// Контроллер для управления мастерами шиномонтажной мастерской.
    /// Предоставляет CRUD-операции для работы с данными о мастерах.
    /// </summary>
    public class MastersController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера MastersController.
        /// </summary>
        /// <param name="context">Контекст базы данных для работы с сущностями.</param>
        public MastersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех мастеров.
        /// GET: /Masters
        /// </summary>
        /// <returns>Представление Index со списком мастеров.</returns>
        public async Task<IActionResult> Index()
        {
            return View(await _context.Masters.ToListAsync());
        }

        /// <summary>
        /// Отображает детальную информацию о конкретном мастере.
        /// GET: /Masters/Details/{id}
        /// </summary>
        /// <param name="id">Идентификатор мастера. Если равен null, возвращает NotFound.</param>
        /// <returns>
        /// - Представление Details с информацией о мастере, если найден.
        /// - NotFound, если мастер не найден или id равен null.
        /// </returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var master = await _context.Masters
                .FirstOrDefaultAsync(m => m.MasterId == id);
                
            if (master == null)
            {
                return NotFound();
            }

            return View(master);
        }

        /// <summary>
        /// Отображает форму для создания нового мастера.
        /// GET: /Masters/Create
        /// </summary>
        /// <returns>Представление Create с пустой формой.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Обрабатывает данные формы для создания нового мастера.
        /// POST: /Masters/Create
        /// </summary>
        /// <param name="master">Данные мастера из формы, связанные с моделью.</param>
        /// <returns>
        /// - Перенаправление на Index при успешном создании.
        /// - Представление Create с ошибками валидации, если данные неверны.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Position,Rank,HourlyRate")] Master master)
        {
            if (ModelState.IsValid)
            {
                _context.Add(master);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(master);
        }

        /// <summary>
        /// Отображает форму для редактирования данных мастера.
        /// GET: /Masters/Edit/{id}
        /// </summary>
        /// <param name="id">Идентификатор редактируемого мастера. Если равен null, возвращает NotFound.</param>
        /// <returns>
        /// - Представление Edit с данными мастера, если найден.
        /// - NotFound, если мастер не найден или id равен null.
        /// </returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var master = await _context.Masters.FindAsync(id);
            if (master == null)
            {
                return NotFound();
            }
            return View(master);
        }

        /// <summary>
        /// Обрабатывает данные формы для обновления информации о мастере.
        /// POST: /Masters/Edit/{id}
        /// </summary>
        /// <param name="id">Идентификатор мастера из URL.</param>
        /// <param name="master">Обновленные данные мастера из формы.</param>
        /// <returns>
        /// - Перенаправление на Index при успешном обновлении.
        /// - NotFound, если id не совпадает с MasterId.
        /// - Представление Edit с ошибками валидации, если данные неверны.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MasterId,FullName,Position,Rank,HourlyRate")] Master master)
        {
            if (id != master.MasterId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(master);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MasterExists(master.MasterId))
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
            return View(master);
        }

        /// <summary>
        /// Отображает страницу подтверждения удаления мастера.
        /// GET: /Masters/Delete/{id}
        /// </summary>
        /// <param name="id">Идентификатор мастера для удаления. Если равен null, возвращает NotFound.</param>
        /// <returns>
        /// - Представление Delete с информацией о мастере, если найден.
        /// - NotFound, если мастер не найден или id равен null.
        /// </returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var master = await _context.Masters
                .FirstOrDefaultAsync(m => m.MasterId == id);
            if (master == null)
            {
                return NotFound();
            }

            return View(master);
        }

        /// <summary>
        /// Выполняет удаление мастера из базы данных.
        /// POST: /Masters/Delete/{id}
        /// </summary>
        /// <param name="id">Идентификатор удаляемого мастера.</param>
        /// <returns>Перенаправление на Index после успешного удаления.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var master = await _context.Masters.FindAsync(id);
            if (master != null)
            {
                _context.Masters.Remove(master);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование мастера с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор мастера для проверки.</param>
        /// <returns>
        /// true - если мастер с таким идентификатором существует в базе данных.
        /// false - если мастер не найден.
        /// </returns>
        private bool MasterExists(int id)
        {
            return _context.Masters.Any(e => e.MasterId == id);
        }
    }
}