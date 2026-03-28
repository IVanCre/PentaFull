

namespace Client.Interfaces
{
    internal interface ISoundManager
    {
        /// <summary>
        /// Воспроизводит кастомный звук
        /// </summary>
        void PlaySound();

        /// <summary>
        /// Воспроизводит звук нового сообщения, установленный юзером
        /// </summary>
        void InputMessageNotify();
    }
}
