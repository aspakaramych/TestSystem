using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TestSystem.Core.Interfaces;
using TestSystem.Core.RabbitModels; // Проверь namespace
using TestSystem.Infrastructure.Data;
using TestSystem.Infrastructure.Repositories.DapperRepositories;
using TestSystem.Infrastructure.Repositories.EfCoreRepositories;
using TestSystem.Infrastructure.RabbitMqService; // Твои новые Rabbit-сервисы
using TestSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention();
});

builder.Services.AddScoped<DapperDbContext>();
builder.Services.AddScoped<IDapperPackageRepository, DapperPackageRepository>();
builder.Services.AddScoped<ITaskEntityRepository, TaskEntityRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();

builder.Services.AddSingleton<ConcurrentDictionary<string, TaskCompletionSource<CodeExecutionResult>>>();

builder.Services.AddSingleton<RabbitMqPublisher>();

builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMqPublisher>());


builder.Services.AddHostedService<RabbitMqConsumer>();

builder.Services.AddScoped<IPackageService, TestSystem.Infrastructure.Services.PackageService>();

builder.Services.AddLogging();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthorization();
app.MapControllers();

app.Run();