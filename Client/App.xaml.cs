
using Penta_ClientLib.Interfaces;

namespace Client
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }//чтобы сервисы можно было дергать где угодно

        public App(IServiceProvider services)
        {
            InitializeComponent();

            Services = services;
            MainPage = new AppShell();

            TryAutoLogin();
        }

        private async void TryAutoLogin()
        {
            var _clientFacade = Services.GetRequiredService<IClientFacade>();

            var result = await _clientFacade.Login();
            if (result.Item1)
            {
                await Shell.Current.GoToAsync("//ChatsPage");//перенаправление на страницу Чатов
            }
        }

    }
}
