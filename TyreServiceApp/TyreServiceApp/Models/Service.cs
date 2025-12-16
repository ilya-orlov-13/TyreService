using System.ComponentModel.DataAnnotations;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет услугу, предоставляемую шиномонтажной мастерской
    /// </summary>
    /// <remarks>
    /// Класс содержит информацию об услугах, которые могут быть выполнены в заказе.
    /// Каждая услуга имеет уникальный код, название и стоимость.
    /// </remarks>
    /// <example>
    /// Пример создания услуги:
    /// <code>
    /// var service = new Service
    /// {
    ///     ServiceCode = 1,
    ///     ServiceName = "Замена летних шин на зимние",
    ///     ServiceCost = 2500.00m
    /// };
    /// </code>
    /// </example>
    public class Service
    {
        /// <summary>
        /// Уникальный идентификатор услуги
        /// </summary>
        /// <value>
        /// Целочисленное значение, автоматически генерируемое базой данных
        /// </value>
        /// <remarks>
        /// Используется как первичный ключ в таблице услуг
        /// </remarks>
        [Key]
        [Display(Name = "Код услуги")] 
        public int ServiceCode { get; set; }

        /// <summary>
        /// Название услуги
        /// </summary>
        /// <value>
        /// Строковое значение, описывающее тип предоставляемой услуги
        /// </value>
        /// <remarks>
        /// Обязательное поле. Максимальная длина не ограничена атрибутами,
        /// но рекомендуется использовать не более 100 символов
        /// </remarks>
        [Required(ErrorMessage = "Название услуги обязательно")]
        [Display(Name = "Название услуги")] 
        [StringLength(100, ErrorMessage = "Название услуги не должно превышать 100 символов")]
        public string ServiceName { get; set; } = string.Empty;
        
        /// <summary>
        /// Стоимость услуги в рублях
        /// </summary>
        /// <value>
        /// Десятичное значение с двумя знаками после запятой
        /// </value>
        /// <remarks>
        /// Хранится в базе данных как decimal(10,2).
        /// Для отображения использует формат валюты
        /// </remarks>
        [Required(ErrorMessage = "Стоимость услуги обязательна")]
        [Display(Name = "Стоимость услуги")] 
        [Range(0, 1000000, ErrorMessage = "Стоимость должна быть от 0 до 1 000 000 рублей")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal ServiceCost { get; set; }

        /// <summary>
        /// Форматированное отображение услуги с ценой
        /// </summary>
        /// <returns>
        /// Строка вида "Название услуги - 2 500,00 ₽"
        /// </returns>
        /// <example>
        /// "Замена шин - 2 500,00 ₽"
        /// </example>
        public override string ToString()
        {
            return $"{ServiceName} - {ServiceCost:C2}";
        }
    }
}