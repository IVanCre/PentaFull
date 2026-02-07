using Android.App;
using Android.Runtime;
using Penta_ClientLib.Interfaces;
using Client.Platforms.Android.Repository;


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

            //библиотека Penta_ClientLib предоставляет способ реализации указанных хранилищ самому разрабу
            builder.Services.AddSingleton<ISettingsHolder, DBManager>();
            builder.Services.AddSingleton<IChatHolder, DBManager>();
            builder.Services.AddSingleton<IMessageHolder, DBManager>();
            builder.Services.AddSingleton<IContactHolder, DBManager>();


            return MauiProgram.CreateMauiApp(builder);
        }
    }
}
