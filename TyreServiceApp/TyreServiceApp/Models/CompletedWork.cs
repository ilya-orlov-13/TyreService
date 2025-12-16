using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет выполненную работу в шиномонтажной мастерской.
    /// Связывает заказ, услугу и мастера с информацией о выполненной работе.
    /// </summary>
    /// <remarks>
    /// Эта модель используется для учета выполненных работ, расчета стоимости и анализа производительности.
    /// </remarks>
    public class CompletedWork
    {
        /// <summary>
        /// Уникальный идентификатор выполненной работы.
        /// </summary>
        /// <value>Автоматически генерируемый первичный ключ.</value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkId { get; set; }

        /// <summary>
        /// Номер заказа, к которому относится выполненная работа.
        /// </summary>
        /// <value>Внешний ключ, ссылается на <see cref="Order.OrderNumber"/>.</value>
        [Required(ErrorMessage = "Номер заказа обязателен")]
        [Display(Name = "Номер заказа")] 
        public int OrderNumber { get; set; }

        /// <summary>
        /// Код услуги, которая была выполнена.
        /// </summary>
        /// <value>Внешний ключ, ссылается на <see cref="Service.ServiceCode"/>.</value>
        [Required(ErrorMessage = "Код услуги обязателен")]
        [Display(Name = "Код услуги")] 
        public int ServiceCode { get; set; }

        /// <summary>
        /// Идентификатор мастера, выполнившего работу.
        /// </summary>
        /// <value>Внешний ключ, ссылается на <see cref="Master.MasterId"/>.</value>
        [Required(ErrorMessage = "Мастер обязателен")]
        [Display(Name = "Мастер")] 
        public int MasterId { get; set; }
        
        /// <summary>
        /// Количество колес, обработанных в рамках работы.
        /// </summary>
        /// <value>Целое число от 0 до 4 (для легкового автомобиля).</value>
        [Required(ErrorMessage = "Количество колес обязательно")]
        [Range(0, 4, ErrorMessage = "Количество колес должно быть от 0 до 4")]
        [Display(Name = "Количество колес")] 
        public int WheelCount { get; set; }

        /// <summary>
        /// Время выполнения работы в минутах.
        /// </summary>
        /// <value>Время в минутах, затраченное на выполнение работы.</value>
        [Required(ErrorMessage = "Время выполнения обязательно")]
        [Range(1, 480, ErrorMessage = "Время выполнения должно быть от 1 до 480 минут (8 часов)")]
        [Display(Name = "Время выполнения (минут)")] 
        public int CompletionTimeMin { get; set; }

        /// <summary>
        /// Итоговая стоимость выполненной работы.
        /// </summary>
        /// <value>Стоимость в рублях с двумя знаками после запятой.</value>
        [Required(ErrorMessage = "Стоимость работы обязательна")]
        [Range(0, 1000000, ErrorMessage = "Стоимость работы должна быть от 0 до 1 000 000 руб.")]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Итоговая стоимость (руб.)")] 
        public decimal WorkTotal { get; set; }

        /// <summary>
        /// Навигационное свойство к заказу, к которому относится выполненная работа.
        /// </summary>
        /// <value>Ссылка на объект <see cref="Order"/>.</value>
        [ForeignKey("OrderNumber")]
        [Display(Name = "Заказ")]
        public virtual Order? Order { get; set; }

        /// <summary>
        /// Навигационное свойство к услуге, которая была выполнена.
        /// </summary>
        /// <value>Ссылка на объект <see cref="Service"/>.</value>
        [ForeignKey("ServiceCode")]
        [Display(Name = "Услуга")]
        public virtual Service? Service { get; set; }

        /// <summary>
        /// Навигационное свойство к мастеру, выполнившему работу.
        /// </summary>
        /// <value>Ссылка на объект <see cref="Master"/>.</value>
        [ForeignKey("MasterId")]
        [Display(Name = "Исполнитель")]
        public virtual Master? Master { get; set; }

        /// <summary>
        /// Вычисляет стоимость работы за час.
        /// </summary>
        /// <returns>Стоимость одного часа работы в рублях/час.</returns>
        /// <remarks>
        /// Если время выполнения равно 0, возвращает 0 для предотвращения деления на ноль.
        /// </remarks>
        [NotMapped]
        [Display(Name = "Стоимость часа (руб./час)")]
        public decimal HourlyRate
        {
            get
            {
                if (CompletionTimeMin == 0) return 0;
                return Math.Round((WorkTotal / CompletionTimeMin) * 60, 2);
            }
        }

        /// <summary>
        /// Вычисляет стоимость работы на одно колесо.
        /// </summary>
        /// <returns>Стоимость обработки одного колеса в рублях.</returns>
        /// <remarks>
        /// Если количество колес равно 0, возвращает 0 для предотвращения деления на ноль.
        /// </remarks>
        [NotMapped]
        [Display(Name = "Стоимость колеса (руб./шт.)")]
        public decimal CostPerWheel
        {
            get
            {
                if (WheelCount == 0) return 0;
                return Math.Round(WorkTotal / WheelCount, 2);
            }
        }

    }
}