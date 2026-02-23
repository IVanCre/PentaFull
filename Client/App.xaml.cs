
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
        }
    }
}
