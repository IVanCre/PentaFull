namespace Client.Interfaces
{
    public interface IDialogManager
    {
        Task<bool> ShowConfirmDialog(string title, string message, string accept, string cancel);
        Task ShowMessage(string title, string message, string cancel);
    }
}
