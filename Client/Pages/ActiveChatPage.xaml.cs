
using Client.Models;
using System.Collections.ObjectModel;

namespace Client.Pages
{

    public partial class ActiveChatPage : ContentPage
	{
		public ObservableCollection<MessageInfo> MessageList { get; set; }
		public string Name { get; private set; }
		private int _id;


		public ActiveChatPage(string name, int chatID)
		{
            InitializeComponent();

            MessageList = new();
            Name = name;
			_id = chatID;

			LoadLastMessages();
            BindingContext = this;
		}		
		private void LoadLastMessages()
		{
			//подгружаем последние Х сообщений
        }


        private void OnSendMessage(object sender, EventArgs e)
		{
			var input = MessageText.Text;
			MessageList.Add(new MessageInfo() { Text = input });

			MessageText.Text = "";
        }
    }
}