using Penta_ClientLib.Interfaces;

using Penta_ClientLib.Services;
using Microsoft.Extensions.DependencyInjection;


namespace Penta_ClientLib
{
    public static class  Dependencies 
    {
        public static IServiceCollection AddClientDependencies(this IServiceCollection services)
        {
            //тут добавляем в контейнер наши классы


            services.AddSingleton<IAccountManager, AccountManager>();
            services.AddSingleton<IChatManager, ChatManager>();
            services.AddSingleton<IContactManager, ContactManager>();
            services.AddSingleton<IMessageProcessor, MessageSaver>();
            services.AddSingleton<IWebClient, WebClient>();


            return services;
        }
    }
}
