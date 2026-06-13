namespace TyreServiceApp.Models.Api;

public record UserDto(
    int CustomerId,
    int? ClientId,
    string FullName,
    string Phone
);

public record AuthResponse(
    string Token,
    UserDto User
);

public record CarDto(
    int CarId,
    string Brand,
    string Model,
    int ManufactureYear,
    string LicensePlate,
    string Vin,
    string? PhotoUrl,
    string? AdditionalPhotosJson,
    string FullInfo
);

public record OrderDto(
    int OrderNumber,
    DateTime OrderDate,
    DateTime? ScheduledAt,
    string Status,
    string PaymentStatus,
    decimal? ClientTotal,
    CarDto? Car,
    string? MasterName,
    List<CompletedWorkDto> Services,
    string? TireInfo = null
);

public record CompletedWorkDto(
    int WorkId,
    int ServiceCode,
    string ServiceName,
    decimal WorkTotal,
    int WheelCount
);

public record ServiceDto(
    int ServiceCode,
    string ServiceName,
    decimal ServiceCost,
    int? FixedDurationMin
);

public record TimeSlotDto(
    DateTime Time,
    bool Available
);

public record TireDto(
    int TireId,
    string TireType,
    string Seasonality,
    string Manufacturer,
    string TireModel,
    string Size,
    int LoadIndex,
    int WearPercentage,
    decimal Pressure,
    string FullInfo
);

public class CustomerCreateTireRequest
{
    public string TireType { get; set; } = "Легковая";
    public string Seasonality { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string TireModel { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int LoadIndex { get; set; }
    public int WearPercentage { get; set; }
    public decimal Pressure { get; set; } = 2.2m;
}

public class CustomerUpdateTireRequest
{
    public string TireType { get; set; } = "Легковая";
    public string Seasonality { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string TireModel { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int LoadIndex { get; set; }
    public int WearPercentage { get; set; }
    public decimal Pressure { get; set; } = 2.2m;
}

public record CreateOrderRequest(
    int? CarId,
    List<int>? ServiceCodes,
    bool HasOther,
    string? Description,
    DateTime? ScheduledAt,
    int WheelCount = 4,
    int? TireId = null
);

public record EditOrderRequest(
    List<int>? ServiceCodes,
    bool HasOther,
    string? Description,
    DateTime? ScheduledAt,
    int WheelCount = 4
);

// Дополнительные модели для расширенного API создания заказов
public record CreateOrderApiRequest(
    DateTime? OrderDate,
    int? CarId,
    int? TireId,
    int? MasterId,
    DateTime? ScheduledAt,
    decimal? DiscountPercent,
    string? DiscountType,
    List<int>? ComplexityCoefficientIds,
    List<CreateOrderConsumableRequest>? Consumables,
    List<CreateOrderServiceRequest>? Services
);

public record CreateOrderConsumableRequest(
    int ConsumableId,
    int Quantity
);

public record CreateOrderServiceRequest(
    int ServiceCode,
    int WheelCount
);

public record UpdateOrderRequest(
    int? MasterId,
    DateTime? ScheduledAt,
    string? Status,
    decimal? DiscountPercent,
    string? DiscountType
);
