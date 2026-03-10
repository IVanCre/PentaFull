

using Penta_ClientLib.Interfaces;


namespace Client.Pages
{

	public partial class MembersManager : ContentView
	{
        private IClientFacade _clientFacade;
        private int currentChatID = -1;

        public MembersManager()
        {
            InitializeComponent();
            _clientFacade = App.Services.GetRequiredService<IClientFacade>();
        }

        public void Show(int chatID)
        {
            currentChatID = chatID;
            IsVisible = true;
        }

		private async void OnAddMember(object sender, EventArgs e)
		{
			await _clientFacade.AddUserToGroupChatAsync(currentChatID, ContactIDEntry.Text);
            ContactIDEntry.Text = "";
            IsVisible = false;
        }
		private async void OnDeleteMember(object sender, EventArgs e)
		{
			await _clientFacade.DeleteUserFromGroupChatAsync(currentChatID, ContactIDEntry.Text);
            ContactIDEntry.Text = "";
            IsVisible = false;
        }
	}
}