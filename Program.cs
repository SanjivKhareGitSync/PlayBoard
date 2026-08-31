using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlayBoard.Middleware;
using PlayBoard.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Allows localhost/file origins
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
builder.Services.AddScoped<IUserStore, JsonUserStore>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IGameStateStore, InMemoryGameStateStore>();

// Register custom authorization handlers and policies
builder.Services.AddSingleton<IAuthorizationHandler, PlayBoard.ClassCollection.FlagAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    // Policy name "FlagAllowed" — require the custom requirement
    options.AddPolicy("FlagAllowed", policy => policy.Requirements.Add(new PlayBoard.ClassCollection.FlagRequirement()));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// JWT settings
byte[] key;
var jwtSettings = builder.Configuration.GetSection("Jwt");
if (jwtSettings["Key"] is not null)
{
    key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "");
}
else
{
    throw new InvalidOperationException("JWT Key is not configured.");
}

// Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

// Swagger with JWT Bearer support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlayBoard API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token as: {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });
});

var app = builder.Build();

// After var app = builder.Build();
app.UseCors("AllowAll");  // Before app.UseAuthorization() / app.MapControllers()
app.UseStaticFiles();
app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // must come before UseAuthorization
app.UseMiddleware<AdminAccessMiddleware>(); // path-based guard, before routing decides the endpoint
app.UseAuthorization();

app.MapControllers();

app.Run();