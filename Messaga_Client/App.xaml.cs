namespace Messaga_Client
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
            var authMngr = Services.GetService<IAuthManager>();
            var settings = Services.GetService<SettingsHolder>();

            if (authMngr != null && settings != null)
            {
                var currentUser = settings.UserCreds;
                if (currentUser != null)
                {
                    var result = await authMngr.TryLogin(currentUser.UserLogin, currentUser.Pass);
                    await Shell.Current.GoToAsync("//ChatsPage");
                }
            }
        }
    }
}

