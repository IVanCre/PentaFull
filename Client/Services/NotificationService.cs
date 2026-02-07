
namespace Client.Services
{
    public static class NotificationService
    {
        public static async Task<bool> ShowConfirmDialog(string title, string message, string accept, string cancel)
        {
            var currentPage = Application.Current.MainPage;
            if (currentPage == null)
                return false;

            // Если MainPage — NavigationPage, берём текущую дочернюю страницу
            if (currentPage is NavigationPage navPage)
                currentPage = navPage.CurrentPage;

            return await currentPage.DisplayAlert(title, message, accept, cancel);
        }

        public static async Task ShowMessage(string title, string message, string cancel)
        {
            var currentPage = Application.Current.MainPage;
            if (currentPage is NavigationPage navPage)
                currentPage = navPage.CurrentPage;

            if (currentPage != null)
                await currentPage.DisplayAlert(title, message, cancel);
        }
    }
}
