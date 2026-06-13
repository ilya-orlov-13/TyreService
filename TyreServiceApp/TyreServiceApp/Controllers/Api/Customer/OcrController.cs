using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TyreServiceApp.Models.Api;

namespace TyreServiceApp.Controllers.Api.Customer;

[Route("api/customer/ocr")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class OcrController : ControllerBase
{
    [HttpPost("scan")]
    public async Task<ActionResult<ApiResponse<object>>> Scan(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Файл не выбран"));

        try
        {
            await using var ms = new MemoryStream();
            await photo.CopyToAsync(ms);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(120);

            var payload = JsonContent.Create(new { base64_image = base64Image });
            var response = await client.PostAsync("http://localhost:5003/ocr", payload);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode,
                    ApiResponse<object>.Fail($"OCR service error: {response.StatusCode}"));

            using var doc = JsonDocument.Parse(responseBody);
            var text = doc.RootElement.GetProperty("text").GetString() ?? "";

            var result = ParseOcrText(text);
            return Ok(ApiResponse<object>.Ok(result));
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message;
            var msg = "Ошибка распознавания: " + ex.Message + (inner != null ? " (" + inner + ")" : "");
            return StatusCode(500, ApiResponse<object>.Fail(msg));
        }
    }

    private static object ParseOcrText(string text)
    {
        var lines = text.Split('\n');
        var lower = text.ToLower();

        var brands = new[]
        {
            "toyota","bmw","mercedes","audi","volkswagen","vw","opel","ford","renault","peugeot","citroen",
            "hyundai","kia","honda","nissan","mazda","mitsubishi","subaru","suzuki","lexus","infiniti",
            "lada","vaz","uaz","gaz","chevrolet","cadillac","tesla","volvo","skoda","seat","fiat","ferrari",
            "porsche","land rover","jaguar","mini","chrysler","dodge","jeep","bentley","aston martin",
            "тойота","бмв","мерседес","ауди","фольксваген","опель","форд","рено","пежо","ситроен",
            "хёндэ","хендай","киа","хонда","ниссан","мазда","митсубиси","субару","сузуки","лексус","инфинити",
            "лада","ваз","уаз","газ","шевроле","кадиллак","тесла","вольво","шкода","сеат","фиат","феррари",
            "порш","ланд ровер","ягуар","мини","крайслер","додж","джип","бентли","астон мартин"
        };

        var brand = "";
        foreach (var b in brands)
        {
            if (lower.Contains(b))
            {
                brand = b;
                break;
            }
        }

        var model = "";
        if (!string.IsNullOrEmpty(brand))
        {
            var brandIdx = Array.FindIndex(lines, l => l.ToLower().Contains(brand));
            if (brandIdx >= 0)
            {
                for (int i = brandIdx; i < Math.Min(brandIdx + 5, lines.Length); i++)
                {
                    var line = lines[i].Trim();
                    if (line.Length > 2 && line.Length < 35 && !char.IsDigit(line[0]) && !line.ToLower().Contains(brand))
                    {
                        model = line;
                        break;
                    }
                }
            }
        }
        if (string.IsNullOrEmpty(model))
        {
            var modelMatch = Regex.Match(text, @"(?:наименование|модель|model)\s*[:\s]\s*(.+)", RegexOptions.IgnoreCase);
            if (modelMatch.Success)
                model = modelMatch.Groups[1].Value.Trim();
        }

        var vin = "";
        var vinMatch = Regex.Match(text, @"\b[A-HJ-NPR-Z0-9]{17}\b", RegexOptions.IgnoreCase);
        if (vinMatch.Success)
            vin = vinMatch.Value.ToUpper();
        if (string.IsNullOrEmpty(vin))
        {
            var vinSection = Regex.Match(text, @"(?:VIN|vin|номер)\s*[:\s]\s*([A-Z0-9]{13,20})", RegexOptions.IgnoreCase);
            if (vinSection.Success)
                vin = vinSection.Groups[1].Value.ToUpper();
        }

        var licensePlate = "";
        var plateMatch = Regex.Match(text, @"[А-ЯA-Z]\d{3}[А-ЯA-Z]{2}\d{2,3}", RegexOptions.IgnoreCase);
        if (plateMatch.Success)
            licensePlate = plateMatch.Value.ToUpper();

        var year = "";
        foreach (Match y in Regex.Matches(text, @"\b(19[0-9]{2}|20[0-9]{2})\b"))
        {
            if (int.TryParse(y.Value, out var yv) && yv >= 1990 && yv <= 2030)
            {
                if (string.IsNullOrEmpty(year) || yv > int.Parse(year))
                    year = yv.ToString();
            }
        }

        if (!string.IsNullOrEmpty(brand))
            brand = char.ToUpper(brand[0]) + brand[1..];

        return new
        {
            brand,
            model,
            year,
            vin,
            licensePlate
        };
    }
}
