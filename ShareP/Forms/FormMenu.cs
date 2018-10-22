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
        private ClientController m_clientController;

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
            textBoxUsername.BackColor = System.Drawing.SystemColors.Window;
            textBoxIP.BackColor = System.Drawing.SystemColors.Window;

            m_user = new User();
            textBoxUsername.Text = m_user.Username;

            Connection.CurrentUser = m_user;

            textBoxIP.Text = m_user.IP;

            listBox1.DrawItem += new DrawItemEventHandler(listBox_DrawItem);

            //Controllers
            m_clientController = new ClientController();
        }

        public void FillHostUsersList()
        {
            listBox1.Items.Clear();
            foreach(User user in Connection.CurrentGroup.userList)
            {
                listBox1.Items.Add(user.Username);
            }
            listBox1.Refresh();
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

        private void Disconnect()
        {
            Connection.Disconnect();
            LoadConnectionTab();
        }

        private void LoadConnectionTab()
        {
            menuPicker.Height = buttonConnection.Height;
            menuPicker.Top = buttonConnection.Top;

            if (Connection.GetRole() != Connection.Role.Notconnected)
            {
                labelConStatus.Text = "Connected";
                labelConStatus.ForeColor = Color.Green;
                labelGroupName.Text = Connection.CurrentGroup.name;
                labelGroupHost.Text = Connection.CurrentGroup.hostName;
                buttonDisconnect.Show();
                if (Connection.GetRole() == Connection.Role.Client)
                    tabsConnection.SelectTab("tabConnected");
                else if (Connection.GetRole() == Connection.Role.Host)
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

            if (Connection.GetRole() == Connection.Role.Notconnected)
            {
                tabsMenu.SelectTab("notConnectedTab");
                return;
            }
        
            tabsMenu.SelectTab("presentationTab");
        }
        

        private void LoadMessagesTab()
        {
            menuPicker.Height = buttonMessages.Height;
            menuPicker.Top = buttonMessages.Top;

            if (Connection.GetRole() == Connection.Role.Notconnected)
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
            if (Connection.GetRole() == Connection.Role.Notconnected)
            {
                ChangeStatusConnection();
            } 
            else
            {
                ChangeStatusConnection(true);
            }
            textBoxIP.Text = Helper.GetMyIP();
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
            newGroup.formMenu = this;
            Connection.CreateGroup(newGroup);
            Connection.CurrentGroup.AddUser(m_user);
            LoadConnectionTab();
            CheckStatusConnection();
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
            FormSearchServers formSearchServers = new FormSearchServers(m_clientController);
            if (formSearchServers.ShowDialog() == DialogResult.OK)
            {
                LoadConnectionTab();
            }
        }

        private void textBoxUsername_MouseHover(object sender, EventArgs e)
        {
            pictureBoxEditUsername.Show();
        }

        private void textBoxUsername_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxEditUsername.Hide();
        }

        private void textBoxUsername_Click(object sender, EventArgs e)
        {
            FormChangeUsername formChangeUsername = new FormChangeUsername(m_user);
            formChangeUsername.ShowDialog();
            textBoxUsername.Text = m_user.Username;
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
        }

        private void textBoxFile_DoubleClick(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            string file = textBoxFile.Text;
            if (file.Length < 1)
                (new FormAlert("No file", "Please, choose presentation file.", true)).ShowDialog();
            else
            {
                try
                {
                    PresentationController.LoadPPT(file);
                }
                catch (Exception ex)
                {
                    Log.LogException(ex, "Can't load presentation");
                    (new FormAlert("Error", "Problem occured while opening the file", true)).ShowDialog();
                }
            }
        }
    }
}
