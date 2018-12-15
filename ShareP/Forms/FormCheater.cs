using ShareP.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareP
{
    public partial class FormCheater : Form
    {
        Dictionary<string, Cheater> cheaters;
        bool white;

        public FormCheater()
        {
            InitializeComponent();
            Helper.CreateBorder(this);
            cheaters = new Dictionary<string, Cheater>();
            UpdateListOfUsers();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            white = false;
        }

        public void UpdateListOfUsers()
        {
            cheaters.Clear();
            if (Connection.CurrentGroup != null)
            {
                foreach(var user in Connection.CurrentGroup.userList)
                {
                    Cheater cheater = new Cheater();
                    cheater.user = user;
                    cheater.ifCheater = false;          //Change to TRUE for test
                    cheater.focusReturned = false;
                    cheater.timeLeft = -1;
                    cheater.disconnected = false;
                    cheaters.Add(user.Username, cheater);
                }
                RedrawList();
            }
        }

        private void RedrawList()
        {
            int totalCount = 0;
            int row = 57;
            int column = 12;
            List<Panel> toRemove = new List<Panel>();
            foreach(var panel in this.Controls)
            {
                if (panel is Panel)
                {
                    if ((((Panel)panel).Tag != null && ((Panel)panel).Tag.ToString().CompareTo("nodelete") != 0) 
                      || ((Panel)panel).Tag == null)
                    {
                        toRemove.Add((Panel)panel);
                    }
                }
            }

            foreach(Panel panel in toRemove)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<Panel>((p) => this.Controls.Remove(p)), panel);
                    this.Invoke(new Action<Panel>((p) => p.Dispose()), panel);
                }
                else
                {
                    this.Controls.Remove(panel);
                    panel.Dispose();
                }
            }

            foreach(var cheater in cheaters)
            {
                if (cheater.Value.ifCheater)
                {
                    Panel panel = new Panel();
                    panel.Location = new Point(column, row);
                    panel.Size = new Size(82, 90);
                    panel.Show();
                    cheater.Value.panel = panel;
                    

                    Label labelCheater = new Label();
                    if (!cheater.Value.focusReturned)
                        labelCheater.Text = "CHEATER";
                    if (cheater.Value.disconnected)
                    {
                        labelCheater.Text = "DISCONNECTED";
                        labelCheater.Font = new Font(FontFamily.GenericSansSerif, 6, FontStyle.Regular);
                    }
                    else
                        labelCheater.Font = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold);
                    labelCheater.ForeColor = Color.Red;
                    labelCheater.Location = new Point(6, 2);
                    labelCheater.Show();
                    cheater.Value.cheatLabel = labelCheater;
                    panel.Controls.Add(labelCheater);

                    PictureBox pictureBox = new PictureBox();
                    pictureBox.Location = new Point(20, 20);
                    pictureBox.Size = new Size(43, 46);
                    pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                    if (cheater.Value.focusReturned)
                        pictureBox.Image = (Image)Resources.ResourceManager.GetObject("User_Green");
                    else
                        pictureBox.Image = (Image)Resources.ResourceManager.GetObject("User_Red");
                    pictureBox.Show();
                    cheater.Value.image = pictureBox;
                    

                    Label labelName = new Label();
                    labelName.Text = cheater.Value.user.Username;
                    labelName.Font = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold);
                    labelName.Location = new Point(3, 69);
                    labelName.AutoSize = false;
                    labelName.Size = new Size(73, 15);
                    labelName.TextAlign = ContentAlignment.MiddleCenter;
                    labelName.Show();
                    cheater.Value.name = labelName;

                    if (this.InvokeRequired)
                        this.Invoke(new Action<Panel>((p) => this.Controls.Add(p)), panel);
                    else
                        this.Controls.Add(panel);
                    if (panel.InvokeRequired)
                        panel.Invoke(new Action<PictureBox>((p) => panel.Controls.Add(p)), pictureBox);
                    else
                        panel.Controls.Add(pictureBox);
                    if (panel.InvokeRequired)
                        panel.Invoke(new Action<Label>((l) => panel.Controls.Add(l)), labelName);
                    else
                        panel.Controls.Add(labelName);

                    if (panel.InvokeRequired)
                        panel.Invoke(new Action(() => panel.BringToFront()));
                    else
                        panel.Controls.Add(labelName);

                    if (this.InvokeRequired)
                        this.Invoke(new Action(() => this.Refresh()));
                    else
                        this.Refresh();

                    totalCount++;

                    if (totalCount % 4 == 0)
                    {
                        column = 12;
                        row += 96;
                    } else
                    {
                        column += 88;
                    }

                    if (totalCount >= 12)
                    {
                        break;
                    }
                }
            }
        }
        

        public void MarkCheater(string username)
        {
            if (!cheaters.ContainsKey(username))
                return;

            var cheater = cheaters[username];
            cheater.focusReturned = false;
            cheater.ifCheater = true;
            cheater.timeLeft = -1;

            RedrawList();
        }

        public void MarkNotCheater(string username)
        {
            if (!cheaters.ContainsKey(username))
                return;

            var cheater = cheaters[username];
            cheater.focusReturned = true;
            cheater.ifCheater = true;
            cheater.disconnected = false;
            cheater.timeLeft = 10000;

            RedrawList();
        }

        public void MarkDisconnected(string username)
        {
            if (!cheaters.ContainsKey(username))
                return;

            var cheater = cheaters[username];
            cheater.focusReturned = false;
            cheater.ifCheater = true;
            cheater.disconnected = true;
            cheater.timeLeft = -1;

            RedrawList();
        }

        private void FormCheater_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach(var cheater in cheaters)
            {
                if (cheater.Value.ifCheater && cheater.Value.focusReturned)
                {
                    cheater.Value.timeLeft -= 100;
                }
                if (cheater.Value.timeLeft == 0)
                {
                    cheater.Value.timeLeft = -1;
                    cheater.Value.ifCheater = false;
                    cheater.Value.focusReturned = false;

                    RedrawList();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void FormCheater_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (white)
            {
                this.BackColor = Color.Black;
                label1.ForeColor = Color.White;
                button2.Text = "White theme";
            }
            else
            {
                this.BackColor = Color.White;
                label1.ForeColor = Color.Black;
                button2.Text = "Black theme";
            }
            white = !white;
        }
    }

    class Cheater
    {
        public User user;
        public Panel panel;
        public Label name;
        public Label cheatLabel;
        public PictureBox image;
        public int timeLeft;
        public bool ifCheater;
        public bool focusReturned;
        public bool disconnected;
    }
}
