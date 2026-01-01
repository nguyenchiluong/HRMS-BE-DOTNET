using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EmployeeApi.Data;
using EmployeeApi.Repositories;
using EmployeeApi.Services;
using EmployeeApi.Services.Employee;
using EmployeeApi.Services.Timesheet;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,         // Java service may not set issuer
        ValidateAudience = false,       // Java service may not set audience
        ValidateLifetime = true,        // Validate exp claim
        ValidateIssuerSigningKey = true, // Must validate signature
        // Java service uses Base64-encoded secret key
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKey)),
        ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 min clock skew
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the format: {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();

// Auth Service HttpClient
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IOnboardingTokenService, OnboardingTokenService>();

// RabbitMQ Configuration
builder.Services.AddSingleton<IConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var factory = new ConnectionFactory
    {
        HostName = configuration["RabbitMQ:Host"] ?? "localhost",
        Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
        UserName = configuration["RabbitMQ:Username"] ?? "hrms_user",
        Password = configuration["RabbitMQ:Password"] ?? "hrms_password",
        VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        RequestedHeartbeat = TimeSpan.FromSeconds(60)
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddScoped<IMessageProducerService, MessageProducerService>();

//CORS

// Add services to the container.
builder.Services.AddControllers();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173") // React dev server
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

//CORS

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS
app.UseCors("AllowReactApp");
//CORS

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
