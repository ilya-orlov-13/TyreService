using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;

namespace TyreServiceApp.Controllers
{
    /// <summary>
    /// Контроллер для управления клиентами шиномонтажной мастерской.
    /// Предоставляет CRUD-операции для работы с данными клиентов.
    /// </summary>
    /// <remarks>
    /// Этот контроллер обрабатывает все HTTP-запросы, связанные с клиентами:
    /// просмотр списка, создание, редактирование, просмотр деталей и удаление.
    /// </remarks>
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ClientsController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения для доступа к данным клиентов.</param>
        /// <remarks>
        /// Использует Dependency Injection для получения контекста базы данных.
        /// </remarks>
        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отображает список всех клиентов.
        /// </summary>
        /// <returns>Представление с коллекцией всех клиентов.</returns>
        /// <remarks>
        /// HTTP GET: /Clients
        /// Загружает всех клиентов из базы данных и передает их в представление.
        /// </remarks>
        /// <response code="200">Успешное выполнение. Возвращает список клиентов.</response>
        // GET: Clients
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clients.ToListAsync());
        }

        /// <summary>
        /// Отображает подробную информацию о конкретном клиенте.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <returns>
        /// Представление с деталями клиента или результат NotFound, если клиент не найден.
        /// </returns>
        /// <remarks>
        /// HTTP GET: /Clients/Details/5
        /// Загружает клиента по идентификатору вместе с его автомобилями.
        /// Если клиент не найден, возвращается HTTP 404.
        /// </remarks>
        /// <response code="200">Клиент найден. Возвращает детальную информацию.</response>
        /// <response code="404">Клиент с указанным идентификатором не найден.</response>
        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Cars)
                .FirstOrDefaultAsync(m => m.ClientId == id);
                
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        /// <summary>
        /// Отображает форму для создания нового клиента.
        /// </summary>
        /// <returns>Представление с формой создания клиента.</returns>
        /// <remarks>
        /// HTTP GET: /Clients/Create
        /// Возвращает пустую форму для ввода данных нового клиента.
        /// </remarks>
        /// <response code="200">Успешное выполнение. Возвращает форму создания.</response>
        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Обрабатывает данные формы для создания нового клиента.
        /// </summary>
        /// <param name="client">Данные клиента из формы.</param>
        /// <returns>
        /// Перенаправление на список клиентов при успешном создании 
        /// или повторное отображение формы с ошибками валидации.
        /// </returns>
        /// <remarks>
        /// HTTP POST: /Clients/Create
        /// Принимает данные из формы, выполняет валидацию и сохраняет нового клиента в базу данных.
        /// </remarks>
        /// <response code="302">Клиент успешно создан. Перенаправление на список клиентов.</response>
        /// <response code="200">Ошибки валидации. Возвращает форму с сообщениями об ошибках.</response>
        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Phone")] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        /// <summary>
        /// Отображает форму для редактирования существующего клиента.
        /// </summary>
        /// <param name="id">Идентификатор редактируемого клиента.</param>
        /// <returns>
        /// Представление с формой редактирования или результат NotFound, если клиент не найден.
        /// </returns>
        /// <remarks>
        /// HTTP GET: /Clients/Edit/5
        /// Загружает клиента по идентификатору и передает его в форму редактирования.
        /// </remarks>
        /// <response code="200">Клиент найден. Возвращает форму редактирования.</response>
        /// <response code="404">Клиент с указанным идентификатором не найден.</response>
        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        /// <summary>
        /// Обрабатывает данные формы для обновления информации о клиенте.
        /// </summary>
        /// <param name="id">Идентификатор клиента.</param>
        /// <param name="client">Обновленные данные клиента.</param>
        /// <returns>
        /// Перенаправление на список клиентов при успешном обновлении,
        /// результат NotFound при несоответствии идентификаторов или 
        /// повторное отображение формы с ошибками валидации.
        /// </returns>
        /// <remarks>
        /// HTTP POST: /Clients/Edit/5
        /// Принимает данные из формы редактирования и обновляет информацию о клиенте в базе данных.
        /// </remarks>
        /// <response code="302">Клиент успешно обновлен. Перенаправление на список клиентов.</response>
        /// <response code="404">Клиент не найден или несоответствие идентификаторов.</response>
        /// <response code="200">Ошибки валидации. Возвращает форму с сообщениями об ошибках.</response>
        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,FullName,Phone")] Client client)
        {
            if (id != client.ClientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.ClientId))
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
            return View(client);
        }

        /// <summary>
        /// Отображает форму подтверждения удаления клиента.
        /// </summary>
        /// <param name="id">Идентификатор клиента для удаления.</param>
        /// <returns>
        /// Представление с информацией о клиенте и подтверждением удаления 
        /// или результат NotFound, если клиент не найден.
        /// </returns>
        /// <remarks>
        /// HTTP GET: /Clients/Delete/5
        /// Загружает клиента для отображения информации перед удалением.
        /// Пользователь должен подтвердить удаление.
        /// </remarks>
        /// <response code="200">Клиент найден. Возвращает форму подтверждения удаления.</response>
        /// <response code="404">Клиент с указанным идентификатором не найден.</response>
        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        /// <summary>
        /// Выполняет удаление клиента из базы данных.
        /// </summary>
        /// <param name="id">Идентификатор клиента для удаления.</param>
        /// <returns>Перенаправление на список клиентов.</returns>
        /// <remarks>
        /// HTTP POST: /Clients/Delete/5
        /// Удаляет клиента по идентификатору. Если клиент не найден, операция игнорируется.
        /// После удаления выполняется перенаправление на страницу со списком клиентов.
        /// </remarks>
        /// <response code="302">Клиент успешно удален (или не найден). Перенаправление на список клиентов.</response>
        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Проверяет существование клиента с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор клиента для проверки.</param>
        /// <returns>
        /// true, если клиент с указанным идентификатором существует; в противном случае - false.
        /// </returns>
        /// <remarks>
        /// Вспомогательный метод для проверки существования клиента в базе данных.
        /// Используется для обработки исключений конкурентного доступа.
        /// </remarks>
        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }
    }
}