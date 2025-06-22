using Microsoft.EntityFrameworkCore;
using InventoryAPI.Data;
using InventoryAPI.Services;
using InventoryAPI.Interfaces;
using InventoryAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Entity Framework
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// RabbitMQ Configuration
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Inventory API",
        Version = "v1",
        Description = "API para gestión de inventario de productos",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Inventory Team",
            Email = "inventory@company.com"
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

// CORS (si necesitas acceso desde frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API V1");
        c.DocumentTitle = "Inventory API Documentation";
        // Removido: c.RoutePrefix = string.Empty; 
        // Ahora Swagger estará en /swagger en lugar de raíz
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Asegurar que la base de datos existe y aplicar migraciones
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

    try
    {
        context.Database.EnsureCreated();
        app.Logger.LogInformation("Database ensured for InventoryAPI");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error ensuring database for InventoryAPI");
    }
}

app.Logger.LogInformation("InventoryAPI started successfully");

app.Run();