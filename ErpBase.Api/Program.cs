using System.Text;
using System.Text.Json.Serialization;
using ErpBase.Api.Authorization;
using ErpBase.Api.Data;
using ErpBase.Api.Options;
using ErpBase.Api.Services.AuthService;
using ErpBase.Api.Services.Email;
using ErpBase.Api.Services.JwtService;
using ErpBase.Api.Services.PasswordReset;
using ErpBase.Api.Services.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.IdentityModel.Tokens;

LoadLocalEnvironment();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ERP Base API",
        Version = "v1",
        Description = "API para gestión de usuarios, roles, permisos y accesos."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(openApiDocument => new OpenApiSecurityRequirement
    {
        [
            new OpenApiSecuritySchemeReference("Bearer", openApiDocument, null)
        ] = []
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<PasswordResetOptions>(builder.Configuration.GetSection(PasswordResetOptions.SectionName));
builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection(CloudinaryOptions.SectionName));
builder.Services.Configure<SmtpEmailOptions>(options =>
{
    builder.Configuration.GetSection(SmtpEmailOptions.SectionName).Bind(options);
    options.Host = GetConfiguredValue(builder.Configuration, options.Host, "Email:Smtp:Host", "Email__Smtp__Host", "EMAIL_SMTP_HOST");
    options.UserName = GetConfiguredValue(builder.Configuration, options.UserName, "Email:Smtp:UserName", "Email__Smtp__UserName", "EMAIL_SMTP_USERNAME");
    options.Password = GetConfiguredValue(builder.Configuration, options.Password, "Email:Smtp:Password", "Email__Smtp__Password", "EMAIL_SMTP_PASSWORD");
    options.FromEmail = GetConfiguredValue(builder.Configuration, options.FromEmail, "Email:Smtp:FromEmail", "Email__Smtp__FromEmail", "EMAIL_SMTP_FROM_EMAIL");
    options.FromName = GetConfiguredValue(builder.Configuration, options.FromName, "Email:Smtp:FromName", "Email__Smtp__FromName", "EMAIL_SMTP_FROM_NAME");

    var port = GetConfiguredValue(builder.Configuration, options.Port.ToString(), "Email:Smtp:Port", "Email__Smtp__Port", "EMAIL_SMTP_PORT");
    if (int.TryParse(port, out var parsedPort))
    {
        options.Port = parsedPort;
    }

    var enableSsl = GetConfiguredValue(builder.Configuration, options.EnableSsl.ToString(), "Email:Smtp:EnableSsl", "Email__Smtp__EnableSsl", "EMAIL_SMTP_ENABLE_SSL");
    if (bool.TryParse(enableSsl, out var parsedEnableSsl))
    {
        options.EnableSsl = parsedEnableSsl;
    }
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(GetRequiredConnectionString(builder.Configuration)));

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
builder.Services.AddScoped<IPasswordResetEmailSender, SmtpPasswordResetEmailSender>();
builder.Services.AddHttpClient<ICloudinaryStorageService, CloudinaryStorageService>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = StatusCodes.Status401Unauthorized,
                    message = "No autenticado. Debe enviar un token valido."
                });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode = StatusCodes.Status403Forbidden,
                    message = "No autorizado. No tiene permisos para realizar esta accion."
                });
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Base API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/reset-password", async context =>
{
    var fileProvider = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
    var filePath = Path.Combine(fileProvider.WebRootPath, "reset-password", "index.html");
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(filePath);
}).AllowAnonymous();

app.Run();

static void LoadLocalEnvironment()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (!string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(environment))
    {
        return;
    }

    var envPath = FindFileUpwards(Directory.GetCurrentDirectory(), ".env");
    if (envPath is not null)
    {
        foreach (var line in File.ReadAllLines(envPath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0) continue;

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    SetIfMissing("Cloudinary__CloudName", "CLOUDINARY_CLOUD_NAME");
    SetIfMissing("Cloudinary__ApiKey", "CLOUDINARY_API_KEY");
    SetIfMissing("Cloudinary__ApiSecret", "CLOUDINARY_API_SECRET");
    SetIfMissing("Cloudinary__Folder", "CLOUDINARY_FOLDER");
    SetIfMissing("Seed__AdminEmail", "SEED_ADMIN_EMAIL");
    SetIfMissing("Seed__AdminPassword", "SEED_ADMIN_PASSWORD");
    SetIfMissing("Email__Smtp__Host", "EMAIL_SMTP_HOST");
    SetIfMissing("Email__Smtp__Port", "EMAIL_SMTP_PORT");
    SetIfMissing("Email__Smtp__EnableSsl", "EMAIL_SMTP_ENABLE_SSL");
    SetIfMissing("Email__Smtp__UserName", "EMAIL_SMTP_USERNAME");
    SetIfMissing("Email__Smtp__Password", "EMAIL_SMTP_PASSWORD");
    SetIfMissing("Email__Smtp__FromEmail", "EMAIL_SMTP_FROM_EMAIL");
    SetIfMissing("Email__Smtp__FromName", "EMAIL_SMTP_FROM_NAME");
}

static string? FindFileUpwards(string startDirectory, string fileName)
{
    var directory = new DirectoryInfo(startDirectory);
    while (directory is not null)
    {
        var candidate = Path.Combine(directory.FullName, fileName);
        if (File.Exists(candidate)) return candidate;
        directory = directory.Parent;
    }

    return null;
}

static void SetIfMissing(string targetKey, string sourceKey)
{
    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(targetKey))) return;

    var sourceValue = Environment.GetEnvironmentVariable(sourceKey);
    if (!string.IsNullOrWhiteSpace(sourceValue))
    {
        Environment.SetEnvironmentVariable(targetKey, sourceValue);
    }
}

static string GetRequiredConnectionString(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        return connectionString;
    }

    throw new InvalidOperationException("Database connection string is missing. Configure ConnectionStrings__DefaultConnection.");
}

static string GetConfiguredValue(IConfiguration configuration, string currentValue, params string[] keys)
{
    if (!string.IsNullOrWhiteSpace(currentValue)) return currentValue;

    foreach (var key in keys)
    {
        var value = configuration[key] ?? Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(value)) return value;
    }

    return currentValue;
}
