using Microsoft.Toolkit.Uwp.Notifications;

namespace FitLog.Notifications
{
    public static class MyNotify
    {
        public static void ShowNotification(string title, string message)
        {
            var notify = new ToastContentBuilder();
            notify.AddText(title);
            notify.AddText(message);
            notify.Show();
        }
    }
}
