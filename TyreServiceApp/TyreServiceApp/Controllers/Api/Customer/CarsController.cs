using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;
using TyreServiceApp.Models;
using TyreServiceApp.Models.Api;
using TyreServiceApp.Services;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Controllers.Api.Customer;

[Route("api/customer/cars")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CustomerCarsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMinioService _minio;

    public CustomerCarsController(ApplicationDbContext db, IMinioService minio)
    {
        _db = db;
        _minio = minio;
    }

    private int GetClientId() => CustomerClientIdResolver.Resolve(User, _db);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CarDto>>>> GetAll()
    {
        var clientId = GetClientId();
        if (clientId == 0)
            return Unauthorized(ApiResponse<List<CarDto>>.Fail("Клиент не найден"));

        var cars = await _db.Cars
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.CarId)
            .ToListAsync();

        var dtos = await Task.WhenAll(cars.Select(ToDtoAsync));
        return Ok(ApiResponse<List<CarDto>>.Ok(dtos.ToList()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CarDto>>> GetById(int id)
    {
        var clientId = GetClientId();
        var car = await _db.Cars
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.CarId == id && c.ClientId == clientId);

        if (car == null)
            return NotFound(ApiResponse<CarDto>.Fail("Автомобиль не найден"));

        return Ok(ApiResponse<CarDto>.Ok(await ToDtoAsync(car)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CarDto>>> Create(
        [FromForm] string brand,
        [FromForm] string model,
        [FromForm] int manufactureYear,
        [FromForm] string licensePlate,
        [FromForm] string vin,
        [FromForm] int? carClassId,
        [FromForm] List<IFormFile>? photos)
    {
        var clientId = GetClientId();
        if (clientId == 0)
            return Unauthorized(ApiResponse<CarDto>.Fail("Клиент не найден"));

        var errors = new List<string>();
        if (await _db.Cars.AnyAsync(c => c.LicensePlate == licensePlate))
            errors.Add("Автомобиль с таким госномером уже существует");
        if (await _db.Cars.AnyAsync(c => c.Vin == vin))
            errors.Add("Автомобиль с таким VIN уже существует");
        if (errors.Count > 0)
            return Conflict(ApiResponse<CarDto>.Fail(string.Join("; ", errors)));

        var photoPaths = new List<string>();
        if (photos?.Count > 0)
        {
            foreach (var photo in photos.Where(p => p.Length > 0))
            {
                try
                {
                    var path = await _minio.UploadAsync(photo, clientId);
                    if (string.IsNullOrEmpty(path))
                        return StatusCode(500, ApiResponse<CarDto>.Fail("Не удалось загрузить фото: S3 недоступен"));
                    photoPaths.Add(path);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ApiResponse<CarDto>.Fail($"Ошибка загрузки фото: {ex.Message}"));
                }
            }
        }

        var car = new Car
        {
            ClientId = clientId,
            Brand = brand,
            Model = model,
            ManufactureYear = manufactureYear,
            LicensePlate = licensePlate,
            Vin = vin,
            CarClassId = carClassId,
            PhotoPath = photoPaths.FirstOrDefault(),
            AdditionalPhotos = photoPaths.Count > 1
                ? JsonSerializer.Serialize(photoPaths.Skip(1).ToList())
                : null
        };

        _db.Cars.Add(car);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = car.CarId },
            ApiResponse<CarDto>.Ok(await ToDtoAsync(car)));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CarDto>>> Update(
        int id,
        [FromForm] string brand,
        [FromForm] string model,
        [FromForm] int manufactureYear,
        [FromForm] string licensePlate,
        [FromForm] string vin,
        [FromForm] int? carClassId,
        [FromForm] string? deletePhotoIndices,
        [FromForm] List<IFormFile>? photos)
    {
        var clientId = GetClientId();
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.CarId == id && c.ClientId == clientId);
        if (car == null)
            return NotFound(ApiResponse<CarDto>.Fail("Автомобиль не найден"));

        var errors = new List<string>();
        if (await _db.Cars.AnyAsync(c => c.LicensePlate == licensePlate && c.CarId != id))
            errors.Add("Автомобиль с таким госномером уже существует");
        if (await _db.Cars.AnyAsync(c => c.Vin == vin && c.CarId != id))
            errors.Add("Автомобиль с таким VIN уже существует");
        if (errors.Count > 0)
            return Conflict(ApiResponse<CarDto>.Fail(string.Join("; ", errors)));

        // Build current photo path list
        var photoPaths = new List<string>();
        if (!string.IsNullOrEmpty(car.PhotoPath))
            photoPaths.Add(car.PhotoPath);
        if (!string.IsNullOrEmpty(car.AdditionalPhotos))
        {
            var oldPhotos = DeserializePhotoKeys(car.AdditionalPhotos);
            photoPaths.AddRange(oldPhotos);
        }

        // Remove deleted photos
        if (!string.IsNullOrEmpty(deletePhotoIndices))
        {
            var indices = JsonSerializer.Deserialize<List<int>>(deletePhotoIndices) ?? [];
            var toDelete = indices.Where(i => i >= 0 && i < photoPaths.Count).OrderByDescending(i => i).ToList();
            foreach (var i in toDelete)
            {
                await _minio.DeleteAsync(photoPaths[i]);
                photoPaths.RemoveAt(i);
            }
        }

        // Upload new photos
        if (photos?.Count > 0)
        {
            foreach (var photo in photos.Where(p => p.Length > 0))
            {
                try
                {
                    var path = await _minio.UploadAsync(photo, clientId);
                    if (string.IsNullOrEmpty(path))
                        return StatusCode(500, ApiResponse<CarDto>.Fail("Не удалось загрузить фото: S3 недоступен"));
                    photoPaths.Add(path);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ApiResponse<CarDto>.Fail($"Ошибка загрузки фото: {ex.Message}"));
                }
            }
        }

        car.PhotoPath = photoPaths.FirstOrDefault();
        car.AdditionalPhotos = photoPaths.Count > 1
            ? JsonSerializer.Serialize(photoPaths.Skip(1).ToList())
            : null;

        car.Brand = brand;
        car.Model = model;
        car.ManufactureYear = manufactureYear;
        car.LicensePlate = licensePlate;
        car.Vin = vin;
        car.CarClassId = carClassId;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23505")
        {
            if (pg.ConstraintName == "IX_Cars_LicensePlate")
                return Conflict(ApiResponse<CarDto>.Fail("Автомобиль с таким госномером уже существует"));
            if (pg.ConstraintName == "IX_Cars_Vin")
                return Conflict(ApiResponse<CarDto>.Fail("Автомобиль с таким VIN уже существует"));
            throw;
        }

        return Ok(ApiResponse<CarDto>.Ok(await ToDtoAsync(car)));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var clientId = GetClientId();
        var car = await _db.Cars.FirstOrDefaultAsync(c => c.CarId == id && c.ClientId == clientId);
        if (car == null)
            return NotFound(ApiResponse<object>.Fail("Автомобиль не найден"));

        if (!string.IsNullOrEmpty(car.PhotoPath))
            await _minio.DeleteAsync(car.PhotoPath);
        if (!string.IsNullOrEmpty(car.AdditionalPhotos))
        {
            var photos = DeserializePhotoKeys(car.AdditionalPhotos);
            foreach (var p in photos)
                await _minio.DeleteAsync(p);
        }

        _db.Cars.Remove(car);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { }));
    }

    private async Task<CarDto> ToDtoAsync(Car car)
    {
        var photoUrl = !string.IsNullOrEmpty(car.PhotoPath)
            ? await _minio.GetFileUrlAsync(car.PhotoPath)
            : null;

        var additionalPhotosJson = car.AdditionalPhotos;
        if (!string.IsNullOrEmpty(additionalPhotosJson))
        {
            var keys = DeserializePhotoKeys(additionalPhotosJson);
            var urls = await Task.WhenAll(keys.Select(k => _minio.GetFileUrlAsync(k)));
            additionalPhotosJson = JsonSerializer.Serialize(urls.ToList());
        }

        return new CarDto(
            car.CarId,
            car.Brand,
            car.Model,
            car.ManufactureYear,
            car.LicensePlate,
            car.Vin,
            photoUrl,
            additionalPhotosJson,
            car.FullInfo
        );
    }

    private static List<string> DeserializePhotoKeys(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
