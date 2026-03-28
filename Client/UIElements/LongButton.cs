
namespace Client.UIElements
{
    internal sealed class LongButton:Button
    {
        private int _pressSeconds = 2;
        private DateTime _startPress;

        public event EventHandler LongPressClicked;
        public event EventHandler ShortPressClicked;

        public LongButton()
        {
            Pressed += StartClick;
            Released += EndClick;
        }

        private void StartClick(object sender, EventArgs e)
        {
            _startPress = DateTime.Now;
        }
        private void EndClick(object sender, EventArgs e)
        {
            if ((DateTime.Now - _startPress).TotalSeconds >= _pressSeconds)
                LongPressClicked?.Invoke(sender, e);
            else
                ShortPressClicked?.Invoke(sender, e);

        }
    }
}
