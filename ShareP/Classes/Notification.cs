using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareP
{
    static class Notification
    {
        public static NotifyIcon notifyIcon;
        public static NotificationType type;

        public static void AddClosingEvent()
        {
            notifyIcon.BalloonTipClosed += (sender, e) => 
            {
                var thisIcon = (NotifyIcon)sender;
                thisIcon.Visible = false;
                thisIcon.Dispose();
            };
        }

        public static void Show(string title, string text, NotificationType ntype, ToolTipIcon toolTipIcon = ToolTipIcon.Info)
        {
            notifyIcon.ShowBalloonTip(1000, title, text, toolTipIcon);
            type = ntype;
        }

        public static void HideAll()
        {
            notifyIcon.Visible = false;
            notifyIcon.Visible = true;
        }
    }
}
