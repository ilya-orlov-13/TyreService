using System.ComponentModel.DataAnnotations;

namespace TyreServiceApp.Models.Api
{
    /// <summary>
    /// Модель запроса для создания шины
    /// </summary>
    public class CreateTireRequest
    {
        [Required(ErrorMessage = "Идентификатор автомобиля обязателен")]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Тип шины обязателен")]
        [StringLength(50, ErrorMessage = "Тип шины не может превышать 50 символов")]
        public string TireType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Сезонность обязательна")]
        [StringLength(20, ErrorMessage = "Сезонность не может превышать 20 символов")]
        public string Seasonality { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Производитель не может превышать 50 символов")]
        public string? Manufacturer { get; set; }

        [StringLength(50, ErrorMessage = "Модель шины не может превышать 50 символов")]
        public string? TireModel { get; set; }

        [StringLength(20, ErrorMessage = "Размер не может превышать 20 символов")]
        public string? Size { get; set; }

        [Range(0, 200, ErrorMessage = "Индекс нагрузки должен быть от 0 до 200")]
        public int? LoadIndex { get; set; }

        [Range(0, 100, ErrorMessage = "Процент износа должен быть от 0 до 100")]
        public int? WearPercentage { get; set; }

        [Range(0, 10, ErrorMessage = "Давление должно быть от 0 до 10")]
        public decimal? Pressure { get; set; }
    }

    /// <summary>
    /// Модель запроса для обновления шины
    /// </summary>
    public class UpdateTireRequest
    {
        [Required(ErrorMessage = "Идентификатор автомобиля обязателен")]
        public int CarId { get; set; }

        [Required(ErrorMessage = "Тип шины обязателен")]
        [StringLength(50, ErrorMessage = "Тип шины не может превышать 50 символов")]
        public string TireType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Сезонность обязательна")]
        [StringLength(20, ErrorMessage = "Сезонность не может превышать 20 символов")]
        public string Seasonality { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Производитель не может превышать 50 символов")]
        public string? Manufacturer { get; set; }

        [StringLength(50, ErrorMessage = "Модель шины не может превышать 50 символов")]
        public string? TireModel { get; set; }

        [StringLength(20, ErrorMessage = "Размер не может превышать 20 символов")]
        public string? Size { get; set; }

        [Range(0, 200, ErrorMessage = "Индекс нагрузки должен быть от 0 до 200")]
        public int? LoadIndex { get; set; }

        [Range(0, 100, ErrorMessage = "Процент износа должен быть от 0 до 100")]
        public int? WearPercentage { get; set; }

        [Range(0, 10, ErrorMessage = "Давление должно быть от 0 до 10")]
        public decimal? Pressure { get; set; }
    }
}
