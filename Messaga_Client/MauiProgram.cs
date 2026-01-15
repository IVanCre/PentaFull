using Messaga_Client.Interfaces;
using Messaga_Client.Services;
using Messaga_Client.Services.DataProviders;
using Messaga_Client.Services.Repository;
using Microsoft.Extensions.Logging;

namespace Messaga_Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp(MauiAppBuilder builder = null)
        {
            if (builder == null)//на некоторых платформах может понадобиться самим добавить свои сервисы заранее в билдер(пример: android)
                builder = MauiApp.CreateBuilder();

            //ВНИМАНИЕ! Некоторые сервисы реализуются в платформенной части на более ранних этапах выполнения!
            AddServices(builder.Services);

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static void AddServices(IServiceCollection services)
        {
            services.AddSingleton<DBContext>();

            services.AddSingleton<IMessageProvider,MessageProvider>();
            services.AddSingleton<SettingsProvider>();
            services.AddSingleton<IUserProvider, UserProvider>();

            services.AddSingleton<IAuthManager, AuthManager>();
            services.AddSingleton<IWebClient, WebClient>();


        }
    }
}
