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

        public static void Show(string title, string text, ToolTipIcon toolTipIcon = ToolTipIcon.Info)
        {
            notifyIcon.ShowBalloonTip(1000, title, text, toolTipIcon);
        }

        public static void HideAll()
        {
            notifyIcon.Visible = false;
            notifyIcon.Visible = true;
        }
    }
}
