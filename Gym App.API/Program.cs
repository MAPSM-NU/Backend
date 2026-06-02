using Gym_App.Application.Authorization;
using Gym_App.Application.Authorization.Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Application.Services;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Repositries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using Gym_App.Infrastructure.Interfaces.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// LOGGING CONFIGURATION
// ============================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Infastructure/logs/gymapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders().AddSerilog();

// ============================================
// CORE SERVICES
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// ============================================
// CORS CONFIGURATION
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSignalR();

// ============================================
// SWAGGER CONFIGURATION
// ============================================
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gym App API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    // Ignore obsolete properties and methods
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
});

// ============================================
// DATABASE CONFIGURATION
// ============================================
builder.Services.AddDbContext<DbBase>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("VpsConnection"),
        b => b.MigrationsAssembly("Gym_App.Infrastructure")));

// ============================================
// UNIT OF WORK & REPOSITORIES
// ============================================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ============================================
// APPLICATION SERVICES
// ============================================
builder.Services.AddScoped<IUserServise, UserService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IMuscleService, MuscleService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ITokenHandler, Gym_App.Application.Services.TokenHandler>();


// ============================================
// UTILITY SERVICES
// ============================================
builder.Services.AddSingleton<NotificationNotifier>();
builder.Services.AddSingleton<INotificationSink>(provider => provider.GetRequiredService<NotificationNotifier>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<NotificationNotifier>());

builder.Services.AddSingleton<WorkoutNotifier>();
builder.Services.AddSingleton<IWorkoutNotificationSink>(provider => provider.GetRequiredService<WorkoutNotifier>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<WorkoutNotifier>());

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IExerciseData, ExerciseData>();

// ============================================
// AUTHORIZATION
// ============================================
builder.Services.AddSingleton<IAuthorizationHandler, SameUserHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ListUserHandler>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ICachedAuthorizationService, CachedAuthorizationService>();

// ============================================
// AUTHENTICATION
// ============================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Token"]!)),
            ValidateIssuerSigningKey = true,
        };
        options.IncludeErrorDetails = true;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// ============================================
// AUTHORIZATION POLICIES
// ============================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NormalUsage", policy => 
        policy.RequireRole("Admin", "User"));
    
    options.AddPolicy("ElevatedPower", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("SameUserPolicy", policy =>
        policy.Requirements.Add(new SameUserRequirement(allowAdmins: true)));
    
    options.AddPolicy("ListUserPolicy", policy =>
        policy.Requirements.Add(new ListUserRequirement(allowAdmins: true)));
});

// ============================================
// JSON SERIALIZATION
// ============================================
builder.Services.ConfigureHttpJsonOptions(x =>
{
    x.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    x.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

Log.Information("Starting Gym App API...");
Log.Information("Issuer: {Issuer}", builder.Configuration["JwtSettings:Issuer"]);
Log.Information("Audience: {Audience}", builder.Configuration["JwtSettings:Audience"]);
Log.Information("Token Key: {Key}", builder.Configuration["JwtSettings:Token"]);
Log.Information("Database Connection: {ConnectionString}", builder.Configuration.GetConnectionString("Connection"));
Log.Information("Email Sender: {EmailSender}", builder.Configuration["EmailSettings:SenderEmail"]);
Log.Information("SMTP Host: {SmtpHost}", builder.Configuration["EmailSettings:SmtpHost"]);
Log.Information("SMTP Port: {SmtpPort}", builder.Configuration["EmailSettings:SmtpPort"]);
Log.Information("SMTP Username: {SmtpUsername}", builder.Configuration["EmailSettings:SmtpUsername"]);
Log.Information("SMTP Password: {SmtpPassword}", builder.Configuration["EmailSettings:SmtpPassword"]);

// ============================================
// BUILD APPLICATION
// ============================================
var app = builder.Build();

// ============================================
// DATABASE MIGRATION
// ============================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DbBase>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Log.Error($"Migration failed: {ex.Message}");
    }
}

// ============================================
// MIDDLEWARE PIPELINE
// ============================================
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseSwagger(c =>
{
    c.SerializeAsV2 = false;
});
app.UseSwaggerUI();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chat");
app.MapHub<NotificationHub>("/notif");
app.MapControllers();

app.Run();

