
using Client.Interfaces;
using Penta_ClientLib.DataStructs;
using Penta_ClientLib.Interfaces;
using System.Collections.ObjectModel;
using Client.UIElements;
using MessageLib;
using Client.Platforms.Android.PushServices;


namespace Client.Pages
{
    public partial class ChatsPage : ContentPage
    {
        public ObservableCollection<ChatInfo> ChatsList { get;set; } = new();
        private IClientFacade _clientFacade;
        private IDialogManager _actionMenuSelector;


        public ChatsPage()
        {
            InitializeComponent();

            BindingContext = this;

            var itemToRemove = Shell.Current.Items.FirstOrDefault(x => x.Route == "LoadingPage");//удаляем заглушку
            if (itemToRemove != null)
                Shell.Current.Items.Remove(itemToRemove);
        }


        protected override async void OnAppearing()//вызывается при отображении страницы
        {
            base.OnAppearing();

            App.Services.GetRequiredService<INotifyHelper>()?.SkipAllNotifications();

            if (_clientFacade == null)
            {
                _clientFacade = App.Services.GetRequiredService<IClientFacade>();
                _actionMenuSelector = App.Services.GetRequiredService<IDialogManager>();
                _clientFacade.MessageAddedToChat += ShowNewMessageInChat;
                _clientFacade.CreatedNewChat += ChatCreated;
                _clientFacade.ContactChanged += TryUpdateChatName;
            }

            var findedChats = await _clientFacade.GetAllChatsInfoAsync();
            if(findedChats.Count!= ChatsList.Count)
            {
                ContactInfo identityContact;
                var findedContacts = await _clientFacade.GetAllContactsAsync();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ChatsList.Clear();//так быстрее
                    foreach (var chatInfo in findedChats)
                    {
                        identityContact = findedContacts.FirstOrDefault(x => x.UserContactID == chatInfo.ChatName);
                        if (identityContact != null)
                            ChatsList.Add(new ChatInfo(chatInfo.ID, identityContact.UserName, chatInfo.ChatType, chatInfo.HaveUnreadedMessages));//прописываем имя юзера из контакта
                        else
                            ChatsList.Add(chatInfo);//оставляем как есть
                    }
                });
            }

        }

        private async void OnShortClick(object sender, EventArgs e)
        {
            LoaderSpin.IsRunning = true;
            var button = sender as LongButton;
            var backClr = button.BackgroundColor;
            button.BackgroundColor = Color.Parse("LightGray");
            var chat = (ChatInfo)button?.BindingContext;

            await Navigation.PushAsync(new ActiveChatPage(chat));//сразу переходим в переписку чата
            button.BackgroundColor = backClr;
            LoaderSpin.IsRunning = false;
        }
        private async void OnLongClick(object sender, EventArgs e)
        {
            var button = sender as LongButton;
            var chat = (ChatInfo)button?.BindingContext;

            var backClr = button.BackgroundColor;
            button.BackgroundColor = Color.Parse("LightGray");
            if (await _actionMenuSelector.ShowConfirmDialog("", "Удалить чат?", "Да", "Нет"))
            {
                MainThread.BeginInvokeOnMainThread(() => ChatsList.Remove(chat));
                if (chat.ChatType == ChatType.Group)
                    await _clientFacade.DeleteGroupChatAsync(chat.ID);
                else
                    await _clientFacade.DeletePrivateChatAsync(chat.ID);
            }
            button.BackgroundColor = backClr;
        }

        private void TryUpdateChatName(string oldName, string newName)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var finded = ChatsList.FirstOrDefault(x => x.ChatName == oldName);
                if (finded != null)
                    finded.ChatName = newName;
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

            ChatInfo createdChatInfo=null;
            if (findedContact != null)
                createdChatInfo = new ChatInfo(chatID, findedContact.UserName, ChatType.Private,false);//сохраняем с указанным именем(т.к. есть аналогичный контакт)
            else
            {
                var findedChat = await _clientFacade.GetChatByID(chatID);
                if(findedChat!=null)
                    createdChatInfo = new ChatInfo(chatID, chatName, findedChat.ChatType,false);//сохраняем с исходным именем
            }

            if(createdChatInfo!=null)
                MainThread.BeginInvokeOnMainThread(() => ChatsList.Add(createdChatInfo));
        }

        private void ShowNewMessageInChat(int chatID, Message mesage)
        {
            var index = ChatsList.IndexOf(ChatsList.FirstOrDefault(x => x.ID == chatID));
            if (index != -1)
            {
                var chat = ChatsList[index];
                chat.HaveUnreadedMessages = true;
                ChatsList[index] = chat;//пересоздание приведет к автоизменению UI
            }
        }
    }
}