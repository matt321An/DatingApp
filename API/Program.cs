using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
             var host = CreateHostBuilder(args).Build();
             using var scope = host.Services.CreateScope();
             var services = scope.ServiceProvider;
            
            // Seed the database 
             try
             {
                var context = services.GetRequiredService<DataContext>();

                // Apply any migrations and create the DB if it doesn't exist
                await context.Database.MigrateAsync();
                
                await Seed.SeedUsers(context);
             }
             catch (Exception ex)
             {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured during migration");
             }

            // Run the application
             await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
