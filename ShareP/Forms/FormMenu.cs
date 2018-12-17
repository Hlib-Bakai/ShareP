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
using ShareP.Controllers;
using ShareP.Forms;
using System.Reflection;

namespace ShareP
{
    public partial class FormMenu : Form
    {
        private User m_user;
        private SearchController m_searchController;
        private bool firstTimeUser;

        public FormMenu()
        {
            Application.ApplicationExit += new EventHandler(OnProcessExit);
            InitializeComponent();
            Helper.CreateBorder(this);
            InitializeElements();
            ChangeStatusConnection();
            LoadConnectionTab();
            //CreateTestParams();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var exception = (Exception)e.ExceptionObject;
                Log.LogUnhandled(exception);
            };
            Log.LogInfo("Initial load successful");
        }

        private void InitializeElements()
        {
            Notification.notifyIcon = notifyIcon1;
            Notification.AddClosingEvent();

            firstTimeUser = IsFirstTimeUser();
            m_user = new User();
            Connection.CurrentUser = m_user;
            FillCurrentUser();
            labelUsername.Text = m_user.Username;

            Connection.FormMenu = this;
            Connection.ReservePresentation = false;

            labelIP.Text = m_user.IP;

            listBox1.DrawItem += new DrawItemEventHandler(listBox_DrawItem);
            listBoxChatUsers.DrawItem += new DrawItemEventHandler(listBox_DrawItem);


            richTextBoxMessages.BackColor = Color.White;
            richTextBoxMessages.GotFocus += TextBoxGotFocus;

            ChatController.SetTextBox(richTextBoxMessages, textBoxInputMessage);

            CleanTempFiles();

            //Controllers
            m_searchController = new SearchController();

        }
        
        private void FormMenu_Shown(object sender, EventArgs e)
        {
            if (firstTimeUser)
                ShowFirstTimeInstruction();
        }

        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        private void TextBoxGotFocus(object sender, EventArgs args)
        {
            HideCaret(richTextBoxMessages.Handle);
        }

        public async void StartPresentation(string name)  // Server side
        {
            this.Hide();
            FormLoading formLoading = new FormLoading("Presentation is preparing for sharing. Please, wait...");
            formLoading.Show();

            PresentationController.StartApp();

            await Task.Run(() => PresentationController.ExportImages(Helper.GetCurrentFolder()));

            formLoading.Close();

            Connection.CurrentPresentation = new Presentation()
            {
                Name = name,
                CurrentSlide = 1,
                Author = Connection.CurrentUser.Username
            };

            LoadConnectionTab();

            PresentationController.StartSlideShow(checkBoxCheater.Checked);
        }

        public void OnPresentationStart()   // Viewer side
        {
            if (this.WindowState != FormWindowState.Normal && (bool)Properties.Settings.Default["nPresentation"] &&
                                                              !(bool)Properties.Settings.Default["autojoin"])
                Notification.Show("Presentation", "Presentation " + Connection.CurrentPresentation.Name + " started", NotificationType.Presentation);
            if (InvokeRequired)
                Invoke(new Action(() => buttonJoin.Enabled = true));
            else
                buttonJoin.Enabled = true;
            if (InvokeRequired)
                Invoke(new Action(() => LoadPresentationTab()));
            else
                LoadPresentationTab();
            ViewerController.StartLoadingSlides();
            if ((bool)Properties.Settings.Default["autojoin"])
            {
                if (InvokeRequired)
                    Invoke(new Action(() => StartViewer()));
                else
                    StartViewer();
            }
        }

        public void OnPresentationFinished()  // Both sides
        {
            RestoreWindow();
            //LoadPresentationTab();
            // Suggest download?
        }

        public void FillCurrentUser()
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default["username"].ToString()))
            {
                Connection.CurrentUser.Username = Properties.Settings.Default["username"].ToString();
            }
            else
            {
                try
                {
                    ChangeUsername(System.Environment.MachineName);
                }
                catch (Exception e)
                {
                    Log.LogException(e, "Can't get computer's name. Using default one");
                }
                if (String.IsNullOrEmpty(Connection.CurrentUser.Username))
                    ChangeUsername("User #" + (new Random(Guid.NewGuid().GetHashCode())).Next(1, 10000).ToString());
            }

            if (Connection.CurrentUser.Username.Length > 10)
                ChangeUsername(Connection.CurrentUser.Username.Substring(0, 10));

            Connection.CurrentUser.IP = Helper.GetMyIP();
            Connection.CurrentUser.Id = Helper.GenerateUniqueId();
        }

        private bool IsFirstTimeUser()
        {
            return String.IsNullOrEmpty(Properties.Settings.Default["username"].ToString());
        }

        public void ShowFirstTimeInstruction()
        {
            Log.LogInfo("First time user. Asking for instruction");
            int overlay = Helper.ShowOverlay(this);
            FormAlert formAlert = new FormAlert("Welcome to ShareP", "It looks like your first time here. Want to read an instruction?",
                                                    false, "Yes", "No");
            if (formAlert.ShowDialog() == DialogResult.OK)
            {
                // Show instruction
            }
            Helper.HideOverlay(overlay);
        }

        public void FillHostUsersList()
        {
            if (Connection.CurrentGroup == null)
                return;

            if (listBox1.InvokeRequired)  //Accessing element from another thread
            {
                listBox1.Invoke(new Action(() => listBox1.Items.Clear()));
                foreach (User user in Connection.CurrentGroup.userList)
                {
                    listBox1.Invoke(new Action<string>((i) => listBox1.Items.Add(i)), user.Username);
                }
                listBox1.Invoke(new Action(() => listBox1.Show()));
            }
            else
            {
                listBox1.Items.Clear();
                foreach (User user in Connection.CurrentGroup.userList)
                {
                    listBox1.Items.Add(user.Username);
                }
                listBox1.Show();
            }
        }

        public void FillChatUsersList()
        {
            if (Connection.CurrentGroup == null)
                return;

            if (listBoxChatUsers.InvokeRequired)  //Accessing element from another thread
            {
                listBoxChatUsers.Invoke(new Action(() => listBoxChatUsers.Items.Clear()));
                foreach (User user in Connection.CurrentGroup.userList)
                {
                    listBoxChatUsers.Invoke(new Action<string>((i) => listBoxChatUsers.Items.Add(i)), user.Username);
                }
                listBoxChatUsers.Invoke(new Action(() => listBoxChatUsers.Refresh()));
            }
            else
            {
                listBoxChatUsers.Items.Clear();
                foreach (User user in Connection.CurrentGroup.userList)
                {
                    listBoxChatUsers.Items.Add(user.Username);
                }
                listBoxChatUsers.Refresh();
            }
        }


        private void buttonConnection_Click(object sender, EventArgs e)
        {
            LoadConnectionTab();
        }

        private void buttonPresentation_Click(object sender, EventArgs e)
        {
            LoadPresentationTab();
        }

        private void buttonMessages_Click(object sender, EventArgs e)
        {
            LoadMessagesTab();
        }

        private void Disconnect(bool force = false)
        {
            if (Connection.CurrentRole == Role.Host)
            {
                int overlay = Helper.ShowOverlay(this);
                FormAlert formAlert = new FormAlert("Confirmation", "Close the group?");
                if (force || formAlert.ShowDialog() == DialogResult.OK)
                {
                    Helper.HideOverlay(overlay);
                    ServerController.OnGroupClose();
                    PresentationController.OnAppClosing();
                    ViewerController.OnAppClosing();
                    Connection.Disconnect();
                    LoadConnectionTab();
                    buttonJoin.Enabled = true;
                }
                Helper.HideOverlay(overlay);
            }
            else if (Connection.CurrentRole == Role.Client)
            {
                int overlay = Helper.ShowOverlay(this);
                FormAlert formAlert = new FormAlert("Confirmation", "Disconnect from the group?");
                if (force || formAlert.ShowDialog() == DialogResult.OK)
                {
                    Helper.HideOverlay(overlay);
                    ViewerController.OnAppClosing();
                    PresentationController.OnAppClosing();
                    Connection.Disconnect();
                    LoadConnectionTab();
                    buttonJoin.Enabled = true;
                }
                Helper.HideOverlay(overlay);
            }
        }

        private void StartViewer()
        {
            buttonJoin.Enabled = false;
            this.Hide();
            ViewerController.LoadViewer();
            ViewerController.LoadSlide(Connection.CurrentPresentation.CurrentSlide);
        }

        public void OnViewerClosed()
        {
            buttonJoin.Enabled = true;
            RestoreWindow();
        }

        private void LoadConnectionTab()
        {
            menuPicker.Height = buttonConnection.Height;
            menuPicker.Top = buttonConnection.Top;

            if (Connection.CurrentRole != Role.Notconnected)
            {
                labelConStatus.Text = "Connected";
                labelConStatus.ForeColor = Color.Green;
                labelGroupName.Text = Connection.CurrentGroup.name;
                labelGroupHost.Text = Connection.CurrentGroup.hostName;
                buttonDisconnect.Show();
                if (Connection.CurrentRole == Role.Client)
                    tabsConnection.SelectTab("tabConnected");
                else if (Connection.CurrentRole == Role.Host)
                {
                    tabsConnection.SelectTab("tabConnectedHost");
                    FillHostUsersList();
                    if (Connection.CurrentGroup != null)
                    {
                        if (Connection.CurrentGroup.settings.Download)
                        {
                            labelSDownload.Text = "YES";
                            labelSDownload.ForeColor = Color.FromArgb(0, 192, 0);
                        }
                        else
                        {
                            labelSDownload.Text = "NO";
                            labelSDownload.ForeColor = Color.Red;
                        }

                        if (Connection.CurrentGroup.settings.Viewerspresent)
                        {
                            labelSPresent.Text = "YES";
                            labelSPresent.ForeColor = Color.FromArgb(0, 192, 0);
                        }
                        else
                        {
                            labelSPresent.Text = "NO";
                            labelSPresent.ForeColor = Color.Red;
                        }

                        if (Connection.CurrentGroup.navigation == GroupNavigation.Backwards)
                        {
                            labelSNavigation.Text = "Backwards";
                            labelSNavigation.ForeColor = Color.FromArgb(0, 192, 0);
                        }
                        else if (Connection.CurrentGroup.navigation == GroupNavigation.BothDirections)
                        {
                            labelSNavigation.Text = "YES";
                            labelSNavigation.ForeColor = Color.FromArgb(0, 192, 0);
                        }
                        else if (Connection.CurrentGroup.navigation == GroupNavigation.FollowOnly)
                        {
                            labelSNavigation.Text = "NO";
                            labelSNavigation.ForeColor = Color.Red;
                        }
                    }
                }
            }
            else
            {
                labelConStatus.Text = "Not connected";
                labelConStatus.ForeColor = Color.Red;
                labelGroupName.Text = "<Not connected>";
                labelGroupHost.Text = "<Not connected>";
                buttonDisconnect.Hide();
                tabsConnection.SelectTab("tabNotConnected");
            }

            tabsMenu.SelectTab("connectionTab");
        }

        private void LoadPresentationTab()
        {
            menuPicker.Height = buttonPresentation.Height;
            menuPicker.Top = buttonPresentation.Top;

            if (Connection.CurrentRole == Role.Notconnected)
            {
                tabsMenu.SelectTab("notConnectedTab");
                return;
            }

            if (Connection.CurrentPresentation != null)
            {
                labelCurrentName.Text = Connection.CurrentPresentation.Name;
                labelCurrentAuthor.Text = Connection.CurrentPresentation.Author;
                if (Connection.CurrentPresentation.Author.CompareTo(Connection.CurrentUser.Username) == 0)
                {
                    labelCurrentAuthor.ForeColor = Color.Red;
                    labelCurrentAuthor.Text += " [YOU]";
                }
                else
                {
                    labelCurrentAuthor.ForeColor = Color.FromArgb(0, 162, 232);
                }
                labelCurrentSlide.Text = Connection.CurrentPresentation.CurrentSlide.ToString() + "/" +
                                         Connection.CurrentPresentation.SlidesTotal.ToString();
                if (Connection.CurrentPresentation.Author.CompareTo(Connection.CurrentUser.Username) != 0)
                    buttonJoin.Visible = true;
                else
                    buttonJoin.Visible = false;

                if (ViewerController.IsWorking)
                {
                    if (InvokeRequired)
                        Invoke(new Action(() => buttonJoin.Enabled = false));
                    else
                        buttonJoin.Enabled = false;
                }
                else
                {
                    if (InvokeRequired)
                        Invoke(new Action(() => buttonJoin.Enabled = true));
                    else
                        buttonJoin.Enabled = true;
                }
            }
            else
            {
                labelCurrentName.Text = "<No presentation now>";
                labelCurrentAuthor.Text = "<No presentation now>";
                labelCurrentAuthor.ForeColor = Color.FromArgb(0, 162, 232);
                labelCurrentSlide.Text = "0/0";
                buttonJoin.Visible = false;
            }

            if (Connection.CurrentRole == Role.Client && !Connection.CurrentGroup.settings.Viewerspresent)
            {
                panelAllowed.Hide();

                if (Connection.CurrentPresentation == null)
                    panelNotAllowed.Show();
                else
                    panelNotAllowed.Hide();
            }
            else
            {
                panelNotAllowed.Hide();
                if (Connection.CurrentPresentation == null)
                    panelAllowed.Show();
                else
                    panelAllowed.Hide();
            }

            if (Connection.CurrentRole == Role.Host)
            {
                labelCheater.Visible = true;
                checkBoxCheater.Visible = true;
                buttonHelpCheater.Visible = true;
            }
            else
            {
                labelCheater.Visible = false;
                checkBoxCheater.Visible = false;
                buttonHelpCheater.Visible = false;
            }
            tabsMenu.SelectTab("presentationTab");
        }


        private void LoadMessagesTab()
        {
            menuPicker.Height = buttonMessages.Height;
            menuPicker.Top = buttonMessages.Top;

            if (Connection.CurrentRole == Role.Notconnected)
            {
                tabsMenu.SelectTab("notConnectedTab");
                return;
            }

            if (Connection.CurrentGroup != null && Connection.CurrentGroup.settings.Chat)
            {
                panelChatDisabled.Hide();
                textBoxInputMessage.Show();
                buttonSendMessage.Show();
            }
            else
            {
                panelChatDisabled.Show();
                panelChatDisabled.BringToFront();
                textBoxInputMessage.Hide();
                buttonSendMessage.Hide();
            }

            var newUsersList = Connection.clientConnection.RequestUsersList();
            if (newUsersList != null)
                Connection.CurrentGroup.userList = newUsersList;
            FillChatUsersList();
            tabsMenu.SelectTab("messagesTab");
        }


        private void ChangeStatusConnection(bool green = false)
        {
            if (green)
            {
                statusConnectionRed.Hide();
                statusConnectionGreen.Show();
            }
            else
            {
                statusConnectionRed.Show();
                statusConnectionGreen.Hide();
            }
        }

        private void CheckStatusConnection()
        {
            if (Connection.CurrentRole == Role.Notconnected)
            {
                ChangeStatusConnection();
            }
            else if (Connection.clientConnection.Faulted)
            {
                ChangeStatusConnection();
            }
            else
            {
                ChangeStatusConnection(true);
                string newIp = Helper.GetMyIP();
                if (newIp.CompareTo(labelIP.Text) != 0)
                {
                    Log.LogInfo("IP Changed from " + labelIP.Text + " to " + newIp);
                    timerConnection.Enabled = false;
                    RestoreWindow();
                    Disconnect(true);
                    int overlay = Helper.ShowOverlay();
                    FormAlert formAlert = new FormAlert("IP Changed", "Probably network was changed. You will be disconnected.", true);
                    formAlert.ShowDialog();
                    Helper.HideOverlay(overlay);
                    labelIP.Text = Helper.GetMyIP();
                    timerConnection.Enabled = true;
                }
            }
            labelIP.Text = Helper.GetMyIP();
        }

        public void ChangeUsername(string newUsername)
        {
            Connection.CurrentUser.Username = newUsername;
            Properties.Settings.Default["username"] = newUsername;
            Properties.Settings.Default.Save();
            Log.LogInfo(String.Format("Username changed to {0}.", newUsername));
        }


        private void CreateNewGroup(Group newGroup)
        {
            if (String.IsNullOrEmpty(newGroup.hostName))
            {
                newGroup.hostName = m_user.Username;
            }
            if (String.IsNullOrEmpty(newGroup.hostIp))
            {
                newGroup.hostIp = Helper.GetMyIP();
            }
            if (Connection.CreateGroup(newGroup))
            {
                Connection.CurrentGroup.AddUser(m_user);
            }
            else
            {
                int overlay = Helper.ShowOverlay(this);
                FormAlert formAlert = new FormAlert("Error", "Server could not be started. Make sure to run application as an administator", true);
                formAlert.ShowDialog();
                Helper.HideOverlay(overlay);
            }
            LoadConnectionTab();
            CheckStatusConnection();
        }

        public void RestoreWindow()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => this.Show()));
                this.Invoke(new Action(() => this.WindowState = FormWindowState.Normal));
                this.Invoke(new Action(() => LoadConnectionTab()));
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.LoadConnectionTab();
            }
        }

        public void StartPresentationReservedTimer()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() => timerPresentation.Start()));
            else
                timerPresentation.Start();
        }

        public void StopPresentationReservedTimer()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(() => timerPresentation.Stop()));
            else
                timerPresentation.Stop();
        }

        private void BanUser()
        {
            if (listBox1.SelectedItem != null)
            {
                if (listBox1.SelectedItem.ToString().CompareTo(Connection.CurrentUser.Username) == 0)
                {
                    int ov1 = Helper.ShowOverlay(this);
                    FormAlert formAlert1 = new FormAlert("Error", "You cannot ban yourself", true);
                    formAlert1.ShowDialog();
                    Helper.HideOverlay(ov1);
                    return;
                }
                int ov2 = Helper.ShowOverlay(this);
                FormAlert formAlert = new FormAlert("Confirm action", "Ban " + listBox1.SelectedItem.ToString() + "?");
                if (formAlert.ShowDialog() == DialogResult.OK)
                {
                    ServerController.BanUser(listBox1.SelectedItem.ToString());
                }
                Helper.HideOverlay(ov2);
            }
            else
            {
                int ov3 = Helper.ShowOverlay(this);
                FormAlert formAlert1 = new FormAlert("Error", "No user is selected to ban", true);
                formAlert1.ShowDialog();
                Helper.HideOverlay(ov3);
                return;
            }
        }

        #region System

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ListBox list = (ListBox)sender;
            if (e.Index > -1)
            {
                object item = list.Items[e.Index];
                e.DrawBackground();
                e.DrawFocusRectangle();
                Brush brush = new SolidBrush(e.ForeColor);
                SizeF size = e.Graphics.MeasureString(item.ToString(), e.Font);
                e.Graphics.DrawString(item.ToString(), e.Font, brush, e.Bounds.Left + (e.Bounds.Width / 2 - size.Width / 2), e.Bounds.Top + (e.Bounds.Height / 2 - size.Height / 2));
            }
        }

        #endregion


        #region Move window
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    ReleaseCapture();
            //    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            //}
        }

        private void panelTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }





        #endregion


        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void timerConnection_Tick(object sender, EventArgs e)
        {
            CheckStatusConnection();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            LoadConnectionTab();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            int overlay = Helper.ShowOverlay(this);
            FormSettings formSettings = new FormSettings();
            if (formSettings.ShowDialog() == DialogResult.OK)
            {
                labelUsername.Text = Connection.CurrentUser.Username;
            }
            Helper.HideOverlay(overlay);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FormSearchServers formSearchServers = new FormSearchServers(m_searchController);
            int overlay = Helper.ShowOverlay(this);
            if (formSearchServers.ShowDialog() == DialogResult.OK)
            {
                LoadConnectionTab();
            }
            Helper.HideOverlay(overlay);
        }



        private void button3_Click(object sender, EventArgs e)
        {
            int overlay = Helper.ShowOverlay(this);
            FormCreateGroup formCreateGroup = new FormCreateGroup();
            if (formCreateGroup.ShowDialog(this) == DialogResult.OK)
            {
                Helper.HideOverlay(overlay);
                CreateNewGroup(formCreateGroup.NewGroup);
            }
            Helper.HideOverlay(overlay);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Connection.Disconnect();
            LoadConnectionTab();
            CheckStatusConnection();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            textBoxFile.Text = openFileDialog.FileName;
            if (String.IsNullOrEmpty(textBoxPresentationName.Text))
                textBoxPresentationName.Text = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
        }

        private void textBoxFile_DoubleClick(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private bool CheckLengthPresentationName()
        {
            labelLength.Hide();
            int length = textBoxPresentationName.Text.Length;
            if (length < 2 || length > 20)
            {
                labelLength.Show();
                return false;
            }
            return true;
        }


        private async void button5_Click_1(object sender, EventArgs e)
        {
            if (!CheckLengthPresentationName())
                return;
            if (!PresentationController.CheckApp())
            {
                int ov = Helper.ShowOverlay(this);
                FormAlert formAlert = new FormAlert("PowerPoint error", "Make sure You have installed PowerPoint 2007 or newer", true);
                formAlert.ShowDialog();
                Helper.HideOverlay(ov);
                return;
            }

            if ((Connection.CurrentRole == Role.Client && !Connection.clientConnection.ClRequestPresentationStart()) ||
                (Connection.CurrentRole == Role.Host && Connection.ReservePresentation))
            {
                int ov = Helper.ShowOverlay(this);
                FormAlert formAlert = new FormAlert("Error", "Someone in the group has already started presentation", true);
                formAlert.ShowDialog();
                Helper.HideOverlay(ov);
                return;
            }

            string file = textBoxFile.Text;
            string name = textBoxPresentationName.Text;
            int overlay = Helper.ShowOverlay();
            if (file.Length < 1)
                (new FormAlert("No file", "Please, choose presentation file.", true)).ShowDialog();
            else
            {
                var formLoading = new FormLoading("Loading presentation. Please wait...");
                try
                {
                    if (Connection.CurrentRole == Role.Host)
                        Connection.ReservePresentation = true;

                    formLoading.Show();

                    await Task.Run(() => PresentationController.LoadPPT(file));

                    formLoading.Close();
                    StartPresentation(name);
                }
                catch (Exception ex)
                {
                    if (formLoading != null)
                        formLoading.Close();
                    Log.LogException(ex, "Can't load presentation");
                    (new FormAlert("Error", "Problem occured while opening the file", true)).ShowDialog();
                    if (Connection.CurrentRole == Role.Host)
                        Connection.ReservePresentation = false;
                }
            }
            Helper.HideOverlay(overlay);
        }

        private void timerUsers_Tick(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FormMenu_Move(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            RestoreWindow();
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            RestoreWindow();
            switch (Notification.type)
            {
                case NotificationType.Chat:
                    LoadMessagesTab();
                    break;
                case NotificationType.Connection:
                    LoadConnectionTab();
                    break;
                case NotificationType.Presentation:
                    LoadPresentationTab();
                    break;
            }
        }

        private void button7_Click_1(object sender, EventArgs e) // Delete
        {
            Connection.CurrentPresentation = new Presentation()
            {
                CurrentSlide = 1,
                SlidesTotal = 10
            };
            ViewerController.LoadViewer();
        }

        private void buttonJoin_Click(object sender, EventArgs e)
        {
            StartViewer();
        }

        private void button8_Click(object sender, EventArgs e) // Delete
        {
            bool res = ViewerController.IsWorking;
        }

        private void FormMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            //OnAppClosing();
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            OnAppClosing();
        }

        private void OnAppClosing()
        {
            if (Connection.CurrentRole == Role.Host)
            {
                ServerController.OnGroupClose();
                PresentationController.OnAppClosing();
            }
            else if (Connection.CurrentRole == Role.Client)
            {
                ViewerController.OnAppClosing();
                Connection.Disconnect();
            }
            CleanTempFiles();
        }

        private void CleanTempFiles()
        {
            PresentationController.CleanTempFiles();
            ViewerController.CleanTempFiles();
        }

        private void labelUsername_MouseHover(object sender, EventArgs e)
        {
            pictureBoxEditUsername.Show();
        }

        private void labelUsername_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxEditUsername.Hide();
        }

        private void labelUsername_Click(object sender, EventArgs e)
        {
            FormChangeUsername formChangeUsername = new FormChangeUsername();

            int overlay = Helper.ShowOverlay(this);

            if (Connection.CurrentRole != Role.Notconnected)
            {
                FormAlert formAlert = new FormAlert("Error", "You are not allowed to change username when connected.", true);
                formAlert.ShowDialog();
            }
            else
                formChangeUsername.ShowDialog();

            Helper.HideOverlay(overlay);
            labelUsername.Text = m_user.Username;
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            FormDownloadSlide formProgress = new FormDownloadSlide();
            formProgress.ShowDialog();
        }

        private void buttonChangeGroupS_Click(object sender, EventArgs e)
        {
            int overlay = Helper.ShowOverlay(this);

            FormChangeGroupSettings formChangeGroupSettings = new FormChangeGroupSettings();
            if (formChangeGroupSettings.ShowDialog() == DialogResult.OK)
            {
                ServerController.OnGroupSettingsChanged();
                LoadConnectionTab();
            }
            Helper.HideOverlay(overlay);
        }

        private void button7_Click_2(object sender, EventArgs e)
        {
            FormCheater formCheater = new FormCheater();
            formCheater.Show();
            formCheater.MarkNotCheater("test5");
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            textBoxFile.Text = "";
            textBoxPresentationName.Text = "";
            checkBoxCheater.Checked = false;
        }

        private void buttonHelpCheater_Click(object sender, EventArgs e)
        {
            int ov = Helper.ShowOverlay(this);
            FormHelpCheater formHelpCheater = new FormHelpCheater();
            formHelpCheater.ShowDialog();
            Helper.HideOverlay(ov);
        }

        private void buttonSendMessage_Click(object sender, EventArgs e)
        {
            if (textBoxInputMessage.Text.Length > 0)
            {
                ChatController.SendMessage();
            }
        }

        private void textBoxInputMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBoxInputMessage.Text.Length > 0)
                {
                    ChatController.SendMessage();
                }
                e.SuppressKeyPress = true;
            }
        }

        private void labelIP_Click(object sender, EventArgs e)
        {
            int overlay = Helper.ShowOverlay(this);
            FormChangeIp formChangeIp = new FormChangeIp();
            if (Connection.CurrentRole != Role.Notconnected)
            {
                FormAlert formAlert = new FormAlert("Error", "You are not allowed to change network when connected.", true);
                formAlert.ShowDialog();
            }
            else
                if (formChangeIp.ShowDialog() == DialogResult.OK)
                CheckStatusConnection();
            Helper.HideOverlay(overlay);
        }

        private void labelIP_MouseHover(object sender, EventArgs e)
        {
            pictureBoxEditIp.Show();
        }

        private void labelIP_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxEditIp.Hide();
        }

        private void timerPresentation_Tick(object sender, EventArgs e)
        {
            Connection.ReservePresentation = false;
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            BanUser();
        }

    }
}