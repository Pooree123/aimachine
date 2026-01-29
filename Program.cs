using Aimachine.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

// ✅ 1. เพิ่มบรรทัดนี้: แก้ปัญหา DateTime ของ PostgreSQL (สำคัญมาก ไม่งั้นจะ Error เวลา save วันที่)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// DbContext
// ✅ 2. เปลี่ยนจาก UseSqlServer เป็น UseNpgsql
builder.Services.AddDbContext<AimachineContext>(options =>
     options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 3. ตั้งค่า JWT Authentication
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Controllers
builder.Services.AddControllers();

// ✅ 4. ตั้งค่า Swagger ให้มีปุ่ม Login
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "ให้พิมพ์คำว่า Bearer เว้นวรรค แล้วตามด้วย Token \nตัวอย่าง: Bearer eyJhbGciOi...",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        new string[] { }
    }});
});

var app = builder.Build();

app.UseCors("AllowAll");

// ✅ 5. เปิดใช้งาน Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
// app.UseHttpsRedirection();

// ✅ 6. เปิดระบบตรวจบัตร (ต้องอยู่ก่อน UseAuthorization เสมอ!)
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.Run();