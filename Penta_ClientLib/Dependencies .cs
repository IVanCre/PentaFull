using Penta_ClientLib.Interfaces;

using Penta_ClientLib.Services;
using Microsoft.Extensions.DependencyInjection;


namespace Penta_ClientLib
{
    public static class  Dependencies 
    {
        /// <summary>
        /// Добавляет основные сервисы 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IAccountManager, AccountManager>();
            services.AddSingleton<IChatManager, ChatManager>();
            services.AddSingleton<IContactConverter, ContactConverter>();
            services.AddSingleton<IMessageReciever, MessageReciever>();
            services.AddSingleton<IWebClient, WebClient>();

            services.AddSingleton<IClientFacade, ClientFacade>();

            return services;
        }
    }
}
