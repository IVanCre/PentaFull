
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
        private IDialogManager _actionMenuSelector;

        public ContactsPage()
		{
			InitializeComponent();

            _clientFacade = App.Services.GetRequiredService<IClientFacade>();
            _actionMenuSelector = App.Services.GetRequiredService<IDialogManager>();
            BindingContext = this;
        }
        
        protected override async void OnAppearing()//вызывается при отображении страницы
        {
            base.OnAppearing();

            if (string.IsNullOrEmpty(MyContactIDLabel.Text))
            {
                MyContactIDLabel.Text = "Ваш ContactID: " + await _clientFacade.GetMyContactID();

                var result = await _clientFacade.GetAllContactsAsync();//т.к. чаты могут быть созданы в длругом месте тоже

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ContactsList?.Clear();//сносим старое
                    foreach (var contact in result)
                        ContactsList.Add(contact);
                });
            }
        }

        private async void OnLongClick(object sender, EventArgs e)
        {
            var button = sender as LongButton;
            var contact = (ContactInfo)button?.BindingContext;

            if (await _actionMenuSelector.ShowConfirmDialog("", "Удалить контакт?", "Да", "Нет"))
            {
                MainThread.BeginInvokeOnMainThread(() => ContactsList.Remove(contact));
                await _clientFacade.DeleteContactAsync(contact.UserName);
            }
        }
        private async void OnShowContact(object sender, EventArgs e)
        {
            var button = sender as LongButton;
            var contact = (ContactInfo)button?.BindingContext;

            await Navigation.PushAsync(new ContactCreatorPage(contact));
        }

        private async void OnContactCreateClick(object sender, EventArgs e)
		{
            await Navigation.PushAsync(new ContactCreatorPage(ContactsList, _clientFacade));
        }
    }
}