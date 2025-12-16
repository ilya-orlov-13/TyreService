using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers
{
    /// <summary>
    /// Контроллер для управления выполненными работами в шиномонтажной мастерской.
    /// Обеспечивает CRUD-операции над записями о выполненных работах, включая связь с заказами, услугами и мастерами.
    /// </summary>
    public class CompletedWorksController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера с контекстом базы данных.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения для доступа к сущностям.</param>
        public CompletedWorksController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Возвращает представление со списком всех выполненных работ.
        /// Используется для отображения таблицы выполненных работ.
        /// </summary>
        /// <returns>Представление с коллекцией выполненных работ.</returns>
        // GET: CompletedWorks
        public async Task<IActionResult> Index()
        {
            return View(await _context.CompletedWorks.ToListAsync());
        }

        /// <summary>
        /// Возвращает подробную информацию о конкретной выполненной работе.
        /// Загружает связанные данные: заказ, услугу и мастера.
        /// </summary>
        /// <param name="id">Идентификатор выполненной работы (WorkId).</param>
        /// <returns>
        /// Представление с деталями выполненной работы, либо NotFound, если работа не найдена.
        /// </returns>
        // GET: CompletedWorks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var completedWork = await _context.CompletedWorks
                .Include(cw => cw.Order)
                .Include(cw => cw.Service)
                .Include(cw => cw.Master)
                .FirstOrDefaultAsync(m => m.WorkId == id);
                
            if (completedWork == null)
            {
                return NotFound();
            }

            return View(completedWork);
        }

        /// <summary>
        /// Возвращает форму для создания новой выполненной работы.
        /// Подготавливает выпадающие списки для выбора заказа, услуги и мастера.
        /// </summary>
        /// <returns>Представление формы создания выполненной работы.</returns>
        // GET: CompletedWorks/Create
        public IActionResult Create()
        {
            ViewBag.OrderNumber = new SelectList(_context.Orders, "OrderNumber", "OrderNumber");
            ViewBag.ServiceCode = new SelectList(_context.Services, "ServiceCode", "ServiceName");
            ViewBag.MasterId = new SelectList(_context.Masters, "MasterId", "FullName");
            return View();
        }

        /// <summary>
        /// Обрабатывает POST-запрос для создания новой выполненной работы.
        /// Сохраняет данные о выполненной работе в базу данных после валидации модели.
        /// </summary>
        /// <param name="completedWork">Объект выполненной работы с данными из формы.</param>
        /// <returns>
        /// Перенаправление на список работ при успешном создании,
        /// либо повторное отображение формы с ошибками валидации.
        /// </returns>
        // POST: CompletedWorks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderNumber,ServiceCode,MasterId,WheelCount,CompletionTimeMin,WorkTotal")] CompletedWork completedWork)
        {
            if (ModelState.IsValid)
            {
                _context.Add(completedWork);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.OrderNumber = new SelectList(_context.Orders, "OrderNumber", "OrderNumber", completedWork.OrderNumber);
            ViewBag.ServiceCode = new SelectList(_context.Services, "ServiceCode", "ServiceName", completedWork.ServiceCode);
            ViewBag.MasterId = new SelectList(_context.Masters, "MasterId", "FullName", completedWork.MasterId);
            return View(completedWork);
        }

        /// <summary>
        /// Возвращает форму для редактирования существующей выполненной работы.
        /// </summary>
        /// <param name="id">Идентификатор редактируемой выполненной работы (WorkId).</param>
        /// <returns>
        /// Представление формы редактирования с данными о работе,
        /// либо NotFound, если работа не найдена.
        /// </returns>
        // GET: CompletedWorks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var completedWork = await _context.CompletedWorks.FindAsync(id);
            if (completedWork == null)
            {
                return NotFound();
            }
            ViewBag.OrderNumber = new SelectList(_context.Orders, "OrderNumber", "OrderNumber", completedWork.OrderNumber);
            ViewBag.ServiceCode = new SelectList(_context.Services, "ServiceCode", "ServiceName", completedWork.ServiceCode);
            ViewBag.MasterId = new SelectList(_context.Masters, "MasterId", "FullName", completedWork.MasterId);
            return View(completedWork);
        }

        /// <summary>
        /// Обрабатывает POST-запрос для обновления данных о выполненной работе.
        /// Проверяет соответствие идентификатора и валидность данных перед сохранением.
        /// </summary>
        /// <param name="id">Идентификатор редактируемой выполненной работы (WorkId).</param>
        /// <param name="completedWork">Объект с обновленными данными выполненной работы.</param>
        /// <returns>
        /// Перенаправление на список работ при успешном обновлении,
        /// либо повторное отображение формы с ошибками валидации.
        /// </returns>
        // POST: CompletedWorks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkId,OrderNumber,ServiceCode,MasterId,WheelCount,CompletionTimeMin,WorkTotal")] CompletedWork completedWork)
        {
            if (id != completedWork.WorkId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(completedWork);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompletedWorkExists(completedWork.WorkId))
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
            ViewBag.OrderNumber = new SelectList(_context.Orders, "OrderNumber", "OrderNumber", completedWork.OrderNumber);
            ViewBag.ServiceCode = new SelectList(_context.Services, "ServiceCode", "ServiceName", completedWork.ServiceCode);
            ViewBag.MasterId = new SelectList(_context.Masters, "MasterId", "FullName", completedWork.MasterId);
            return View(completedWork);
        }

        /// <summary>
        /// Возвращает форму подтверждения удаления выполненной работы.
        /// Загружает связанные данные для отображения в форме подтверждения.
        /// </summary>
        /// <param name="id">Идентификатор удаляемой выполненной работы (WorkId).</param>
        /// <returns>
        /// Представление подтверждения удаления с данными о работе,
        /// либо NotFound, если работа не найдена.
        /// </returns>
        // GET: CompletedWorks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var completedWork = await _context.CompletedWorks
                .Include(cw => cw.Order)
                .Include(cw => cw.Service)
                .Include(cw => cw.Master)
                .FirstOrDefaultAsync(m => m.WorkId == id);
            if (completedWork == null)
            {
                return NotFound();
            }

            return View(completedWork);
        }

        /// <summary>
        /// Обрабатывает POST-запрос для удаления выполненной работы из базы данных.
        /// </summary>
        /// <param name="id">Идентификатор удаляемой выполненной работы (WorkId).</param>
        /// <returns>Перенаправление на список работ после успешного удаления.</returns>
        // POST: CompletedWorks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var completedWork = await _context.CompletedWorks.FindAsync(id);
            if (completedWork != null)
            {
                _context.CompletedWorks.Remove(completedWork);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование выполненной работы с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор проверяемой выполненной работы (WorkId).</param>
        /// <returns>true, если работа существует; иначе false.</returns>
        private bool CompletedWorkExists(int id)
        {
            return _context.CompletedWorks.Any(e => e.WorkId == id);
        }
    }
}