using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Text;
using Gamification.API.Middleware;
using Gamification.API.Services;
using Gamification.Core.Date;
using Gamification.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Настраиваем авторизацию через JWT.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT-токен."
    });

    // Добавляем требование авторизации.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// Подключаем базу данных
builder.Services.AddDbContext<Dbcontext>();

// Регистрируем игровые сервисы
builder.Services.AddSingleton<ILevelService, LevelService>();
builder.Services.AddSingleton<IXpCalculator, XpCalculator>();
builder.Services.AddSingleton<IAchievementEvaluator, AchievementEvaluator>();

builder.Services.AddScoped<GamificationEngine>();

// Регистрируем сервис создания JWT
builder.Services.AddScoped<JwtTokenService>();

// Получаем секретный ключ из конфигурации
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Не найден ключ JWT.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Настраиваем конвейер обработки запросов

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();