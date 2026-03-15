using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Repositries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

//using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Infastructure/logs/gymapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders().AddSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API",
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
});

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IUserServise, UserService>();
builder.Services.AddScoped<ITokenHandler, Gym_App.Application.Services.TokenHandler>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<IExerciseData, ExerciseData>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IMuscleService, MuscleService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IScheduleService,ScheduleService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRepositry, UserRepositry>();
builder.Services.AddScoped<IBaseRepositry<User>, UserRepositry>();
builder.Services.AddScoped<IWorkoutRepositry, WorkoutRepositry>();
builder.Services.AddScoped<IBaseRepositry<Workout>, WorkoutRepositry>();
builder.Services.AddScoped<IExerciseRepositry, ExerciseRepositry>();
builder.Services.AddScoped<IBaseRepositry<Exercise>, ExerciseRepositry>();
builder.Services.AddScoped<IMuscleRepositry, MuscleRepositry>();
builder.Services.AddScoped<IBaseRepositry<Muscles>, MuscleRepositry>();
builder.Services.AddScoped<IMessageRepositry, MessageRepositry>();
builder.Services.AddScoped<IBaseRepositry<Message>, MessageRepositry>();
builder.Services.AddScoped<ISessionRepositry, SessionRepositry>();
builder.Services.AddScoped<IBaseRepositry<Session>, SessionRepositry>();
builder.Services.AddScoped<ITokenRepositry, TokenRepositry>();
builder.Services.AddScoped<IBaseRepositry<RefreshTokens>, TokenRepositry>();
builder.Services.AddScoped<IRoleRepositry, RoleRepositry>();
builder.Services.AddScoped<IBaseRepositry<Role>, RoleRepositry>();
builder.Services.AddScoped<IFeedbackRepositry, FeedbackRepositry>();
builder.Services.AddScoped<IBaseRepositry<Feedback>, FeedbackRepositry>();
//builder.Services.AddScoped<IBaseRepositry<BaseEntity>,BaseRepositry<BaseEntity>>();
builder.Services.AddSingleton<IAuthorizationHandler, SameUserHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ListUserHandler>();


builder.Services.AddDbContext<DbBase>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ModyConnection"]);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
    AddJwtBearer(options =>
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
    });

Console.WriteLine("Connection String = " + builder.Configuration["ConnectionStrings:VpsConnection"]);
Console.WriteLine("Issuer = " + builder.Configuration["JwtSettings:Issuer"]);
Console.WriteLine("Audience = " + builder.Configuration["JwtSettings:Audience"]);
Console.WriteLine("Token = " + (builder.Configuration["JwtSettings:Token"]));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NormalUsage",
        policy => policy.RequireRole("Admin","User"));
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ElevatedPower",
        policy => policy.RequireRole("Admin"));
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SameUserPolicy", policy =>
        policy.Requirements.Add(new SameUserRequirement(allowAdmins: true)));
    // keep other policies (NormalUsage, ElevatedPower) as-is
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ListUserPolicy", policy =>
        policy.Requirements.Add(new ListUserRequirement(allowAdmins: true)));
});
//builder.Services.AddControllersWithViews().AddJsonOptions(x =>
//   x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);//yezabat error beta3 recurrsion taba3 el json
builder.Services.ConfigureHttpJsonOptions(x=>
{
    x.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    x.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DbBase>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Log but don’t crash startup
        Console.WriteLine($"Migration failed: {ex.Message}");
    }
}
// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
