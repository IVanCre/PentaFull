
using Penta_ClientLib.Interfaces;
using System.Collections.ObjectModel;


namespace Client.Pages;

public partial class ContactCreatorPage : ContentPage
{
    private IContactHolder _contactHolder;
    private Collection<ContactInfo> _uiContacts;

    public ContactCreatorPage(
        Collection<ContactInfo> uiContacts,
        IContactHolder contactHolder)
	{
		InitializeComponent();
        _contactHolder = contactHolder;
        _uiContacts= uiContacts;
    }

    private async void OnCreateContact(object sender, EventArgs e)
    {
        var userName = UserName.Text;
        var contactID= ContactID.Text;
        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(contactID))
        {
            var contact = new ContactInfo(userName,contactID);
            if (await _contactHolder.AddContact(userName, contactID))
            {
                _uiContacts.Add(contact);
                await Navigation.PopAsync();//сразщу возвращаемся на страницу контактов
            }
        }
    }
}