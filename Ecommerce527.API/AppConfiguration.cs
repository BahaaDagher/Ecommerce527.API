using ECommerce527.API.Repositories;
using ECommerce527.API.Utilities;
using ECommerce527.API.Utilities.DbSeeder;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Ecommerce527.API
{
    public static class AppConfiguration
    {
        public static void RegisterConfig(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Category>, Repository<Category>>();
            services.AddScoped<IRepository<Brand>, Repository<Brand>>();
            services.AddScoped<IRepository<ECommerce527.API.Models.Product>, Repository<ECommerce527.API.Models.Product>>();
            services.AddScoped<IRepository<Cart>, Repository<Cart>>();
            services.AddScoped<IRepository<Promotion>, Repository<Promotion>>();
            services.AddScoped<IRepository<ApplicationUserOtp>, Repository<ApplicationUserOtp>>();
            services.AddScoped<IProductSubImageRepository, ProductSubImageRepository>();
            services.AddScoped<IProductColorRepository, ProductColorRepository>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IDbInitializer, DbInitializer>();
        }
    }
}
