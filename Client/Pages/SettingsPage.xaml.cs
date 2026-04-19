using Client.Interfaces;
using Penta_ClientLib.Interfaces;
using System.Reflection;
using Penta_ClientLib.DataStructs;


namespace Client.Pages;

public partial class SettingsPage : ContentPage
{
	private IClientFacade _clientFacade;
	private IUpdateManager _appUpdater;
    private IDialogManager _notifier;
    public string AppVersion { get; private set; } = "Unknown";
	public string ServerAvailable { get; private set; } = "Unknown";
	public string NewVersion { get; private set; } = "Unknown";



	public SettingsPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_clientFacade == null)
        {
            _clientFacade = App.Services.GetService<IClientFacade>();
            _appUpdater = App.Services.GetService<IUpdateManager>();

            AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            _clientFacade.ConnectionToServerChanged += ConnectChanged;

            _notifier = App.Services.GetRequiredService<IDialogManager>();

            ConnectChanged(_clientFacade.IsConnected());//сразу ставим текущее состояние
            BindingContext = this;
        }
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
        LoaderSpin.IsRunning = true;
        UpdateBtn.IsEnabled = false;

        var result = await _appUpdater.TryUpdateClientAsync(ClientType.Android);
		if(result!=null)//какие-то проблемы
            await _notifier.ShowMessage("Внимание", $"Ошибка при обновлении:{result.Message}", "ок");

        LoaderSpin.IsRunning = false;
        UpdateBtn.IsEnabled = true;
    }


    private void UseLoggerChanged(object sender, EventArgs e)
	{
		if(LoggerButon.IsChecked)
			_clientFacade.UseSysLogger(true);//включает отслеживание и вывод системных ошибок. Подписка идет на странице LoadPage
		else
            _clientFacade.UseSysLogger(false);
    }
}