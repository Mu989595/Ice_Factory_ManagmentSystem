using System.Text.Json;
using IceFactoryManagmentSystem.Middleware;
using IcePlant.Infrastructure;
using IcePlant.Infrastructure.JsonConverters;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.AspNetCore;

namespace IceFactoryManagmentSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // ── LOGGING CONFIGURATION ──────────────────────────────────────────
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: "logs/ice-factory-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "IcePlant.API")
                    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                    .CreateLogger();

                builder.Host.UseSerilog(Log.Logger);

                // ── CONTROLLER & JSON CONFIGURATION ────────────────────────────────
                builder.Services.AddControllers()
                    .AddJsonOptions(o =>
                    {
                        o.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    });

                // ── CORS CONFIGURATION ─────────────────────────────────────────────
                builder.Services.AddCors(options =>
                {
                    var configuredOrigins = builder.Configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();

                    if (configuredOrigins.Length == 0)
                    {
                        var commaSeparated = builder.Configuration["Cors:AllowedOriginsCsv"];
                        if (!string.IsNullOrWhiteSpace(commaSeparated))
                        {
                            configuredOrigins = commaSeparated
                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        }
                    }

                    var allowedOrigins = configuredOrigins
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .ToArray();

                    if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
                    {
                        allowedOrigins =
                        [
                            "http://localhost:5173",
                            "http://localhost:3000",
                            "http://127.0.0.1:5173"
                        ];
                    }
                    else if (allowedOrigins.Length == 0)
                    {
                        Log.Warning(
                            "No CORS origins configured. Set Cors__AllowedOrigins__0 (and __1, __2, …) in environment variables.");
                    }

                    options.AddPolicy("AllowedFrontend", policy =>
                    {
                        policy
                            .WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithExposedHeaders("X-Total-Count", "X-Page-Count", "X-Request-ID");
                    });
                });

                // ── SWAGGER/OPENAPI CONFIGURATION ──────────────────────────────────
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Ice Factory Management System API",
                        Version = "v1.0",
                        Description = "API for managing ice factory operations",
                        Contact = new OpenApiContact
                        {
                            Name = "Ice Factory Team",
                            Email = "support@icefactory.com"
                        }
                    });

                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token."
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });

                    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
                    options.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
                    options.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });

                    var xmlFile = System.IO.Path.Combine(System.AppContext.BaseDirectory, "IceFactoryManagmentSystem.xml");
                    if (System.IO.File.Exists(xmlFile))
                    {
                        options.IncludeXmlComments(xmlFile);
                    }
                });

                // ── INFRASTRUCTURE & DATABASE ──────────────────────────────────────
                builder.Services.AddInfrastructure(builder.Configuration);



                // ── RATE LIMITING CONFIGURATION ────────────────────────────────────
                builder.Services.AddRateLimiter(options =>
                {
                    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
                        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: context.User?.FindFirst("sub")?.Value
                                ?? context.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown",
                            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 100,
                                Window = TimeSpan.FromMinutes(1)
                            }));

                    options.OnRejected = async (context, _) =>
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            error = "Rate limit exceeded. Maximum 100 requests per minute.",
                            retryAfter = "60"
                        });
                    };
                });

                // ── HSTS (production) ──────────────────────────────────────────────
                builder.Services.AddHsts(options =>
                {
                    options.MaxAge = TimeSpan.FromDays(365);
                    options.IncludeSubDomains = true;
                    options.Preload = true;
                });

                // ── HEALTH CHECKS ──────────────────────────────────────────────────
                builder.Services.AddHealthChecks()
                    .AddDbContextCheck<IcePlant.Infrastructure.Persistence.AppDbContext>(name: "Database");

                // ── APPLICATION SERVICES ───────────────────────────────────────────
                builder.Services.AddScoped<IcePlant.Application.Services.AttendanceService>();
                builder.Services.AddScoped<IcePlant.Application.Services.SaleService>();
                builder.Services.AddScoped<IcePlant.Application.Services.ExpenseService>();
                builder.Services.AddScoped<IcePlant.Application.Services.ReportService>();
                builder.Services.AddScoped<IcePlant.Application.Services.DashboardService>();
                builder.Services.AddScoped<IcePlant.Application.Interfaces.IAuthService, IcePlant.Application.Services.AuthService>();

                var app = builder.Build();

                // ── MIDDLEWARE PIPELINE ────────────────────────────────────────────

                // 1. Global exception handling (MUST be first)
                app.UseGlobalExceptionHandler();

                // 2. Request ID tracking
                app.UseMiddleware<RequestIdMiddleware>();

                // 3. Serilog request logging
                app.UseSerilogRequestLogging();

                // 4. HTTPS & Security Headers (production only)
                if (!app.Environment.IsDevelopment())
                {
                    app.UseHsts();
                    app.UseHttpsRedirection();

                }

                // 5. Swagger — always enabled (remove if you want dev-only)
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ice Factory API v1");
                    options.RoutePrefix = "swagger";
                });

                // 6. Security headers
                app.Use(async (context, next) =>
                {
                    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                    context.Response.Headers["X-Frame-Options"] = "DENY";
                    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                    await next();
                });

                // 7. CORS (must come before auth)
                app.UseCors("AllowedFrontend");

                // 8. Rate limiting
                app.UseRateLimiter();

                // 9. Authentication & Authorization
                app.UseAuthentication();
                app.UseAuthorization();

                // 10. Health checks endpoint
                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";
                        var json = JsonSerializer.Serialize(new
                        {
                            status = report.Status.ToString(),
                            checks = report.Entries.Select(e => new
                            {
                                name = e.Key,
                                status = e.Value.Status.ToString(),
                                description = e.Value.Description
                            })
                        });
                        await context.Response.WriteAsync(json);
                    }
                });

                // 11. Controllers
                app.MapControllers();

                // ── DATABASE INITIALIZATION ────────────────────────────────────────
                Log.Information("Applying database migrations and seeding...");
                await app.Services.ApplyMigrationsAsync();
                Log.Information("Database initialization completed");

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}