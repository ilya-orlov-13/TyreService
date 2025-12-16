using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет мастера шиномонтажной мастерской.
    /// Содержит информацию о сотруднике, его квалификации и ставке оплаты труда.
    /// </summary>
    public class Master
    {
        /// <summary>
        /// Уникальный идентификатор мастера в системе.
        /// Автоматически генерируется базой данных при добавлении записи.
        /// </summary>
        /// <example>5</example>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MasterId { get; set; }

        /// <summary>
        /// Полное имя мастера.
        /// Обязательное поле. Максимальная длина - 100 символов.
        /// </summary>
        /// <example>Иванов Иван Иванович</example>
        [Required(ErrorMessage = "ФИО обязательно")]
        [StringLength(100, ErrorMessage = "ФИО не более 100 символов")]
        [Display(Name = "ФИО")] 
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Должность мастера в шиномонтаже.
        /// Обязательное поле. Максимальная длина - 50 символов.
        /// </summary>
        /// <example>Мастер по шиномонтажу</example>
        /// <example>Старший механик</example>
        [Required(ErrorMessage = "Должность обязательна")]
        [StringLength(50, ErrorMessage = "Должность не более 50 символов")]
        [Display(Name = "Должность")] 
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// Разряд мастера (уровень квалификации).
        /// Должен быть в диапазоне от 1 до 6 (где 6 - высший разряд).
        /// </summary>
        /// <value>Целое число от 1 до 6</value>
        /// <example>4</example>
        [Required(ErrorMessage = "Разряд обязателен")]
        [Range(1, 6, ErrorMessage = "Разряд должен быть от 1 до 6")]
        [Display(Name = "Разряд")] 
        public int Rank { get; set; }

        /// <summary>
        /// Почасовая ставка оплаты труда мастера.
        /// Обязательное поле. Хранится в формате decimal с 2 знаками после запятой.
        /// </summary>
        /// <value>Положительное число с двумя десятичными знаками</value>
        /// <example>850.50</example>
        [Required(ErrorMessage = "Почасовая ставка обязательна")]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, 10000, ErrorMessage = "Ставка должна быть от 0 до 10 000")]
        [Display(Name = "Почасовая ставка (руб.)")] 
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// Коллекция заказов, закрепленных за мастером.
        /// Навигационное свойство для связи один-ко-многим с сущностью Order.
        /// </summary>
        /// <remarks>
        /// Используется Entity Framework Core для ленивой загрузки связанных заказов.
        /// </remarks>
        [Display(Name = "Заказы")]
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        
        /// <summary>
        /// Коллекция выполненных работ мастером.
        /// Навигационное свойство для связи один-ко-многим с сущностью CompletedWork.
        /// </summary>
        /// <remarks>
        /// Используется для учета выполненных услуг и расчета заработной платы.
        /// </remarks>
        [Display(Name = "Выполненные работы")]
        public virtual ICollection<CompletedWork> CompletedWorks { get; set; } = new List<CompletedWork>();

        /// <summary>
        /// Форматированная строка с полной информацией о мастере.
        /// Вычисляемое свойство, не сохраняется в базе данных.
        /// </summary>
        /// <returns>Строка вида "ФИО (Должность, N разряд)"</returns>
        /// <example>Иванов Иван Иванович (Мастер по шиномонтажу, 4 разряд)</example>
        [NotMapped]
        [Display(Name = "Мастер")]
        public string MasterInfo => $"{FullName} ({Position}, {Rank} разряд)";

        /// <summary>
        /// Форматированная строка почасовой ставки с валютой.
        /// Вычисляемое свойство, не сохраняется в базе данных.
        /// </summary>
        /// <returns>Строка вида "850.50 ₽/час"</returns>
        /// <example>850.50 ₽/час</example>
        [NotMapped]
        [Display(Name = "Ставка")]
        public string FormattedHourlyRate => $"{HourlyRate:C2}/час";
    }
}