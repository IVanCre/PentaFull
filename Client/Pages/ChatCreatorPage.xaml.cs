using Client.Interfaces;
using Client.Services;
using Penta_ClientLib.Interfaces;


namespace Client.Pages
{
	public partial class ChatCreatorPage : ContentPage
	{
        private IClientFacade _clientFacade;
        private IUINotificator _notifier;

        public ChatCreatorPage()
		{
			InitializeComponent();
            _clientFacade = App.Services.GetRequiredService<IClientFacade>();
            _notifier = App.Services.GetRequiredService<IUINotificator>();
        }

        private async void OnCreateChat(object sender, EventArgs e)
        {
            var chatName = ChatName.Text;
            if (!string.IsNullOrEmpty(chatName))
            {
                var sended = await _clientFacade.CreateGroupChatAsync(chatName);
                if (sended.Item1)
                    await Navigation.PopAsync();
                else
                    await _notifier.ShowMessage("Ошибка",$"Ошибка при отправке запроса: {sended.Item2?.Message}","ОК");
            }
        }
    }
}