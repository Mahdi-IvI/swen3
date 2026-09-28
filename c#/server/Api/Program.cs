using System.Text;
using Api.Exceptions;
using Api.Services;
using Bll.@new;
using Dal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Logging.AddLog4Net("log4net.config");

// Add services to the container.
var jwtSection = builder.Configuration.GetSection(JwtOptions.Jwt);
builder.Services
    .AddOptions<JwtOptions>()
    .Bind(jwtSection)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IUserRepository, EntityFrameworkUserRepository>();
builder.Services.AddScoped<IDocumentRepository, EntityFrameworkDocumentRepository>();
builder.Services.AddScoped<IFolderRepository, EntityFrameworkFolderRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")
                      ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required."));
    if (!builder.Environment.IsDevelopment()) return; // check for development environment
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, LogLevel.Information);
});

builder.Services.AddControllers();
builder.Services.AddCors();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = jwtSection.Get<JwtOptions>()!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here. Example: Bearer eyJhbGciOi..."
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", null, null),
            []
        }
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();

    // Enable CORS for development
    app.UseCors(policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
            .AllowAnyMethod()
            .AllowAnyHeader();
    });

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.UseExceptionHandler();

app.UseDefaultFiles(); // serves wwwroot/index.html at root /
app.UseStaticFiles(); // serves static files from wwwroot

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();