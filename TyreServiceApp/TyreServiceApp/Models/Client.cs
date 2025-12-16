using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет клиента шиномонтажной мастерской.
    /// </summary>
    /// <remarks>
    /// Клиент является владельцем одного или нескольких автомобилей,
    /// для которых выполняются заказы на обслуживание.
    /// </remarks>
    public class Client
    {
        /// <summary>
        /// Уникальный идентификатор клиента в системе.
        /// </summary>
        /// <value>
        /// Целочисленное значение, автоматически генерируемое базой данных.
        /// </value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClientId { get; set; }

        /// <summary>
        /// Полное имя клиента.
        /// </summary>
        /// <value>
        /// Строка, содержащая фамилию, имя и отчество клиента.
        /// Максимальная длина: 100 символов.
        /// </value>
        /// <example>Иванов Иван Иванович</example>
        [Required(ErrorMessage = "ФИО обязательно")]
        [StringLength(100, ErrorMessage = "ФИО не должно превышать 100 символов")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Контактный телефон клиента.
        /// </summary>
        /// <value>
        /// Строка, содержащая номер телефона в международном или местном формате.
        /// Максимальная длина: 20 символов.
        /// </value>
        /// <example>+7 (999) 123-45-67</example>
        [Required(ErrorMessage = "Телефон обязателен")]
        [StringLength(20, ErrorMessage = "Телефон не должен превышать 20 символов")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Коллекция автомобилей, принадлежащих клиенту.
        /// </summary>
        /// <value>
        /// Навигационное свойство для доступа к списку автомобилей клиента.
        /// Реализует связь "один ко многим" между клиентом и автомобилями.
        /// </value>
        /// <remarks>
        /// Связь реализована через внешний ключ <see cref="Car.ClientId"/> в таблице Cars.
        /// </remarks>
        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

        /// <summary>
        /// Получает количество автомобилей, принадлежащих клиенту.
        /// </summary>
        /// <value>
        /// Количество автомобилей в коллекции <see cref="Cars"/>.
        /// </value>
        [NotMapped]
        [Display(Name = "Количество автомобилей")]
        public int CarCount => Cars?.Count ?? 0;

        /// <summary>
        /// Возвращает строковое представление клиента.
        /// </summary>
        /// <returns>Строка, содержащая ФИО клиента и его идентификатор.</returns>
        public override string ToString()
        {
            return $"{FullName} (ID: {ClientId})";
        }

        /// <summary>
        /// Проверяет, имеет ли клиент хотя бы один автомобиль.
        /// </summary>
        /// <returns>
        /// <c>true</c>, если у клиента есть хотя бы один автомобиль; иначе <c>false</c>.
        /// </returns>
        public bool HasCars()
        {
            return Cars != null && Cars.Any();
        }

        /// <summary>
        /// Получает форматированный номер телефона для отображения.
        /// </summary>
        /// <returns>
        /// Отформатированная строка с номером телефона или "Не указан", если телефон отсутствует.
        /// </returns>
        public string GetFormattedPhone()
        {
            if (string.IsNullOrWhiteSpace(Phone))
                return "Не указан";

            if (Phone.StartsWith("+7") && Phone.Length == 12)
            {
                return $"+7 ({Phone.Substring(2, 3)}) {Phone.Substring(5, 3)}-{Phone.Substring(8, 2)}-{Phone.Substring(10)}";
            }

            return Phone;
        }
    }
}