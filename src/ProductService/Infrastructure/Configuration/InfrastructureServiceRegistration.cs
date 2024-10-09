using ProductService.Infrastructure.Interfaces;
using ProductService.Infrastructure.Repositories;
using ProductService.Services.Interfaces;

namespace ProductService.Infrastructure.Configuration
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection ConfigureInfrastructureService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IProductService, Services.ProductService>();


            services.AddScoped(typeof(IAsyncRepository<>), typeof(AsyncRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
