
using Penta_ClientLib.Interfaces;

namespace Client.Pages;

public partial class LoadingPage : ContentPage
{
	public LoadingPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing(); 
        await TryAutoLogin();
    }

    //попытаемся сразу перейти на нужную страницу и параллельно подключиться к серваку
    private async Task TryAutoLogin()
    {
        var _clientFacade = App.Services.GetRequiredService<IClientFacade>();
        var myContact = await _clientFacade.GetMyContactID();
        if (!string.IsNullOrEmpty(myContact))
        {
            _ = _clientFacade.ConnectToServerAsync();
            await Shell.Current.GoToAsync("//ChatsPage");
        }
        else
            await Shell.Current.GoToAsync("//AuthPage");
    }
}