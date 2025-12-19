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
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<DapperDbContext>();
builder.Services.AddScoped<IClassRoomsRepository, ClassRoomsRepository>();
builder.Services.AddScoped<IDapperClassRoomsRepository, DapperClassRoomsRepository>();
builder.Services.AddScoped<IClassRoomService, ClassRoomService>();
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", policy => policy.RequireClaim("Admin"));
});
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();