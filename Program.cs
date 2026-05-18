using IcePlant.Infrastructure;
using IcePlant.Infrastructure.JsonConverters;
using IceFactoryManagmentSystem.Middleware;
using Microsoft.AspNetCore.Identity;

namespace IceFactoryManagmentSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(o => {
                    o.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add Infrastructure layer and Database
            builder.Services.AddInfrastructure(builder.Configuration);

            // Register Application services
            builder.Services.AddScoped<IcePlant.Application.Services.AttendanceService>();
            builder.Services.AddScoped<IcePlant.Application.Services.SaleService>();
            builder.Services.AddScoped<IcePlant.Application.Services.ExpenseService>();
            builder.Services.AddScoped<IcePlant.Application.Services.ReportService>();
            builder.Services.AddScoped<IcePlant.Application.Services.DashboardService>();
            builder.Services.AddScoped<IcePlant.Application.Interfaces.IAuthService, IcePlant.Application.Services.AuthService>();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.

            // Global error handling — must be first in the pipeline
            app.UseGlobalExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            // Apply migrations and seed db on startup
            await app.Services.ApplyMigrationsAsync();

            await app.RunAsync();
        }
    }
}
