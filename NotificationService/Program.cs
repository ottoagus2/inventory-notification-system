using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Services;
using NotificationService.Interfaces;
using NotificationService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Entity Framework
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// RabbitMQ Configuration
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// Services
builder.Services.AddScoped<IInventoryLogService, InventoryLogService>();
builder.Services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();

// Background Service para el consumidor de RabbitMQ
builder.Services.AddHostedService<InventoryConsumerService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "Servicio de notificaciones para actualizaciones de inventario",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Notification Team",
            Email = "notifications@company.com"
        }
    });

    // Incluir comentarios XML para documentación
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service V1");
        c.DocumentTitle = "Notification Service API Documentation";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Asegurar que la base de datos existe y aplicar migraciones
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

    try
    {
        context.Database.EnsureCreated();
        app.Logger.LogInformation("Database ensured for NotificationService");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error ensuring database for NotificationService");
    }
}

app.Logger.LogInformation("NotificationService started successfully");

app.Run();