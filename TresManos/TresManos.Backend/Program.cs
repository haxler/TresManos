using Microsoft.EntityFrameworkCore;
using TresManos.Backend.Data;
using TresManos.Backend.Repositories.Implementations;
using TresManos.Backend.Repositories.Interfaces;

namespace TresManos.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ============================================
        // 1. CONFIGURACIÓN DE BASE DE DATOS
        // ============================================
        builder.Services.AddDbContext<JuegoDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }
            ));

        // ============================================
        // 2. REGISTRO DE REPOSITORIOS
        // ============================================
        // Esto significa que se crea una única instancia de JuegoDbContext por cada solicitud HTTP entrante y se destruye al finalizar esa solicitud
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        builder.Services.AddScoped<IPartidaRepository, PartidaRepository>();
        builder.Services.AddScoped<IRondaRepository, RondaRepository>();

        // ============================================
        // 3. REGISTRO DE UNIT OF WORK
        // ============================================
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ============================================
        // 4. REGISTRO DE SERVICIOS (opcional, si los creas)
        // ============================================
        // builder.Services.AddScoped<IUsuarioService, UsuarioService>();
        // builder.Services.AddScoped<IPartidaService, PartidaService>();
        // builder.Services.AddScoped<IRondaService, RondaService>();

        // ============================================
        // 5. CONFIGURACIÓN DE CONTROLADORES Y CORS
        // ============================================
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Evitar ciclos de referencia en JSON
                options.JsonSerializerOptions.ReferenceHandler =
                    System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

                // Nombres de propiedades en camelCase
                options.JsonSerializerOptions.PropertyNamingPolicy =
                    System.Text.Json.JsonNamingPolicy.CamelCase;
            });

        // CORS (si necesitas acceso desde frontend)
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // ============================================
        // 6. CONFIGURACIÓN DE SWAGGER/OpenAPI
        // ============================================
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "TresManos API - Piedra, Papel o Tijeras",
                Version = "v1",
                Description = "API para el juego Piedra, Papel o Tijeras con sistema de partidas y revanchas",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "TresManos Backend",
                    Email = "contacto@tresmanos.com"
                }
            });
        });

        // ============================================
        // 7. CONFIGURACIÓN DE LOGGING
        // ============================================
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Configurar niveles de log
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);

        // ============================================
        // 8. BUILD DE LA APLICACIÓN
        // ============================================
        var app = builder.Build();

        // ============================================
        // 9. MIDDLEWARE PIPELINE
        // ============================================

        // Manejo global de excepciones (opcional pero recomendado)
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
                if (error != null)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(error.Error, "Error no controlado en la aplicación");

                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Ocurrió un error interno en el servidor.",
                        message = app.Environment.IsDevelopment() ? error.Error.Message : null
                    });
                }
            });
        });

        // Swagger solo en desarrollo
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "TresManos API v1");
                options.RoutePrefix = string.Empty; // Swagger en la raíz (http://localhost:5000/)
            });
        }

        app.UseHttpsRedirection();

        // Habilitar CORS
        app.UseCors("AllowAll");

        app.UseAuthorization();

        app.MapControllers();

        // ============================================
        // 10. MIGRACIÓN AUTOMÁTICA (opcional, solo desarrollo)
        // ============================================
        if (app.Environment.IsDevelopment())
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<JuegoDbContext>();

                    // Aplicar migraciones pendientes automáticamente
                    context.Database.Migrate();

                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Base de datos migrada correctamente.");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Error al aplicar migraciones a la base de datos.");
                }
            }
        }

        // ============================================
        // 11. EJECUTAR LA APLICACIÓN
        // ============================================
        app.Run();
    }
}