
using Client.Interfaces;
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Interfaces;
using System.Collections.ObjectModel;
using Client.UIElements;


namespace Client.Pages
{
    public partial class ChatsPage : ContentPage
    {
        public ObservableCollection<ChatInfo> ChatsList { get;set; } = new();
        private IClientFacade _clientFacade;
        private IUINotificator _actionMenuSelector;

        public ChatsPage()
        {
            InitializeComponent();

            _clientFacade = App.Services.GetRequiredService<IClientFacade>();
            _actionMenuSelector = App.Services.GetRequiredService<IUINotificator>();
            _clientFacade.CreatedNewChat += ChatCreated;
            _clientFacade.ContactChanged += TryUpdateChatName;
            BindingContext = this;
        }


        protected override async void OnAppearing()//вызываетс€ при отображении страницы
        {
            base.OnAppearing();

            var findedChats = await _clientFacade.GetAllChatsInfoAsync();
            var findedContacts = await _clientFacade.GetAllContactsAsync();
            ContactInfo identityContact;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ChatsList?.Clear();
                foreach (var chatInfo in findedChats)
                {
                    identityContact = findedContacts.FirstOrDefault(x => x.UserContactID == chatInfo.ChatName);
                    if (identityContact != null)
                        ChatsList.Add(new ChatInfo(chatInfo.ID, identityContact.UserName, chatInfo.IsGroupChat));//прописываем им€ юзера из контакта
                    else
                        ChatsList.Add(chatInfo);//оставл€ем как есть
                }
            });
        }

        private async void OnShortClick(object sender, EventArgs e)
        {
            var button = sender as LongButton;
            var chat = (ChatInfo)button?.BindingContext;

            await Navigation.PushAsync(new ActiveChatPage(chat.ChatName,chat.ID));//сразу переходим в переписку чата
        }
        private async void OnLongClick(object sender, EventArgs e)
        {
            var button = sender as LongButton;
            var chat = (ChatInfo)button?.BindingContext;

            if(await _actionMenuSelector.ShowConfirmDialog("","”далить чат?","ƒа","Ќет"))
            {
                MainThread.BeginInvokeOnMainThread(() => ChatsList.Remove(chat));
                if(chat.IsGroupChat)
                    await _clientFacade.DeleteGroupChatAsync(chat.ID);
                else
                    await _clientFacade.DeletePrivateChatAsync(chat.ID);
            }
        }

        private void TryUpdateChatName(string oldName, string newName)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var finded = ChatsList.FirstOrDefault(x => x.ChatName == oldName);
                if (finded != null)
                {
                    ChatsList.Remove(finded);
                    ChatsList.Add(new ChatInfo(finded.ID, newName, finded.IsGroupChat));//чтобы перерисовку вызвать
                }
            });
        }

        private async void OnCreateNew(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChatCreatorPage());//тут отправим запрос  на создание чата
        }

        private async void ChatCreated(int chatID, string chatName)//обработка ответа на создание группового чата 
        {
            var findedContacts = await _clientFacade.GetAllContactsAsync();
            var findedContact = findedContacts.FirstOrDefault(x => x.UserContactID == chatName);

            ChatInfo createdChatInfo;
            if (findedContact != null)
                createdChatInfo = new ChatInfo(chatID, findedContact.UserName, false);//сохран€ем с указанным именем(т.к. есть аналогичный контакт)
            else
            {
                var findedChat = await _clientFacade.GetChatByID(chatID);
                createdChatInfo = new ChatInfo(chatID, chatName, findedChat.IsGroupChat);//сохран€ем с исходным именем
            }

            MainThread.BeginInvokeOnMainThread(() => ChatsList.Add(createdChatInfo));
        }
    }
}