using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет шину автомобиля в системе шиномонтажа.
    /// Содержит информацию о типе, характеристиках и состоянии шины.
    /// </summary>
    public class Tire
    {
        /// <summary>
        /// Уникальный идентификатор шины.
        /// Автоматически генерируется базой данных при создании записи.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TireId { get; set; }

        /// <summary>
        /// Идентификатор автомобиля, к которому привязана шина.
        /// Связывает шину с конкретным автомобилем в системе.
        /// </summary>
        [Required]
        public int CarId { get; set; }

        /// <summary>
        /// Тип шины в зависимости от типа транспортного средства.
        /// Примеры: "Легковая", "Внедорожник", "Грузовая", "Минивен".
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TireType { get; set; } = string.Empty;

        /// <summary>
        /// Сезонность шины, определяющая время года для эксплуатации.
        /// Примеры: "Летняя", "Зимняя", "Всесезонная".
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Сезонность")] 
        public string Seasonality { get; set; } = string.Empty;

        /// <summary>
        /// Производитель шины.
        /// Примеры: "Michelin", "Bridgestone", "Goodyear", "Nokian".
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Производитель")] 
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// Модель шины от производителя.
        /// Примеры: "Pilot Sport 4", "Blizzak", "EfficientGrip".
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Модель шины")] 
        public string TireModel { get; set; } = string.Empty;

        /// <summary>
        /// Размер шины в стандартном обозначении.
        /// Формат: "ширина/профиль R диаметр" (пример: "225/45 R17").
        /// </summary>
        [Required]
        [StringLength(20)]
        [Display(Name = "Размер")] 
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// Индекс нагрузки шины, указывающий максимальную нагрузку в килограммах.
        /// Чем выше индекс, тем большую нагрузку может выдерживать шина.
        /// Пример: 91 (615 кг), 95 (690 кг).
        /// </summary>
        public int LoadIndex { get; set; }

        /// <summary>
        /// Процент износа протектора шины.
        /// Диапазон значений: 0% (новая шина) - 100% (полный износ).
        /// </summary>
        [Display(Name = "Износ")] 
        [Range(0, 100, ErrorMessage = "Износ должен быть в диапазоне от 0 до 100%")]
        public int WearPercentage { get; set; }

        /// <summary>
        /// Давление в шине в барах (bar).
        /// Рекомендуемое значение зависит от размера шины и автомобиля.
        /// Пример: 2.2, 2.5, 3.0.
        /// </summary>
        [Column(TypeName = "decimal(3, 1)")]
        [Range(0.0, 10.0, ErrorMessage = "Давление должно быть в диапазоне от 0 до 10 бар")]
        [Display(Name = "Давление (бар)")]
        public decimal Pressure { get; set; }

        /// <summary>
        /// Навигационное свойство для доступа к автомобилю, к которому привязана шина.
        /// Позволяет получать информацию об автомобиле через свойство Car.
        /// </summary>
        [ForeignKey("CarId")]
        [Display(Name = "Автомобиль")]
        public virtual Car? Car { get; set; }

        /// <summary>
        /// Вычисляемое свойство, возвращающее полное название шины.
        /// Не сохраняется в базе данных (атрибут [NotMapped]).
        /// </summary>
        [NotMapped]
        [Display(Name = "Шина")]
        public string FullTireName => $"{Manufacturer} {TireModel} {Size}";

        /// <summary>
        /// Вычисляемое свойство, возвращающее информацию о состоянии шины.
        /// Определяет состояние на основе процента износа.
        /// Не сохраняется в базе данных.
        /// </summary>
        [NotMapped]
        [Display(Name = "Состояние")]
        public string Condition
        {
            get
            {
                return WearPercentage switch
                {
                    < 20 => "Отличное",
                    < 50 => "Хорошее",
                    < 80 => "Среднее",
                    _ => "Требует замены"
                };
            }
        }

        /// <summary>
        /// Вычисляемое свойство, возвращающее рекомендацию по давлению.
        /// Сравнивает текущее давление с рекомендуемым диапазоном.
        /// Не сохраняется в базе данных.
        /// </summary>
        [NotMapped]
        [Display(Name = "Рекомендация по давлению")]
        public string PressureRecommendation
        {
            get
            {
                // Примерная логика рекомендации
                // В реальном приложении можно добавить более сложные расчеты
                if (Pressure < 1.8m) return "Низкое - требуется подкачка";
                if (Pressure > 3.0m) return "Высокое - требуется стравливание";
                return "Нормальное";
            }
        }

        /// <summary>
        /// Проверяет, является ли шина зимней.
        /// </summary>
        /// <returns>true, если шина зимняя; иначе false.</returns>
        public bool IsWinterTire() => 
            Seasonality.Contains("Зим") || Seasonality.Contains("Winter", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Проверяет, является ли шина летней.
        /// </summary>
        /// <returns>true, если шина летняя; иначе false.</returns>
        public bool IsSummerTire() => 
            Seasonality.Contains("Лет") || Seasonality.Contains("Summer", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Проверяет, является ли шина всесезонной.
        /// </summary>
        /// <returns>true, если шина всесезонная; иначе false.</returns>
        public bool IsAllSeasonTire() => 
            Seasonality.Contains("Всесезон") || Seasonality.Contains("All-Season", StringComparison.OrdinalIgnoreCase);
    }
}