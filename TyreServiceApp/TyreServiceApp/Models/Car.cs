using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет автомобиль в системе шиномонтажа.
    /// Содержит информацию об автомобиле, его владельце и связанных объектах.
    /// </summary>
    /// <remarks>
    /// Этот класс используется для хранения данных об автомобилях в базе данных
    /// и взаимодействия с ними через Entity Framework Core.
    /// </remarks>
    public class Car
    {
        /// <summary>
        /// Уникальный идентификатор автомобиля.
        /// </summary>
        /// <value>
        /// Автоматически генерируемое целочисленное значение, являющееся первичным ключом.
        /// </value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CarId { get; set; }
        
        /// <summary>
        /// Путь к файлу с фотографией автомобиля.
        /// </summary>
        /// <value>
        /// Строка, содержащая относительный путь к изображению в папке wwwroot/uploads/cars/.
        /// Может быть null, если фотография не загружена.
        /// </value>
        [Display(Name = "Фото автомобиля")]
        public string? PhotoPath { get; set; }
        
        /// <summary>
        /// Файл фотографии автомобиля для загрузки.
        /// </summary>
        /// <value>
        /// Объект IFormFile, представляющий загружаемый файл изображения.
        /// Не сохраняется в базе данных (атрибут [NotMapped]).
        /// Используется только при передаче файла через форму.
        /// </value>
        /// <remarks>
        /// Поддерживаются форматы: JPG, PNG, GIF.
        /// Максимальный размер файла: 5 MB.
        /// </remarks>
        [NotMapped]
        [Display(Name = "Загрузить фото")]
        public IFormFile? PhotoFile { get; set; }

        /// <summary>
        /// Идентификатор владельца автомобиля (клиента).
        /// </summary>
        /// <value>
        /// Целочисленное значение, являющееся внешним ключом к таблице Clients.
        /// Обязательное поле.
        /// </value>
        [Required(ErrorMessage = "Выберите клиента")]
        [Display(Name = "Клиент")]
        public int ClientId { get; set; }
        
        /// <summary>
        /// Марка (производитель) автомобиля.
        /// </summary>
        /// <value>
        /// Строка до 50 символов. Обязательное поле.
        /// Примеры: "Toyota", "BMW", "Lada".
        /// </value>
        [Required(ErrorMessage = "Марка обязательна")]
        [StringLength(50, ErrorMessage = "Марка не может превышать 50 символов")]
        [Display(Name = "Марка")]
        public string Brand { get; set; } = string.Empty;
        
        /// <summary>
        /// Модель автомобиля.
        /// </summary>
        /// <value>
        /// Строка до 50 символов. Обязательное поле.
        /// Примеры: "Camry", "X5", "Vesta".
        /// </value>
        [Required(ErrorMessage = "Модель обязательна")]
        [StringLength(50, ErrorMessage = "Модель не может превышать 50 символов")]
        [Display(Name = "Модель")]
        public string Model { get; set; } = string.Empty;
        
        /// <summary>
        /// Год выпуска автомобиля.
        /// </summary>
        /// <value>
        /// Целое число в диапазоне от 1900 до 2100. Обязательное поле.
        /// </value>
        [Required(ErrorMessage = "Год выпуска обязателен")]
        [Range(1900, 2100, ErrorMessage = "Год выпуска должен быть между 1900 и 2100")]
        [Display(Name = "Год выпуска")]
        public int ManufactureYear { get; set; }
        
        /// <summary>
        /// Государственный номерной знак автомобиля.
        /// </summary>
        /// <value>
        /// Строка до 20 символов в российском формате. Обязательное поле.
        /// Примеры: "А123ВС777", "О777ОО177".
        /// </value>
        /// <remarks>
        /// В базе данных имеет уникальное ограничение для предотвращения дублирования.
        /// </remarks>
        [Required(ErrorMessage = "Госномер обязателен")]
        [StringLength(20, ErrorMessage = "Госномер не может превышать 20 символов")]
        [Display(Name = "Госномер")]
        [RegularExpression(@"^[АВЕКМНОРСТУХ]\d{3}[АВЕКМНОРСТУХ]{2}\d{2,3}$", 
            ErrorMessage = "Неверный формат госномера. Пример: А123ВС777")]
        public string LicensePlate { get; set; } = string.Empty;
        
        /// <summary>
        /// VIN-номер (идентификационный номер транспортного средства).
        /// </summary>
        /// <value>
        /// Строка длиной ровно 17 символов. Обязательное поле.
        /// Состоит из латинских букв (кроме I, O, Q) и цифр.
        /// </value>
        /// <remarks>
        /// В базе данных имеет уникальное ограничение.
        /// Является международным стандартом идентификации автомобилей.
        /// </remarks>
        [Required(ErrorMessage = "VIN обязателен")]
        [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN должен содержать ровно 17 символов")]
        [Display(Name = "VIN номер")]
        [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$", 
            ErrorMessage = "Неверный формат VIN. Допустимы латинские буквы (кроме I, O, Q) и цифры")]
        public string Vin { get; set; } = string.Empty;
        
        /// <summary>
        /// Навигационное свойство для доступа к владельцу автомобиля.
        /// </summary>
        /// <value>
        /// Объект типа <see cref="Client"/>, представляющий владельца автомобиля.
        /// Может быть null, если связь не загружена или клиент удален.
        /// </value>
        /// <remarks>
        /// Связывается через внешний ключ <see cref="ClientId"/>.
        /// Используется Entity Framework Core для ленивой загрузки связанных данных.
        /// </remarks>
        [ForeignKey("ClientId")]
        [Display(Name = "Владелец")] 
        public virtual Client? Client { get; set; }

        /// <summary>
        /// Коллекция заказов, связанных с данным автомобилем.
        /// </summary>
        /// <value>
        /// Коллекция объектов <see cref="Order"/>, представляющая историю заказов на автомобиль.
        /// Инициализируется пустым списком для предотвращения NullReferenceException.
        /// </value>
        /// <remarks>
        /// Представляет связь "один-ко-многим" между Car и Order.
        /// Используется для навигации от автомобиля к его заказам.
        /// </remarks>
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        /// <summary>
        /// Коллекция шин, установленных на данный автомобиль.
        /// </summary>
        /// <value>
        /// Коллекция объектов <see cref="Tire"/>, представляющая шины автомобиля.
        /// Инициализируется пустым списком для предотвращения NullReferenceException.
        /// </value>
        /// <remarks>
        /// Представляет связь "один-ко-многим" между Car и Tire.
        /// Используется для отслеживания истории шин автомобиля.
        /// </remarks>
        public virtual ICollection<Tire> Tires { get; set; } = new List<Tire>();

        /// <summary>
        /// Получает полную информацию об автомобиле в формате строки.
        /// </summary>
        /// <returns>
        /// Строка в формате: "Марка Модель (Госномер) [Год]".
        /// Пример: "Toyota Camry (А123ВС777) [2020]".
        /// </returns>
        /// <remarks>
        /// Это вычисляемое свойство, которое не сохраняется в базе данных.
        /// Используется для удобного отображения автомобиля в интерфейсе пользователя.
        /// </remarks>
        [NotMapped]
        [Display(Name = "Автомобиль")]
        public string FullInfo => $"{Brand} {Model} ({LicensePlate}) [{ManufactureYear}]";

        /// <summary>
        /// Проверяет, является ли автомобиль относительно новым (выпущенным за последние 5 лет).
        /// </summary>
        /// <returns>
        /// true, если год выпуска автомобиля не меньше, чем текущий год минус 5 лет;
        /// в противном случае — false.
        /// </returns>
        /// <remarks>
        /// Это вычисляемое свойство, которое не сохраняется в базе данных.
        /// Используется для бизнес-логики, связанной с возрастом автомобиля.
        /// </remarks>
        [NotMapped]
        [Display(Name = "Новый автомобиль")]
        public bool IsNew => ManufactureYear >= DateTime.Now.Year - 5;

        /// <summary>
        /// Получает возраст автомобиля в годах.
        /// </summary>
        /// <returns>
        /// Целое число, представляющее возраст автомобиля в полных годах
        /// на основе текущей даты и года выпуска.
        /// </returns>
        /// <remarks>
        /// Это вычисляемое свойство, которое не сохраняется в базе данных.
        /// Используется для анализа и отчетности.
        /// </remarks>
        [NotMapped]
        [Display(Name = "Возраст автомобиля")]
        public int Age => DateTime.Now.Year - ManufactureYear;
    }
}