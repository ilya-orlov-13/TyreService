using System.ComponentModel.DataAnnotations;

namespace TyreServiceApp.Models.Api
{
    /// <summary>
    /// Модель запроса для создания автомобиля
    /// </summary>
    public class CreateCarRequest
    {
        [Required(ErrorMessage = "Идентификатор клиента обязателен")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Марка обязательна")]
        [StringLength(50, ErrorMessage = "Марка не может превышать 50 символов")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Модель обязательна")]
        [StringLength(50, ErrorMessage = "Модель не может превышать 50 символов")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Год выпуска обязателен")]
        [Range(1900, 2100, ErrorMessage = "Год выпуска должен быть от 1900 до 2100")]
        public int ManufactureYear { get; set; }

        [Required(ErrorMessage = "Госномер обязателен")]
        [StringLength(20, ErrorMessage = "Госномер не может превышать 20 символов")]
        public string LicensePlate { get; set; } = string.Empty;

        [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN должен содержать 17 символов")]
        public string? Vin { get; set; }

        public string? PhotoPath { get; set; }

        public string? AdditionalPhotos { get; set; }
    }

    /// <summary>
    /// Модель запроса для обновления автомобиля
    /// </summary>
    public class UpdateCarRequest
    {
        [Required(ErrorMessage = "Идентификатор клиента обязателен")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Марка обязательна")]
        [StringLength(50, ErrorMessage = "Марка не может превышать 50 символов")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Модель обязательна")]
        [StringLength(50, ErrorMessage = "Модель не может превышать 50 символов")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Год выпуска обязателен")]
        [Range(1900, 2100, ErrorMessage = "Год выпуска должен быть от 1900 до 2100")]
        public int ManufactureYear { get; set; }

        [Required(ErrorMessage = "Госномер обязателен")]
        [StringLength(20, ErrorMessage = "Госномер не может превышать 20 символов")]
        public string LicensePlate { get; set; } = string.Empty;

        [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN должен содержать 17 символов")]
        public string? Vin { get; set; }

        public string? PhotoPath { get; set; }

        public string? AdditionalPhotos { get; set; }
    }
}
