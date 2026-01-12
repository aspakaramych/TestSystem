using Microsoft.EntityFrameworkCore;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;
using TestSystem.Infrastructure.Repositories.DapperRepositories;
using TestSystem.Infrastructure.Repositories.EfCoreRepositories;
using TestSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention();
});
builder.Services.AddScoped<DapperDbContext>();
builder.Services.AddScoped<IDapperTaskEntityRepository, DapperTaskEntityRepository>();
builder.Services.AddScoped<ITaskEntityRepository, TaskEntityRepository>();
builder.Services.AddScoped<ITaskService, TestSystem.Infrastructure.Services.TaskService>();
builder.Services.AddLogging();
builder.Services.AddOpenApi();

var app = builder.Build();


app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();