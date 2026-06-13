using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет клиента шиномонтажной мастерской.
    /// </summary>
    /// <remarks>
    /// Клиент является владельцем одного или нескольких автомобилей,
    /// для которых выполняются заказы на обслуживание.
    /// </remarks>
    [Index(nameof(Phone), IsUnique = true)]
    public class Client : IValidatableObject
    {
        /// <summary>
        /// Уникальный идентификатор клиента в системе.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClientId { get; set; }

        /// <summary>
        /// Полное имя клиента.
        /// </summary>
        [Required(ErrorMessage = "ФИО обязательно")]
        [StringLength(100, ErrorMessage = "ФИО не должно превышать 100 символов")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Контактный телефон клиента.
        /// </summary>
        [StringLength(100, ErrorMessage = "Телефон не должен превышать 100 символов")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        /// <summary>
        /// Адрес электронной почты клиента.
        /// </summary>
        [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        /// <summary>
        /// Коллекция автомобилей, принадлежащих клиенту.
        /// </summary>
        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

        /// <summary>
        /// Коллекция шин на хранении, принадлежащих клиенту.
        /// </summary>
        public virtual ICollection<Tire> Tires { get; set; } = new List<Tire>();

        /// <summary>
        /// Получает количество автомобилей, принадлежащих клиенту.
        /// </summary>
        [NotMapped]
        [Display(Name = "Количество автомобилей")]
        public int CarCount => Cars?.Count ?? 0;

        /// <summary>
        /// Возвращает строковое представление клиента.
        /// </summary>
        public override string ToString()
        {
            return $"{FullName} (ID: {ClientId})";
        }

        /// <summary>
        /// Проверяет, имеет ли клиент хотя бы один автомобиль.
        /// </summary>
        public bool HasCars()
        {
            return Cars != null && Cars.Any();
        }

        /// <summary>
        /// Получает форматированный номер телефона для отображения.
        /// </summary>
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Phone) && string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(
                    "Необходимо указать хотя бы телефон или email",
                    new[] { nameof(Phone), nameof(Email) });
            }
        }
    }
}