using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    /// <summary>
    /// Представляет заказ на выполнение работ в шиномонтажной мастерской.
    /// </summary>
    /// <remarks>
    /// Класс <see cref="Order"/> является центральной сущностью системы,
    /// связывающей клиентов, автомобили, мастеров и выполненные работы.
    /// Каждый заказ содержит информацию о дате создания, ответственном мастере,
    /// автомобиле и статусе оплаты.
    /// </remarks>
    public class Order
    {
        /// <summary>
        /// Уникальный идентификатор заказа.
        /// </summary>
        /// <value>
        /// Автоматически генерируемый числовой идентификатор, используемый
        /// как первичный ключ в таблице Orders базы данных.
        /// </value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Номер заказа")] 
        public int OrderNumber { get; set; }

        /// <summary>
        /// Дата и время создания заказа.
        /// </summary>
        /// <value>
        /// Значение <see cref="DateTime"/>, указывающее когда был создан заказ.
        /// Сохраняется в базе данных как timestamp без временной зоны.
        /// </value>
        [Column(TypeName = "timestamp without time zone")]
        [Display(Name = "Дата заказа")] 
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Идентификатор автомобиля, на котором выполняются работы.
        /// </summary>
        /// <value>
        /// Целочисленное значение, являющееся внешним ключом к таблице Cars.
        /// Обязательное поле, так как каждый заказ должен быть привязан к конкретному автомобилю.
        /// </value>
        [Required(ErrorMessage = "Выберите автомобиль")]
        [Display(Name = "Автомобиль")] 
        public int CarId { get; set; }

        /// <summary>
        /// Идентификатор мастера, ответственного за выполнение заказа.
        /// </summary>
        /// <value>
        /// Nullable целочисленное значение, являющееся внешним ключом к таблице Masters.
        /// Может быть <see langword="null"/>, если мастер еще не назначен.
        /// </value>
        [Display(Name = "Мастер")]
        public int? MasterId { get; set; } 

        /// <summary>
        /// Дата и время оплаты заказа.
        /// </summary>
        /// <value>
        /// Nullable значение <see cref="DateTime"/>, указывающее когда был оплачен заказ.
        /// <see langword="null"/> означает, что заказ еще не оплачен.
        /// Сохраняется в базе данных как timestamp без временной зоны.
        /// </value>
        [Column(TypeName = "timestamp without time zone")]
        [Display(Name = "Дата оплаты")] 
        public DateTime? PaymentDate { get; set; }

        /// <summary>
        /// Навигационное свойство для доступа к автомобилю заказа.
        /// </summary>
        /// <value>
        /// Объект <see cref="Car"/>, представляющий автомобиль, на котором выполняются работы.
        /// Связь устанавливается через свойство <see cref="CarId"/>.
        /// </value>
        [ForeignKey("CarId")]
        [Display(Name = "Автомобиль")] 
        public virtual Car? Car { get; set; }

        /// <summary>
        /// Навигационное свойство для доступа к мастеру, ответственному за заказ.
        /// </summary>
        /// <value>
        /// Объект <see cref="Master"/>, представляющий назначенного мастера.
        /// Связь устанавливается через свойство <see cref="MasterId"/>.
        /// Может быть <see langword="null"/>, если мастер не назначен.
        /// </value>
        [ForeignKey("MasterId")]
        [Display(Name = "Ответственный мастер")]
        public virtual Master? Master { get; set; }

        /// <summary>
        /// Коллекция выполненных работ в рамках данного заказа.
        /// </summary>
        /// <value>
        /// Коллекция объектов <see cref="CompletedWork"/>, представляющих все работы,
        /// выполненные по данному заказу. Используется для связи "один-ко-многим"
        /// между заказом и выполненными работами.
        /// </value>
        /// <remarks>
        /// Каждая работа в этой коллекции относится к конкретной услуге
        /// (<see cref="Service"/>) и была выполнена определенным мастером
        /// в течение указанного количества часов.
        /// </remarks>
        public virtual ICollection<CompletedWork>? CompletedWorks { get; set; }

        /// <summary>
        /// Текстовое представление статуса оплаты заказа.
        /// </summary>
        /// <value>
        /// Строка "Оплачено", если <see cref="PaymentDate"/> имеет значение,
        /// или "Не оплачено", если оплата еще не произведена.
        /// </value>
        /// <remarks>
        /// Это свойство не сохраняется в базе данных (атрибут <see cref="NotMappedAttribute"/>).
        /// Используется только для отображения информации в пользовательском интерфейсе.
        /// </remarks>
        [NotMapped]
        [Display(Name = "Статус оплаты")]
        public string PaymentStatus => PaymentDate.HasValue ? "Оплачено" : "Не оплачено";

        /// <summary>
        /// Проверяет, завершен ли заказ (все работы выполнены и заказ оплачен).
        /// </summary>
        /// <returns>
        /// <see langword="true"/>, если заказ оплачен; в противном случае — <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// В текущей реализации заказ считается завершенным только при наличии даты оплаты.
        /// В будущих версиях может быть добавлена дополнительная логика проверки
        /// завершенности всех работ.
        /// </remarks>
        public bool IsCompleted()
        {
            return PaymentDate.HasValue;
        }

        /// <summary>
        /// Проверяет, назначен ли на заказ ответственный мастер.
        /// </summary>
        /// <returns>
        /// <see langword="true"/>, если мастер назначен; в противном случае — <see langword="false"/>.
        /// </returns>
        public bool HasAssignedMaster()
        {
            return MasterId.HasValue && Master != null;
        }

        /// <summary>
        /// Получает полную информацию о заказе в текстовом формате.
        /// </summary>
        /// <returns>
        /// Строка, содержащая номер заказа, дату, информацию об автомобиле
        /// и статус оплаты.
        /// </returns>
        public string GetOrderInfo()
        {
            var carInfo = Car != null 
                ? $"{Car.Brand} {Car.Model} ({Car.LicensePlate})" 
                : "Автомобиль не указан";

            var masterInfo = Master != null 
                ? Master.FullName 
                : "Мастер не назначен";

            return $"Заказ #{OrderNumber} от {OrderDate:dd.MM.yyyy} | " +
                   $"Автомобиль: {carInfo} | " +
                   $"Мастер: {masterInfo} | " +
                   $"Статус: {PaymentStatus}";
        }
    }
}