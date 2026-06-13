using System.ComponentModel.DataAnnotations;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет услугу, предоставляемую шиномонтажной мастерской.
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Уникальный идентификатор услуги.
        /// </summary>
        [Key]
        [Display(Name = "Код услуги")] 
        public int ServiceCode { get; set; }

        /// <summary>
        /// Название услуги.
        /// </summary>
        [Required(ErrorMessage = "Название услуги обязательно")]
        [Display(Name = "Название услуги")] 
        [StringLength(100, ErrorMessage = "Название услуги не должно превышать 100 символов")]
        public string ServiceName { get; set; } = string.Empty;
        
        /// <summary>
        /// Стоимость услуги в рублях.
        /// </summary>
        [Required(ErrorMessage = "Стоимость услуги обязательна")]
        [Display(Name = "Стоимость услуги")] 
        [Range(0, 1000000, ErrorMessage = "Стоимость должна быть от 0 до 1 000 000 рублей")]
        [DataType(DataType.Currency)]
        public decimal ServiceCost { get; set; }

        /// <summary>
        /// Фиксированное время выполнения услуги в минутах.
        /// Используется для отслеживания скорости работы и расчёта премий.
        /// </summary>
        [Display(Name = "Фикс. время (мин)")]
        [Range(0, 480, ErrorMessage = "Время должно быть от 0 до 480 минут")]
        public int? FixedDurationMin { get; set; }

        /// <summary>
        /// Является ли услуга консультацией (мастер назначит услуги после осмотра).
        /// </summary>
        [Display(Name = "Консультация")]
        public bool IsConsultation { get; set; }

        /// <summary>
        /// Навигационное свойство для выполненных работ с этой услугой.
        /// </summary>
        public ICollection<CompletedWork> CompletedWorks { get; set; } = new List<CompletedWork>();

        /// <summary>
        /// Тарифы по классам автомобилей для этой услуги.
        /// </summary>
        public ICollection<ServiceTariff> ServiceTariffs { get; set; } = new List<ServiceTariff>();

        /// <summary>
        /// Форматированное отображение услуги с ценой.
        /// </summary>
        public override string ToString()
        {
            return $"{ServiceName} - {ServiceCost:C2}";
        }
    }
}