
using Penta_ClientLib.Interfaces;
using System.Collections.ObjectModel;


namespace Client.Pages;

public partial class ContactCreatorPage : ContentPage
{
    private IClientFacade _facade;
    private Collection<ContactInfo> _uiContacts;

    public ContactCreatorPage(
        Collection<ContactInfo> uiContacts,
        IClientFacade contactHolder)
	{
		InitializeComponent();
        _facade = contactHolder;
        _uiContacts= uiContacts;
    }

    public ContactCreatorPage(ContactInfo contactInfo)//для просмотра\редактирования контакта
    {
        InitializeComponent();
        Title = "Просмотр контакта";

        UserName.Text = contactInfo.UserName;
        UserName.IsEnabled = false;
        UserName.TextColor = Color.FromArgb("#000000");

        ContactID.Text= contactInfo.UserContactID;
        ContactID.IsEnabled = false;
        ContactID.TextColor = Color.FromArgb("#000000");

        createBtn.IsVisible = false;
    }

    private async void OnCreateContact(object sender, EventArgs e)
    {
        var userName = UserName.Text;
        var contactID= ContactID.Text;
        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(contactID))
        {
            var contact = new ContactInfo(userName,contactID);
            if (await _facade.AddNewContactAsync(userName, contactID))
            {
                MainThread.BeginInvokeOnMainThread(() => _uiContacts.Add(contact));
                await Navigation.PopAsync();//сразщу возвращаемся на страницу контактов
            }
        }
    }
}