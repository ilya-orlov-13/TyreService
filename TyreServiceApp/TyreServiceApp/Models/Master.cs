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
        /// Идентификатор должности мастера.
        /// </summary>
        [Display(Name = "Должность")]
        public int PositionId { get; set; }

        /// <summary>
        /// Должность мастера.
        /// </summary>
        [Display(Name = "Должность")]
        public virtual Position? Position { get; set; }

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
        /// Почасовая ставка (не используется в расчётах, сохранена для обратной совместимости).
        /// </summary>
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Почасовая ставка (руб.)")]
        public decimal HourlyRate { get; set; } = 0m;

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
        /// Активные сессии мастера на постах.
        /// </summary>
        public virtual ICollection<PostActiveSession>? PostActiveSessions { get; set; }

        /// <summary>
        /// Коллекция выплат по сдельной оплате.
        /// </summary>
        public virtual ICollection<CompletedJobsPayout>? CompletedJobsPayouts { get; set; }

        /// <summary>
        /// Записи таймера по работам мастера.
        /// </summary>
        public virtual ICollection<WorkTimeLog> WorkTimeLogs { get; set; } = new List<WorkTimeLog>();

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
        public string MasterInfo => $"{FullName} ({Position?.Name}, {Rank} разряд)";

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