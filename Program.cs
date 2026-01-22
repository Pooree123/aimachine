using Aimachine.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.IdentityModel.Tokens;            
using System.Text;                                 
using Microsoft.OpenApi.Models;                    

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
builder.Services.AddDbContext<AimachineContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions => sqlOptions.EnableRetryOnFailure()));

// ✅ 2. ตั้งค่า JWT Authentication (ส่วนนี้สำคัญ!)
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

// ✅ 3. ตั้งค่า Swagger ให้มีปุ่ม Login (รูปแม่กุญแจ)
// (ผมเปลี่ยนจาก AddOpenApi มาใช้ AddSwaggerGen เพื่อให้เทส Token ง่ายขึ้นครับ)
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

// ✅ 4. เปิดใช้งาน Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
// app.UseHttpsRedirection();

// ✅ 5. เปิดระบบตรวจบัตร (ต้องอยู่ก่อน UseAuthorization เสมอ!)
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.Run();