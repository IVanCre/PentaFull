using Client.Services;
using Penta_ClientLib.Interfaces;


namespace Client.Pages
{
	public partial class ChatCreatorPage : ContentPage
	{
        private IClientFacade _clientFacade;

        public ChatCreatorPage()
		{
			InitializeComponent();
            _clientFacade = App.Services.GetRequiredService<IClientFacade>();
        }

        private async void OnCreateChat(object sender, EventArgs e)
        {
            var chatName = ChatName.Text;
            if (!string.IsNullOrEmpty(chatName))
            {
                var sended = await _clientFacade.CreateGroupChat(chatName);
                if (sended.Item1)
                    await Navigation.PopAsync();
                else
                    await NotificationService.ShowMessage("Ошибка",$"Ошибка при отправке запроса: {sended.Item2.Message}","ОК");
            }
        }
    }
}