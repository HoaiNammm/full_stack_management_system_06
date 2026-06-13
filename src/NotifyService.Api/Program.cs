using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotifyService.Api.Configurations;
using NotifyService.Api.Data;
using NotifyService.Api.Events;
using NotifyService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<NotifyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<JwtService>();

// RabbitMQ consumer (BackgroundService)
builder.Services.AddHostedService<RabbitMqConsumer>();

// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new Exception("JWT configuration is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer           = true,
            ValidIssuer              = jwtSettings.Issuer,
            ValidateAudience         = true,
            ValidAudience            = jwtSettings.Audience,
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue", policy =>
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NotifyService API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Description  = "Nhập: Bearer {token}",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Migrate + Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new NotifyService.Api.Models.User
            {
                Id           = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FullName     = "Project Manager Demo",
                Email        = "2@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role         = "ProjectManager",
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            },
            new NotifyService.Api.Models.User
            {
                Id           = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FullName     = "Member Demo",
                Email        = "1@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role         = "Member",
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            },
            new NotifyService.Api.Models.User
            {
                Id           = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                FullName     = "Viewer Demo",
                Email        = "3@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role         = "Viewer",
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            }
        );
        db.SaveChanges();
    }
}

app.UseCors("AllowVue");
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
