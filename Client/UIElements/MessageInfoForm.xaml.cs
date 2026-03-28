using Client.Models;

namespace Client.UIElements;

public partial class MessageInfoForm : ContentView
{
	public MessageInfoForm()
	{
		InitializeComponent();
	}

    protected override void OnBindingContextChanged()//позволит часть логики снять с разметки
    {
        base.OnBindingContextChanged();

        if (BindingContext is MessageInfo message)
        {
            if (message.Type == Direction.Output)
            {
                this.HorizontalOptions = LayoutOptions.Start;//чтобы не колупаться с конвертерами и привязками
                SenderLabel.HorizontalTextAlignment = TextAlignment.Start;
                TextLabel.HorizontalTextAlignment = TextAlignment.Start;
                TimestampLabel.HorizontalTextAlignment = TextAlignment.Start;
            }
            else
            {
                this.HorizontalOptions = LayoutOptions.End;
                SenderLabel.HorizontalTextAlignment = TextAlignment.End;
                TextLabel.HorizontalTextAlignment = TextAlignment.End;
                TimestampLabel.HorizontalTextAlignment = TextAlignment.End;
            }
        }
    }
}