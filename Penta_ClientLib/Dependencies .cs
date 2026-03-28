using Penta_ClientLib.Interfaces;
using Penta_ClientLib.Services;
using Microsoft.Extensions.DependencyInjection;
using Penta_ClientLib.Repository;


namespace Penta_ClientLib
{
    public static class  Dependencies 
    {
        public static IServiceCollection AddClientDependencies(this IServiceCollection services)
        {
            services.AddSingleton<ILogger, SysLogger>();
            services.AddSingleton<ISettingsHolder, DBManager>();
            services.AddSingleton<IChatHolder, DBManager>();
            services.AddSingleton<IMessageHolder, DBManager>();
            services.AddSingleton<IContactHolder, DBManager>();

            services.AddSingleton<ISettingsProvider, SettingsProvider>();

            services.AddSingleton<IAccountManager, AccountManager>();
            services.AddSingleton<IChatManager, ChatManager>();
            services.AddSingleton<IMessageReciever, MessageReciever>();
            services.AddSingleton<IWebClient, WebClient>();

            services.AddSingleton<IClientFacade, ClientFacade>();

            return services;
        }
    }
}
