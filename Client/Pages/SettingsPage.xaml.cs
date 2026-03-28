using Client.Interfaces;
using Penta_ClientLib.Interfaces;
using System.Reflection;
using Penta_ClientLib.DataStructs;


namespace Client.Pages;

public partial class SettingsPage : ContentPage
{
	private IClientFacade _clientFacade;
	private IUpdateManager _appUpdater;
	public string AppVersion { get; private set; } = "Unknown";
	public string ServerAvailable { get; private set; } = "Unknown";
	public string NewVersion { get; private set; } = "Unknown";



	public SettingsPage()
	{
		InitializeComponent();

		_clientFacade = App.Services.GetService<IClientFacade>();
		_appUpdater = App.Services.GetService<IUpdateManager>();

        AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		_clientFacade.ConnectionToServerChanged += ConnectChanged;

		ConnectChanged(_clientFacade.IsConnected());//сразу ставим текущее состо€ние
		BindingContext = this;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
        _clientFacade.ConnectionToServerChanged -= ConnectChanged;
    }


    private void ConnectChanged(bool state)
	{
		if (state)
			ServerAvailable = "true";
		else
			ServerAvailable = "false";

		OnPropertyChanged(nameof(ServerAvailable));
	}
	private async void DownloadClick(object sender, EventArgs e)
	{
        await _appUpdater.TryUpdateClientAsync(ClientType.Android);
	}


	private void UseLoggerChanged(object sender, EventArgs e)
	{
		if(LoggerButon.IsChecked)
			_clientFacade.UseSysLogger(true);//включает отслеживание и вывод системных ошибок. ѕодписка идет на странице LoadPage
		else
            _clientFacade.UseSysLogger(false);
    }
}