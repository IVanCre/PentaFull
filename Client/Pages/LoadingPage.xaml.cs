
using Penta_ClientLib.Interfaces;
using Client.Interfaces;
using MessageLib;
using Client.Pages;
using Client.Platforms.Android.PushServices;

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
            _= Task.Run(()=>_clientFacade.DeleteOldMessages(7));//пока с дефолтом
            ConfigureNotifications(_clientFacade);
            await TryAutoLogin(_clientFacade);
        }

        private void ConfigureNotifications(IClientFacade _clientFacade)
        {
            var notifyService = App.Services.GetRequiredService<INotifyHelper>();
            _clientFacade.MessageAddedToChat += async (int chatID, Message mesage) =>
            {
                var currentPage = Shell.Current.CurrentPage;
                string text = $"Новое сообщение.Нажмите для просмотра. {DateTime.Now.ToString("HH:mm:ss")}";
                if (currentPage is ActiveChatPage currChat)
                {
                    if (currChat.ChatID != chatID)
                        notifyService.UpdateNotification(mesage.FromID, currChat.ChatName, text);//звук+пуш
                    else
                        App.Services.GetRequiredService<ISoundManager>()?.InputMessageNotify();//только звук
                }
                else
                {
                    var senderContactName = await _clientFacade.FindUserPseudonimeByID(mesage.FromID);
                    notifyService.UpdateNotification(mesage.FromID, senderContactName, text);
                }
            };
        }

        //попытаемся сразу перейти на нужную страницу и параллельно подключиться к серваку
        private async Task TryAutoLogin(IClientFacade _clientFacade)
        {
            var _uiNotificator = App.Services.GetService<IDialogManager>();

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