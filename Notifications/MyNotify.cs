using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
