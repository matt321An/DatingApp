using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            // Add the JWT token service and other repositories
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings")); // point the program from where to take the configuration
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>(); // service to communicate with cloudinary
            services.AddScoped<ILikesRepository, LikesRepository>(); // service for the 'like' feature
            services.AddScoped<LogUserActivity>(); // service to update lastActive property of logged user
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            // Add the connection to the DB
            services.AddDbContext<DataContext>(options => 
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}