
using Penta_ClientLib.Interfaces;
using Client.Interfaces;
using MessageLib;
using Client.Services;

namespace Client.Pages
{
    /// <summary>
    /// Страница-заглушка для подготовки клиента в рабочее состояние
    /// </summary>
    public partial class LoadingPage : ContentPage
    {
        public LoadingPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var _clientFacade = App.Services.GetRequiredService<IClientFacade>();
            ConfigureServices(_clientFacade);
            await TryAutoLogin(_clientFacade);
        }

        private void ConfigureServices(IClientFacade _clientFacade)
        {
            var _soundManager = App.Services.GetRequiredService<ISoundManager>();
            _clientFacade.MessageAddedToChat += (int chatID, Message mesage) =>
            {
                if(App.DeviceIsSleep)//звук оповещения только с погасшим экраном
                    _soundManager?.InputMessageNotify();
            };
        }

        //попытаемся сразу перейти на нужную страницу и параллельно подключиться к серваку
        private async Task TryAutoLogin(IClientFacade _clientFacade)
        {
            var _uiNotificator = App.Services.GetService<IDialogManager>();
            _clientFacade.SysLogRecieved += (LogEventType type, string text, DateTime timestamp) =>
            {
                _uiNotificator.ShowMessage("Систенмый лог", $"{timestamp.ToString("HH:mm:ss")} {text}", "ок");//чтобы события отслоеживались сразу
            };

            var myContact = await _clientFacade.GetMyContactID();
            if (!string.IsNullOrEmpty(myContact))
            {
                _ = _clientFacade.ConnectToServerAsync();
                await Shell.Current.GoToAsync("//ChatsPage");
            }
            else
                await Shell.Current.GoToAsync("//AuthPage");
        }

    }
}