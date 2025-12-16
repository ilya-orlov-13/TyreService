using System;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Модель представления для отображения информации об ошибке в приложении.
    /// Используется для передачи данных об ошибках из контроллеров в представления.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Этот класс является стандартным для ASP.NET Core приложений и используется
    /// для страниц ошибок (например, Error.cshtml).
    /// </para>
    /// <para>
    /// Модель содержит идентификатор запроса для отслеживания ошибок в логах и
    /// флаг для управления отображением этого идентификатора в пользовательском интерфейсе.
    /// </para>
    /// </remarks>
    /// <example>
    /// Пример использования в контроллере:
    /// <code>
    /// public IActionResult Error()
    /// {
    ///     var errorViewModel = new ErrorViewModel
    ///     {
    ///         RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    ///     };
    ///     return View(errorViewModel);
    /// }
    /// </code>
    /// </example>
    public class ErrorViewModel
    {
        /// <summary>
        /// Получает или задает уникальный идентификатор запроса, связанного с ошибкой.
        /// </summary>
        /// <value>
        /// Строка, содержащая идентификатор запроса. Может быть <c>null</c> или пустой строкой,
        /// если идентификатор недоступен.
        /// </value>
        /// <remarks>
        /// <para>
        /// Обычно это значение берется из <c>HttpContext.TraceIdentifier</c> или 
        /// <c>System.Diagnostics.Activity.Current.Id</c>.
        /// </para>
        /// <para>
        /// Идентификатор запроса полезен для отслеживания ошибок в системах логирования,
        /// позволяя найти конкретный запрос в логах сервера.
        /// </para>
        /// </remarks>
        public string? RequestId { get; set; }

        /// <summary>
        /// Получает значение, указывающее, следует ли отображать идентификатор запроса
        /// в пользовательском интерфейсе.
        /// </summary>
        /// <value>
        /// <c>true</c>, если идентификатор запроса существует и не является пустой строкой;
        /// в противном случае — <c>false</c>.
        /// </value>
        /// <remarks>
        /// Это свойство только для чтения, вычисляемое на основе значения <see cref="RequestId"/>.
        /// Используется в представлениях для условного отображения идентификатора запроса.
        /// </remarks>
        /// <example>
        /// Пример использования в Razor-представлении:
        /// <code>
        /// @if (Model.ShowRequestId)
        /// {
        ///     <p>
        ///         <strong>Request ID:</strong> <code>@Model.RequestId</code>
        ///     </p>
        /// }
        /// </code>
        /// </example>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}