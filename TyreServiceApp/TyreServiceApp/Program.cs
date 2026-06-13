using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using TyreServiceApp.Extensions;
using TyreServiceApp.Hubs;
using TyreServiceApp.Services;
using TyreServiceApp.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(Program).Assembly)
    .AddRazorOptions(options =>
    {
        options.AreaViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new PermDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new PermNullableDateTimeConverter());
    });

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Keys")));

builder.Services.AddApplicationDatabase(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddScoped<ICalculationService, CalculationService>();
builder.Services.AddScoped<IDistributionService, DistributionService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IMinioService, MinioService>();
builder.Services.AddScoped<IPublicStatsService, PublicStatsService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path == "/Owner" || path == "/Admin")
    {
                context.Request.Path = path + "/Home/Dashboard";
    }
    await next();
});

app.UseRouting();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<OrderHub>("/hubs/orders");

app.Run();
