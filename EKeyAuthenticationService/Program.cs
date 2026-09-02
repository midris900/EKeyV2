
using EKeyAuthenticationService.Model;
using EKeyAuthenticationService.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;


namespace EKeyAuthenticationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            var builder = WebApplication.CreateBuilder(args);

            // Define custom log directory (Change as per your requirement)
            var logDirectory = builder.Configuration["Serilog:LogDirectory"] ?? Path.Combine(builder.Environment.ContentRootPath, "Logs");
            Directory.CreateDirectory(logDirectory); // Ensure the directory exists

            // Configure Serilog for File Logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(logDirectory, "ekey-auth-log-.txt"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();

            // Step 2: Register Serilog in the Host
            builder.Host.UseSerilog();


            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpClient();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<AuthServiceGCC>();
            builder.Services.AddScoped<AuthSessionService>();

            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

            // Add CORS services
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          if (allowedOrigins != null && allowedOrigins.Length > 0)
                          {
                              policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                          }
                      });
            });

            var app = builder.Build();

            app.UseSerilogRequestLogging(); // Logs HTTP Requests

            // Configure the HTTP request pipeline.

            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI();
            //}
            app.UseHttpsRedirection();

            app.UseCors(MyAllowSpecificOrigins);

            app.MapControllers();
            app.Run();
        }
    }
}
