using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskReportApi.Data;
using TaskReportApi.Services;
using TaskListProject.Infrastructure.Data;
using TaskListProject.Application;
using MediatR;
using FluentValidation;
using TaskReportApi.CQRS.Mapping;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? throw new InvalidOperationException("JWT Key not configured")))
    };
});

builder.Services.AddAuthorization();

// Configure CORS to allow requests from the Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAngularDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();

            policy.WithOrigins("http://localhost:50747/")
                  .AllowAnyHeader()
                  .AllowAnyMethod();

        
        });
});

// Configure Entity Framework
var connectionString = builder.Configuration.GetConnectionString("LocalDbConnection");
builder.Services.AddDbContext<TaskListProject.Infrastructure.Data.TaskContext>(options =>
    options.UseSqlServer(connectionString));

// Register services
builder.Services.AddScoped<TaskReportService>();
builder.Services.AddScoped<UserQueries>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<TasksHandler>();
builder.Services.AddScoped<TaskListProject.Infrastructure.Data.TasksQueries>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Configure Mapster
MapsterConfig.ConfigureMapster();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Task Report API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS using the policy defined above
app.UseCors("AllowAngularDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
