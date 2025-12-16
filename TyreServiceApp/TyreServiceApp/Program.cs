using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Добавьте службы в контейнер ПОСЛЕ создания builder и ДО builder.Build()
builder.Services.AddControllersWithViews();

// ВАЖНО: Код регистрации DbContext должен быть здесь:

// 1. Получаем базовую строку подключения без пароля из appsettings.json
var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Получаем пароль из секретов
var dbPassword = builder.Configuration["DbPassword"];

// 3. Собираем полную строку подключения
var fullConnectionString = $"{baseConnectionString}Password={dbPassword};";

// 4. Регистрируем контекст БД (эта строка ДОЛЖНА быть до builder.Build())
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(fullConnectionString));

// Конец регистрации сервисов. Теперь можно строить приложение.
var app = builder.Build(); // <-- После этой строки добавлять сервисы уже НЕЛЬЗЯ

// Далее настраиваем конвейер HTTP (middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();