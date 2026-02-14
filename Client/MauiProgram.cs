using Client.Services;
using Penta_ClientLib;
using Client.Interfaces;

namespace Client
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp(MauiAppBuilder builder = null)
        {
            if (builder == null)//на некоторых платформах может понадобиться самим добавить свои сервисы заранее в билдер
                builder = MauiApp.CreateBuilder();
            
            builder.Services.AddClientDependencies();
            builder.Services.AddSingleton<IUINotificator,NotificationService>();
            builder.UseMauiApp<App>();

            return builder.Build();
        }
    }
}
