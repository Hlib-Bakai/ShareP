﻿using System;
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

namespace ShareP
{
    public partial class FormMenu : Form
    {
        private User m_user;
        private SearchController m_searchController;

        public FormMenu()
        {
            InitializeComponent();
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

            m_user = new User();
            Connection.CurrentUser = m_user;
            FillCurrentUser();
            labelUsername.Text = m_user.Username;

            Connection.FormMenu = this;

            labelIP.Text = m_user.IP;

            listBox1.DrawItem += new DrawItemEventHandler(listBox_DrawItem);

            CleanTempFiles();

            //Controllers
            m_searchController = new SearchController();
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

            PresentationController.StartSlideShow();
        }

        public void OnPresentationStart()   // Client side
        {
            if (this.WindowState != FormWindowState.Normal) ;
                Notification.Show("Presentation", "Presentation " + Connection.CurrentPresentation.Name + " started");
            LoadPresentationTab();
            ViewerController.StartLoadingSlides();
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

            Connection.CurrentUser.IP = Helper.GetMyIP();
        }


        public void ChangeUsername(string newUsername)
        {
            Connection.CurrentUser.Username = newUsername;
            Properties.Settings.Default["username"] = newUsername;
            Properties.Settings.Default.Save();
            Log.LogInfo(String.Format("Username changed to {0}.", newUsername));
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
                listBox1.Invoke(new Action(() => listBox1.Refresh()));
            }
            else
            {
                listBox1.Items.Clear();
                foreach (User user in Connection.CurrentGroup.userList)
                {
                    listBox1.Items.Add(user.Username);
                }
                listBox1.Refresh();
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
            if (Connection.CurrentRole == Connection.Role.Host)
            {
                FormAlert formAlert = new FormAlert("Confirmation", "Close the group?");
                if (force || formAlert.ShowDialog() == DialogResult.OK)
                {
                    ServerController.OnGroupClose();
                    PresentationController.OnAppClosing();
                    Connection.Disconnect();
                    LoadConnectionTab();
                }
            }
            else if (Connection.CurrentRole == Connection.Role.Client)
            {
                FormAlert formAlert = new FormAlert("Confirmation", "Disconnect from the group?");
                if (force || formAlert.ShowDialog() == DialogResult.OK)
                {
                    ViewerController.OnAppClosing();
                    Connection.Disconnect();
                    LoadConnectionTab();
                }
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

            if (Connection.CurrentRole != Connection.Role.Notconnected)
            {
                labelConStatus.Text = "Connected";
                labelConStatus.ForeColor = Color.Green;
                labelGroupName.Text = Connection.CurrentGroup.name;
                labelGroupHost.Text = Connection.CurrentGroup.hostName;
                buttonDisconnect.Show();
                if (Connection.CurrentRole == Connection.Role.Client)
                    tabsConnection.SelectTab("tabConnected");
                else if (Connection.CurrentRole == Connection.Role.Host)
                {
                    tabsConnection.SelectTab("tabConnectedHost");
                    FillHostUsersList();
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

            if (Connection.CurrentRole == Connection.Role.Notconnected)
            {
                tabsMenu.SelectTab("notConnectedTab");
                return;
            }

            if (Connection.CurrentPresentation != null)
            {
                labelCurrentName.Text = Connection.CurrentPresentation.Name;
                if (Connection.CurrentRole == Connection.Role.Host)
                {
                    labelCurrentAuthor.ForeColor = Color.Red;
                    labelCurrentAuthor.Text += "YOU";
                }
                else
                {
                    labelCurrentAuthor.ForeColor = Color.FromArgb(0, 162, 232);
                    labelCurrentAuthor.Text = Connection.CurrentPresentation.Author;
                }
                labelCurrentSlide.Text = Connection.CurrentPresentation.CurrentSlide.ToString() + "/" +
                                         Connection.CurrentPresentation.SlidesTotal.ToString();
                if (Connection.CurrentRole != Connection.Role.Host)  // TODO Check if not already joined
                    buttonJoin.Visible = true;
                else
                    buttonJoin.Visible = false;
            }
            else
            {
                labelCurrentName.Text = "<No presentation now>";
                labelCurrentAuthor.Text = "<No presentation now>";
                labelCurrentAuthor.ForeColor = Color.FromArgb(0, 162, 232);
                labelCurrentSlide.Text = "0/0";
                buttonJoin.Visible = false;
            }

            if (Connection.CurrentRole == Connection.Role.Client && !Connection.CurrentGroup.settings.Viewerspresent)
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
            tabsMenu.SelectTab("presentationTab");
        }


        private void LoadMessagesTab()
        {
            menuPicker.Height = buttonMessages.Height;
            menuPicker.Top = buttonMessages.Top;

            if (Connection.CurrentRole == Connection.Role.Notconnected)
            {
                tabsMenu.SelectTab("notConnectedTab");
                return;
            }

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
            if (Connection.CurrentRole == Connection.Role.Notconnected)
            {
                ChangeStatusConnection();
            }
            else
            {
                ChangeStatusConnection(true);
                string newIp = Helper.GetMyIP();
                if (newIp.CompareTo(labelIP.Text) != 0)
                {
                    timerConnection.Enabled = false;
                    FormAlert formAlert = new FormAlert("IP Changed", "Probably network was changed. You will be disconnected.", true);
                    formAlert.ShowDialog();
                    Disconnect(true);
                    labelIP.Text = Helper.GetMyIP();
                    timerConnection.Enabled = true;
                }
            }
            labelIP.Text = Helper.GetMyIP();
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
            Connection.CreateGroup(newGroup);
            Connection.CurrentGroup.AddUser(m_user);
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

        #region System

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

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

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Connection.EstablishClientConnection("192.168.0.110");
            FormSearchServers formSearchServers = new FormSearchServers(m_searchController);
            if (formSearchServers.ShowDialog() == DialogResult.OK)
            {
                LoadConnectionTab();
            }
        }
        
        

        private void button3_Click(object sender, EventArgs e)
        {
            FormCreateGroup formCreateGroup = new FormCreateGroup();
            if (formCreateGroup.ShowDialog() == DialogResult.OK)
            {
                CreateNewGroup(formCreateGroup.NewGroup);
            }
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
            if (length < 2 || length > 10)
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
            string file = textBoxFile.Text;
            string name = textBoxPresentationName.Text;
            if (file.Length < 1)
                (new FormAlert("No file", "Please, choose presentation file.", true)).ShowDialog();
            else
            {
                try
                {
                    FormLoading formLoading = new FormLoading("Loading presentation. Please wait...");
                    formLoading.Show();
                    

                    await Task.Run(() => PresentationController.LoadPPT(file));

                    formLoading.Close();
                }
                catch (Exception ex)
                {
                    Log.LogException(ex, "Can't load presentation");
                    (new FormAlert("Error", "Problem occured while opening the file", true)).ShowDialog();
                }
            }

            StartPresentation(name);
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
            LoadPresentationTab();
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
            if (Connection.CurrentRole == Connection.Role.Host)
            {
                ServerController.OnGroupClose();
                PresentationController.OnAppClosing();
            }
            else if (Connection.CurrentRole == Connection.Role.Client)
            {
                ViewerController.OnAppClosing();
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
            FormChangeUsername formChangeUsername = new FormChangeUsername(m_user);
            if (formChangeUsername.ShowDialog() == DialogResult.OK)
                ChangeUsername(formChangeUsername.NewUsername);
            labelUsername.Text = m_user.Username;
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }
    }
}
