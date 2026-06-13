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
        policy.SetIsOriginAllowed(origin =>
        {
            var host = new Uri(origin).Host;
            return host == "localhost" || host == "127.0.0.1";
        })
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

    db.Database.ExecuteSqlRaw(@"
        IF COL_LENGTH('Users', 'PhoneNumber') IS NULL ALTER TABLE Users ADD PhoneNumber nvarchar(max) NULL;
        IF COL_LENGTH('Users', 'AvatarUrl') IS NULL ALTER TABLE Users ADD AvatarUrl nvarchar(max) NULL;
        IF COL_LENGTH('Users', 'Department') IS NULL ALTER TABLE Users ADD Department nvarchar(max) NULL;
        IF COL_LENGTH('Users', 'Position') IS NULL ALTER TABLE Users ADD Position nvarchar(max) NULL;
        IF COL_LENGTH('Users', 'EmailConfirmed') IS NULL ALTER TABLE Users ADD EmailConfirmed bit NOT NULL CONSTRAINT DF_Users_EmailConfirmed DEFAULT 0;
        IF COL_LENGTH('Users', 'LastLoginAt') IS NULL ALTER TABLE Users ADD LastLoginAt datetime2 NULL;
        IF COL_LENGTH('Users', 'UpdatedAt') IS NULL ALTER TABLE Users ADD UpdatedAt datetime2 NULL;

        IF COL_LENGTH('ActivityLogs', 'Description') IS NULL ALTER TABLE ActivityLogs ADD Description nvarchar(max) NOT NULL CONSTRAINT DF_ActivityLogs_Description DEFAULT '';
        IF COL_LENGTH('ActivityLogs', 'TaskId') IS NULL ALTER TABLE ActivityLogs ADD TaskId uniqueidentifier NULL;
        IF COL_LENGTH('ActivityLogs', 'ProjectId') IS NULL ALTER TABLE ActivityLogs ADD ProjectId uniqueidentifier NULL;
        IF COL_LENGTH('ActivityLogs', 'MetadataJson') IS NULL ALTER TABLE ActivityLogs ADD MetadataJson nvarchar(max) NULL;
        IF COL_LENGTH('ActivityLogs', 'ResourceType') IS NULL ALTER TABLE ActivityLogs ADD ResourceType nvarchar(max) NOT NULL CONSTRAINT DF_ActivityLogs_ResourceType DEFAULT '';
        IF COL_LENGTH('ActivityLogs', 'ResourceId') IS NULL ALTER TABLE ActivityLogs ADD ResourceId uniqueidentifier NOT NULL CONSTRAINT DF_ActivityLogs_ResourceId DEFAULT '00000000-0000-0000-0000-000000000000';
        IF COL_LENGTH('ActivityLogs', 'Timestamp') IS NULL ALTER TABLE ActivityLogs ADD [Timestamp] datetime2 NOT NULL CONSTRAINT DF_ActivityLogs_Timestamp DEFAULT SYSUTCDATETIME();
        IF COL_LENGTH('ActivityLogs', 'CreatedAt') IS NULL ALTER TABLE ActivityLogs ADD CreatedAt datetime2 NOT NULL CONSTRAINT DF_ActivityLogs_CreatedAt DEFAULT SYSUTCDATETIME();

        IF COL_LENGTH('Comments', 'ProjectId') IS NULL ALTER TABLE Comments ADD ProjectId uniqueidentifier NULL;
        IF COL_LENGTH('Comments', 'UserId') IS NULL ALTER TABLE Comments ADD UserId uniqueidentifier NOT NULL CONSTRAINT DF_Comments_UserId DEFAULT '00000000-0000-0000-0000-000000000000';
        IF COL_LENGTH('Comments', 'IsDeleted') IS NULL ALTER TABLE Comments ADD IsDeleted bit NOT NULL CONSTRAINT DF_Comments_IsDeleted DEFAULT 0;

        IF COL_LENGTH('CommentMentions', 'MentionedUserId') IS NULL ALTER TABLE CommentMentions ADD MentionedUserId uniqueidentifier NOT NULL CONSTRAINT DF_CommentMentions_MentionedUserId DEFAULT '00000000-0000-0000-0000-000000000000';
        IF COL_LENGTH('CommentMentions', 'CreatedAt') IS NULL ALTER TABLE CommentMentions ADD CreatedAt datetime2 NOT NULL CONSTRAINT DF_CommentMentions_CreatedAt DEFAULT SYSUTCDATETIME();

        IF OBJECT_ID('CommentAttachments', 'U') IS NULL
        BEGIN
            CREATE TABLE CommentAttachments (
                Id uniqueidentifier NOT NULL CONSTRAINT PK_CommentAttachments PRIMARY KEY DEFAULT NEWID(),
                CommentId uniqueidentifier NOT NULL,
                FileName nvarchar(max) NOT NULL,
                FileUrl nvarchar(max) NOT NULL,
                ContentType nvarchar(max) NULL,
                FileSize bigint NOT NULL,
                CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
            );
        END

        IF COL_LENGTH('Notifications', 'Message') IS NULL ALTER TABLE Notifications ADD Message nvarchar(max) NOT NULL CONSTRAINT DF_Notifications_Message DEFAULT '';
        IF COL_LENGTH('Notifications', 'TaskId') IS NULL ALTER TABLE Notifications ADD TaskId uniqueidentifier NULL;
        IF COL_LENGTH('Notifications', 'ProjectId') IS NULL ALTER TABLE Notifications ADD ProjectId uniqueidentifier NULL;
        IF COL_LENGTH('Notifications', 'SourceService') IS NULL ALTER TABLE Notifications ADD SourceService nvarchar(max) NULL;
        IF COL_LENGTH('Notifications', 'SourceEventId') IS NULL ALTER TABLE Notifications ADD SourceEventId nvarchar(max) NULL;

        IF OBJECT_ID('UserNotifications', 'U') IS NULL
        BEGIN
            CREATE TABLE UserNotifications (
                Id uniqueidentifier NOT NULL CONSTRAINT PK_UserNotifications PRIMARY KEY DEFAULT NEWID(),
                NotificationId uniqueidentifier NOT NULL,
                UserId uniqueidentifier NOT NULL,
                IsRead bit NOT NULL DEFAULT 0,
                ReadAt datetime2 NULL,
                CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
            );
        END

        IF OBJECT_ID('NotificationLogs', 'U') IS NULL
        BEGIN
            CREATE TABLE NotificationLogs (
                Id uniqueidentifier NOT NULL CONSTRAINT PK_NotificationLogs PRIMARY KEY DEFAULT NEWID(),
                NotificationId uniqueidentifier NOT NULL,
                UserId uniqueidentifier NULL,
                Action nvarchar(max) NOT NULL,
                Status nvarchar(max) NOT NULL,
                ErrorMessage nvarchar(max) NULL,
                CreatedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
            );
        END");

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
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
