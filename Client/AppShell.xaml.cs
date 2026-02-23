

namespace Client
{
    public partial class AppShell : Shell
    {
        private bool _loadingPageDisabled = false;
        public AppShell()
        {
            InitializeComponent();
            Navigated += OnShellNavigated;
        }

        private void OnShellNavigated(object sender, ShellNavigatedEventArgs e)
        {
            if (!_loadingPageDisabled) 
            {
                if (e.Previous != null)
                {
                    string prevPageName = e.Previous?.Location.OriginalString.Split('/').LastOrDefault() ?? "None";
                    if (prevPageName == "LoadingPage")//эта страница-заглушка для автовхода
                    {
                        var finded = Current.Items
                            .SelectMany(f => f.Items)
                            .SelectMany(t => t.Items)
                            .FirstOrDefault(sc => sc.Route == prevPageName);
                        if (finded != null )
                        {
                            finded.IsVisible = false;
                            finded.IsEnabled = false;
                        }
                    }
                }
            }
        }
    }
}
