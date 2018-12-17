using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace ShareP
{
    public static class ChatController
    {
        static RichTextBox richTextBox;
        static TextBox inputTextBox;

        static List<Message> messageHistory = new List<Message>();

        public static void RecieveMessage(Message msg, bool self = false, bool loadingHistory = false)
        {
            if (msg.SenderIp.CompareTo(Connection.CurrentUser.IP) == 0 && !self && !loadingHistory)
                return;
            
            messageHistory.Add(msg);

            if (richTextBox.InvokeRequired)
                richTextBox.Invoke(new Action<Message, bool>((m, s) => RecieveMessageActions(m, s)), msg, self);
            else
                RecieveMessageActions(msg, self);

            if ((bool)Properties.Settings.Default["nPresentation"] && !richTextBox.Visible && !loadingHistory)
            {
                Notification.Show("Chat", "New message from " + msg.Sender, NotificationType.Chat);
            }
        }

        public static List<Message> GetMessageHistory()
        {
            return messageHistory;
        }

        private static void RecieveMessageActions(Message msg, bool self)
        {
            int length = richTextBox.Text.Length;
            string timeLine = String.Format("{0}", msg.Time.ToShortTimeString()) + Environment.NewLine;
            string messageLine = msg.Sender + ": " + msg.Text + Environment.NewLine;

            richTextBox.AppendText(timeLine);
            richTextBox.Select(length, timeLine.Length);
            richTextBox.SelectionFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Italic);
            richTextBox.SelectionColor = Color.FromArgb(0, 162, 232);

            length = richTextBox.Text.Length;
            richTextBox.AppendText(messageLine);
            richTextBox.Select(length, msg.Sender.Length);
            richTextBox.SelectionFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
            if (self)
                richTextBox.SelectionColor = Color.Red;

            richTextBox.Select(richTextBox.Text.Length, 0);
            richTextBox.ScrollToCaret();
        }

        public static void SendMessage()
        {
            Message newMessage = new Message();
            newMessage.Text = inputTextBox.Text;
            newMessage.Sender = Connection.CurrentUser.Username;
            newMessage.SenderIp = Connection.CurrentUser.IP;
            newMessage.Time = DateTime.Now;

            Connection.SendMessage(newMessage);

            RecieveMessage(newMessage, true);
            inputTextBox.Clear();
        }

        public static void CleanChat()
        {
            if (richTextBox != null)
                richTextBox.Clear();
            if (messageHistory != null)
                messageHistory.Clear();
        }

        public static void SetTextBox(RichTextBox rtb, TextBox input)
        {
            richTextBox = rtb;
            inputTextBox = input;
        }
    }
}
