
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Interfaces;
using System.Collections.ObjectModel;

namespace Client.Pages
{
    public partial class ChatsPage : ContentPage
    {
        public ObservableCollection<ChatInfo> ChatsList { get;set; } = new();
        private IClientFacade _clientFacade;


        public ChatsPage()
        {
            InitializeComponent();

            _clientFacade = App.Services.GetRequiredService<IClientFacade>();
            _clientFacade.CreatedNewChat += ChatCreated;
            BindingContext = this;
        }
        protected override async void OnAppearing()//вызывается при отображении страницы
        {
            base.OnAppearing();

            ChatsList?.Clear();
            var result = await _clientFacade.GetAllChatsInfo();
            foreach (var chatInfo in result.Item1)
                ChatsList.Add(chatInfo);//т.к. чаты могут быть созданы в длругом месте тоже
        }

        private async void OnChatClick(object sender, EventArgs e)
        {
            var button = sender as Button;
            var chat = (ChatInfo)button?.BindingContext;

            await Navigation.PushAsync(new ActiveChatPage(chat.ChatName,chat.ID));//сразу переходим в переписку чата
        }

        private async void OnStartNewClick(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChatCreatorPage());//тут отправим запрос  на создание чата
        }

        private void ChatCreated(int chatID, string chatName)
        {
            ChatsList.Add(new ChatInfo(chatID,chatName));//тут обработает ответ о создании чата 
        }
    }
}