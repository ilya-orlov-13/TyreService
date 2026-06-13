using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет шину автомобиля в системе шиномонтажа.
    /// </summary>
    public class Tire
    {
        /// <summary>
        /// Уникальный идентификатор шины.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TireId { get; set; }

        /// <summary>
        /// Идентификатор автомобиля, к которому привязана шина (null для шин на хранении).
        /// </summary>
        public int? CarId { get; set; }

        /// <summary>
        /// Идентификатор клиента для шин на хранении (null для шин, привязанных к авто).
        /// </summary>
        public int? ClientId { get; set; }

        /// <summary>
        /// Тип шины в зависимости от типа транспортного средства.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TireType { get; set; } = string.Empty;

        /// <summary>
        /// Сезонность шины.
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Сезонность")]
        public string Seasonality { get; set; } = string.Empty;

        /// <summary>
        /// Производитель шины.
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Производитель")]
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// Модель шины от производителя.
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Модель шины")]
        public string TireModel { get; set; } = string.Empty;

        /// <summary>
        /// Размер шины в стандартном обозначении.
        /// </summary>
        [Required]
        [StringLength(20)]
        [Display(Name = "Размер")]
        public string Size { get; set; } = string.Empty;

        /// <summary>
        /// Индекс нагрузки шины.
        /// </summary>
        public int LoadIndex { get; set; }

        /// <summary>
        /// Процент износа протектора шины.
        /// </summary>
        [Display(Name = "Износ")] 
        [Range(0, 100, ErrorMessage = "Износ должен быть в диапазоне от 0 до 100%")]
        public int WearPercentage { get; set; }

        /// <summary>
        /// Давление в шине в барах.
        /// </summary>
        [Column(TypeName = "decimal(3, 1)")]
        [Range(0.0, 10.0, ErrorMessage = "Давление должно быть в диапазоне от 0 до 10 бар")]
        [Display(Name = "Давление")]
        public decimal Pressure { get; set; }

        /// <summary>
        /// Навигационное свойство для доступа к автомобилю.
        /// </summary>
        public Car? Car { get; set; }

        /// <summary>
        /// Навигационное свойство для доступа к клиенту (для шин на хранении).
        /// </summary>
        public Client? Client { get; set; }

        /// <summary>
        /// Полное описание шины для отображения.
        /// </summary>
        [NotMapped]
        public string FullInfo => $"{Manufacturer} {TireModel} ({Size}, {Seasonality})";
    }
}