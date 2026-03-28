using Client.Interfaces;
using AndroidApp=Android.App;
using Android.Media;

namespace Client.Platforms.Android
{
    internal class AndroidSoundManager : ISoundManager
    {
        public void InputMessageNotify()
        {
            var uri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);

            var ringtone = RingtoneManager.GetRingtone(AndroidApp.Application.Context, uri);
            ringtone?.Play();
        }

        public void PlaySound()
        {
            throw new NotImplementedException();
        }
    }
}
