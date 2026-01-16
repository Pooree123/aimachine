using Microsoft.EntityFrameworkCore;
using Aimachine.Models;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Add services
// =====================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<AimachineContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// =====================
// Build (มีได้ครั้งเดียว)
// =====================
var app = builder.Build();

// =====================
// Middleware
// =====================
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();

    

app.MapControllers();
app.Run();
