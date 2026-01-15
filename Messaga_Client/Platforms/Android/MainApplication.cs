using Android.App;
using Android.Runtime;


namespace Messaga_Client
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
            //тут в builder.Services можно пропихнуть специфичные сервисы, которые необходимы конкретной платформе
            return MauiProgram.CreateMauiApp(builder);
        }

    }
}
