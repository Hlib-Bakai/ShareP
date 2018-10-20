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

namespace ShareP
{
    public partial class FormMenu : Form
    {
        
        private Connection m_connection;
        private Group m_group;
        private User m_user;
        private ServerController m_serverController;
        private ClientController m_clientController;

        public FormMenu()
        {
            InitializeComponent();
            InitializeElements();
            ChangeStatusConnection();
            LoadConnectionTab();
            //CreateTestParams();

            Log.LogInfo("Initial load successful");
        }

        private void InitializeElements()
        {
            m_user = new User();
            m_serverController = new ServerController();
            m_clientController = new ClientController();
        }

        public void CreateTestParams()
        {
            m_group = new Group();
            m_group.name = "Diploma Seminar";
            m_group.hostId = 1;
            m_group.hostName = "Dariusz Król";
            m_connection = new Connection();
            LoadConnectionTab();
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
            m_group = null;
            m_connection = null;
            LoadConnectionTab();
        }

        private void LoadConnectionTab()
        {
            menuPicker.Height = buttonConnection.Height;
            menuPicker.Top = buttonConnection.Top;

            if (m_connection != null)
            {
                labelConStatus.Text = "Connected";
                labelConStatus.ForeColor = Color.Green;
                if (m_group != null)
                {
                    labelGroupName.Text = m_group.name;
                    labelGroupHost.Text = m_group.hostName;
                }
                tabsConnection.SelectTab("tabConnected");
            }
            else
            {
                labelConStatus.Text = "Not connected";
                labelConStatus.ForeColor = Color.Red;
                labelGroupName.Text = "<Not connected>";
                labelGroupHost.Text = "<Not connected>";
                tabsConnection.SelectTab("tabNotConnected");
            }

            tabsMenu.SelectTab("connectionTab");
        }

        private void LoadPresentationTab()
        {
            menuPicker.Height = buttonPresentation.Height;
            menuPicker.Top = buttonPresentation.Top;

            if (m_connection == null)
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

            if (m_connection == null)
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
            if (m_connection == null)
            {
                ChangeStatusConnection();
            } 
            else
            {
                ChangeStatusConnection(true);
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
            CreateTestParams();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            m_serverController.StartServer();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            m_serverController.StopServer();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FormSearchServers formSearchServers = new FormSearchServers(m_clientController);
            formSearchServers.ShowDialog();
        }
    }
}
