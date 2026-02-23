using Android.App;
using Android.Runtime;


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
            builder.Services.AddScoped<DeviceTokenSender>();

            return MauiProgram.CreateMauiApp(builder);
        }

    }
}
