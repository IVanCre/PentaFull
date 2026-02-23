
using Client.Interfaces;
using Client.UIElements;
using Penta_ClientLib.Interfaces;
using System.Collections.ObjectModel;

namespace Client.Pages
{

    public partial class ContactsPage : ContentPage
	{
        public ObservableCollection<ContactInfo> ContactsList { get; set; } = new();
        private IClientFacade _clientFacade;
        private IUINotificator _actionMenuSelector;

        public ContactsPage()
		{
			InitializeComponent();

            _clientFacade = App.Services.GetRequiredService<IClientFacade>();
            _actionMenuSelector = App.Services.GetRequiredService<IUINotificator>();
            BindingContext = this;
        }
        
        protected override async void OnAppearing()//вызываетс€ при отображении страницы
        {
            base.OnAppearing();

            if (string.IsNullOrEmpty(MyContactIDLabel.Text))
                MyContactIDLabel.Text ="Your ContactID: "+ await _clientFacade.GetMyContactID();

            ContactsList?.Clear();//сносим старое
            var result =await _clientFacade.GetAllContactsAsync();//т.к. чаты могут быть созданы в длругом месте тоже
            foreach (var contact in result)
                ContactsList.Add(contact);
        }

        private async void OnShortClick(object sender, EventArgs e)
        {
            var button = sender as Button;
            var contact = (ContactInfo)button?.BindingContext;

            int chatID = 0;
            var allChats =await _clientFacade.GetAllChatsInfoAsync();
            var findedChat = allChats.FirstOrDefault(x => x.ChatName == contact.UserContactID);//попытаемс€ найти уже существующий
            if(findedChat!=null)
                chatID=findedChat.ID;
            else
                chatID = await _clientFacade.CreatePrivateChatAsync(contact.UserName);//создаем новый чат

            if (chatID != 0)//успешно создан
            {
                await Navigation.PushAsync(new ActiveChatPage(contact.UserName, chatID));//переходим в чат
            }
        }
        private async void OnLongClick(object sender, EventArgs e)
        {

            var button = sender as LongButton;
            var contact = (ContactInfo)button?.BindingContext;

            if (await _actionMenuSelector.ShowConfirmDialog("", "”далить контакт?", "ƒа", "Ќет"))
            {
                ContactsList.Remove(contact);
                await _clientFacade.DeleteContactAsync(contact.UserName);
            }
        }


        private async void OnContactCreateClick(object sender, EventArgs e)
		{
            await Navigation.PushAsync(new ContactCreatorPage(ContactsList, _clientFacade));
        }
    }
}