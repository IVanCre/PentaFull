using Client.Interfaces;
using Penta_ClientLib.Interfaces;
using System.Reflection;
using Penta_ClientLib.DataStructs;

namespace Client.Pages;

public partial class SettingsPage : ContentPage
{
	private IClientFacade _facade;
	private IUpdateManager _updater;
	public string AppVersion { get; private set; } = "Unknown";
	public string ServerAvailable { get; private set; } = "Unknown";
	public string NewVersion { get; private set; } = "Unknown";



	public SettingsPage()
	{
		InitializeComponent();

		_facade = App.Services.GetService<IClientFacade>();
		_updater = App.Services.GetService<IUpdateManager>();
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		_facade.ConnectionToServerChanged += ConnectChanged;

		BindingContext = this;
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
		await _updater.TryUpdateClientAsync(ClientType.Android);
	}
}