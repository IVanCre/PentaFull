using Android.App;
using Android.Runtime;
using Client.Interfaces;
using Client.Platforms.Android.PushServices;
using Client.Platforms.Android.UpdateServices;


namespace Client.Platforms.Android
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            //тут можно добавить специфичные платформо-зависимые сервисы
            builder.Services.AddSingleton<IUpdateManager, AndroidUpdateManager>();
            builder.Services.AddScoped<DeviceTokenSender>();
            builder.Services.AddSingleton<ISoundManager, AndroidSoundManager>();
            builder.Services.AddScoped<IPlatformConfigurator, AndroidConfigurator>();

            return MauiProgram.CreateMauiApp(builder);
        }

    }
}
