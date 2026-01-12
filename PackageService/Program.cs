using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;
using TestSystem.Infrastructure.KafkaServices;
using TestSystem.Infrastructure.Repositories.EfCoreRepositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention();
});
builder.Services.AddScoped<ITaskEntityRepository, TaskEntityRepository>();
builder.Services.AddScoped<IPackageRepository, PackageService>();
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<KafkaProducer>());
builder.Services.AddScoped<IPackageService, TestSystem.Infrastructure.Services.PackageService>();
builder.Services.AddLogging();
builder.Services.AddOpenApi();

var app = builder.Build();


app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();