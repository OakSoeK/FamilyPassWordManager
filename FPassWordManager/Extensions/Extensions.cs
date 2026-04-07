using FPassWordManager.Models;
using FPassWordManager.Services;
using Microsoft.AspNetCore.Identity;

namespace FPassWordManager.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<IAccessService, AccessService>();
            services.AddScoped<ICredentialService, CredentialService>();
            services.AddScoped<IWebCredentialService, WebCredentialService>();
            services.AddScoped<ICreditDebitCardService, CreditDebitCardService>();
            services.AddScoped<ISecurityKeyService, SecurityKeyService>();
            services.AddScoped<IPinService, PinService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            return services;
        }
    }
}
