
using Client.Services;

namespace Client
{


    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }//чтобы сервисы можно было дергать где угодно
        public static bool DeviceIsSleep { get; private set; }

        public App(IServiceProvider services)
        {
            InitializeComponent();

            Services = services;
            MainPage = new AppShell();
        }

        protected override void OnSleep()//передаем состояние системы
        {
            base.OnSleep();
            DeviceIsSleep = true;
        }

        // Вызывается, когда устройство просыпается или приложение разворачивается
        protected override void OnResume()
        {
            base.OnResume();
            DeviceIsSleep = false;
        }
    }
}
