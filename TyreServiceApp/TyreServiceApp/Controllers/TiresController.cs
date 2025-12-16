using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers
{
    /// <summary>
    /// Контроллер для управления шинами в системе шиномонтажа.
    /// Обеспечивает CRUD-операции для сущности Tire.
    /// </summary>
    public class TiresController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр контроллера TiresController.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        public TiresController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех шин в системе.
        /// </summary>
        /// <returns>Представление со списком шин.</returns>
        // GET: Tires
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tires.ToListAsync());
        }

        /// <summary>
        /// Отображает детальную информацию о конкретной шине.
        /// </summary>
        /// <param name="id">Идентификатор шины (TireId).</param>
        /// <returns>
        /// Представление с детальной информацией о шине.
        /// Если шина не найдена, возвращает NotFound.
        /// </returns>
        // GET: Tires/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tire = await _context.Tires
                .Include(t => t.Car)
                .FirstOrDefaultAsync(m => m.TireId == id);
                
            if (tire == null)
            {
                return NotFound();
            }

            return View(tire);
        }

        /// <summary>
        /// Отображает форму для создания новой шины.
        /// </summary>
        /// <returns>Представление с формой создания шины.</returns>
        // GET: Tires/Create
        public IActionResult Create()
        {
            ViewBag.CarId = new SelectList(_context.Cars, "CarId", "LicensePlate");
            return View();
        }

        /// <summary>
        /// Обрабатывает данные формы и создает новую шину.
        /// </summary>
        /// <param name="tire">Данные шины из формы.</param>
        /// <returns>
        /// При успешном создании перенаправляет на список шин.
        /// При ошибках валидации возвращает форму с сообщениями об ошибках.
        /// </returns>
        // POST: Tires/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CarId,TireType,Seasonality,Manufacturer,TireModel,Size,LoadIndex,WearPercentage,Pressure")] Tire tire)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tire);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CarId = new SelectList(_context.Cars, "CarId", "LicensePlate", tire.CarId);
            return View(tire);
        }

        /// <summary>
        /// Отображает форму для редактирования существующей шины.
        /// </summary>
        /// <param name="id">Идентификатор шины (TireId).</param>
        /// <returns>
        /// Представление с формой редактирования шины.
        /// Если шина не найдена, возвращает NotFound.
        /// </returns>
        // GET: Tires/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tire = await _context.Tires.FindAsync(id);
            if (tire == null)
            {
                return NotFound();
            }
            ViewBag.CarId = new SelectList(_context.Cars, "CarId", "LicensePlate", tire.CarId);
            return View(tire);
        }

        /// <summary>
        /// Обрабатывает данные формы и обновляет существующую шину.
        /// </summary>
        /// <param name="id">Идентификатор шины (TireId).</param>
        /// <param name="tire">Обновленные данные шины из формы.</param>
        /// <returns>
        /// При успешном обновлении перенаправляет на список шин.
        /// При ошибках валидации возвращает форму с сообщениями об ошибках.
        /// Если идентификаторы не совпадают, возвращает NotFound.
        /// </returns>
        // POST: Tires/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TireId,CarId,TireType,Seasonality,Manufacturer,TireModel,Size,LoadIndex,WearPercentage,Pressure")] Tire tire)
        {
            if (id != tire.TireId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tire);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TireExists(tire.TireId))
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
            ViewBag.CarId = new SelectList(_context.Cars, "CarId", "LicensePlate", tire.CarId);
            return View(tire);
        }

        /// <summary>
        /// Отображает подтверждение удаления шины.
        /// </summary>
        /// <param name="id">Идентификатор шины (TireId).</param>
        /// <returns>
        /// Представление с подтверждением удаления шины.
        /// Если шина не найдена, возвращает NotFound.
        /// </returns>
        // GET: Tires/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tire = await _context.Tires
                .Include(t => t.Car)
                .FirstOrDefaultAsync(m => m.TireId == id);
            if (tire == null)
            {
                return NotFound();
            }

            return View(tire);
        }

        /// <summary>
        /// Выполняет удаление шины из системы.
        /// </summary>
        /// <param name="id">Идентификатор шины (TireId).</param>
        /// <returns>Перенаправляет на список шин после успешного удаления.</returns>
        // POST: Tires/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tire = await _context.Tires.FindAsync(id);
            if (tire != null)
            {
                _context.Tires.Remove(tire);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование шины с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор шины (TireId).</param>
        /// <returns>True, если шина существует, иначе False.</returns>
        private bool TireExists(int id)
        {
            return _context.Tires.Any(e => e.TireId == id);
        }
    }
}