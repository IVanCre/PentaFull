using MessageClientLib.Interfaces;

using MessageClientLib.Services;
using Microsoft.Extensions.DependencyInjection;


namespace MessageClientLib
{
    public static class  Dependencies 
    {
        public static IServiceCollection AddClientDependencies(this IServiceCollection services)
        {
            //тут добавляем в контейнер наши классы


            services.AddSingleton<IAccountManager, AccountManager>();
            services.AddSingleton<IWebClient, WebClient>();


            return services;
        }
    }
}
