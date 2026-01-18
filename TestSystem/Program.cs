using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TestSystem.Core.Interfaces;
using TestSystem.Infrastructure.Data;
using TestSystem.Infrastructure.Grafana;
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
builder.Services.AddScoped<IClassRoomsRepository, ClassRoomsRepository>();
builder.Services.AddScoped<IUserClassroomsRepository, UserClassroomsRepository>();
builder.Services.AddScoped<IDapperClassRoomsRepository, DapperClassRoomsRepository>();
builder.Services.AddScoped<IClassRoomService, ClassRoomService>();
builder.Services.AddLogging();
builder.Services.AddOpenApi();
builder.Services.AddGlobalMetrics("ClassroomAPI");

var app = builder.Build();

app.UseHttpsRedirection();
app.MapOpenApi();
app.MapScalarApiReference();
app.UseAuthorization();
app.UseGlobalMetrics();
app.MapControllers();

app.Run();