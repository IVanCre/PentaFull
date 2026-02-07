
using Penta_ClientLib.Interfaces;
using System.Collections.ObjectModel;

namespace Client.Pages
{

    public partial class ContactsPage : ContentPage
	{
        public ObservableCollection<ContactInfo> ContactsList { get; set; } = new();
        private IContactHolder _contactHolder;
        private IClientFacade _clientFacade;
        public string MyContactID { get; private set; }


        public ContactsPage()
		{
			InitializeComponent();

            _clientFacade = App.Services.GetRequiredService<IClientFacade>();
            _contactHolder= App.Services.GetRequiredService<IContactHolder>();
            BindingContext = this;
        }

        protected override async void OnAppearing()//вызывается при отображении страницы
        {
            base.OnAppearing();

            if (string.IsNullOrEmpty(MyContactID))
                MyContactID = await _clientFacade.GetMyContactID();

            ContactsList?.Clear();//сносим старое
            var result =await _contactHolder.GetAllContacts();//т.к. чаты могут быть созданы в длругом месте тоже
            foreach (var contact in result)
                ContactsList.Add(contact);
        }

        private async void OnContactClick(object sender, EventArgs e)
        {
            var button = sender as Button;
            var contact = (ContactInfo)button?.BindingContext;

            var chatID = await _clientFacade.CreatePrivateChat(contact.UserName);//сразу создаем новый чат
            if (chatID != 0)
            {
                await Navigation.PushAsync(new ActiveChatPage(contact.UserName, chatID));//переходим в чат
            }
        }

		private async void OnContactCreateClick(object sender, EventArgs e)
		{
            await Navigation.PushAsync(new ContactCreatorPage(ContactsList, _contactHolder));
        }
    }
}