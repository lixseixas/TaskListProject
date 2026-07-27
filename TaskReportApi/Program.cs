using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskReportApi.Data;
using TaskReportApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure CORS to allow requests from the Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAngularDev",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configure Entity Framework
var connectionString = builder.Configuration.GetConnectionString("LocalDbConnection");
builder.Services.AddDbContext<TaskContext>(options =>
    options.UseSqlServer(connectionString));

// Register services
builder.Services.AddScoped<TaskReportService>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
