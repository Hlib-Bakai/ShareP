using ShareP.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareP.Forms
{
    public partial class FormSearchServers : Form
    {
        private SearchController m_clientController;

        public FormSearchServers(SearchController clientController)
        {
            InitializeComponent();
            m_clientController = clientController;
            textBox1.Tag = 1;
            StartSearch();
        }

        private void StartSearch()
        {
            listView1.Items.Clear();
            timer1.Enabled = true;
            m_clientController.FindServersAsync(this);
        }

        public void SearchStopped()
        {

            timer1.Enabled = false;
            if (textBox1.InvokeRequired)  //Accessing element from another thread
            {
                textBox1.Invoke(new Action<string>((s) => textBox1.Text = s), "Search finished. Found: " + listView1.Items.Count);
            }
            else
            {
                textBox1.Text = "Search finished. Found: " + listView1.Items.Count;
            }
        }

        public void AddGroup(Group group, string nUsers)
        {
            try
            {
                string[] newElement = { group.name, nUsers, group.hostName, group.hostIp};
                var newListViewItem = new ListViewItem(newElement);
                newListViewItem.ImageIndex = (group.passwordProtected) ? 0 : -1;

                if (listView1.InvokeRequired)  //Accessing element from another thread
                {
                    listView1.Invoke(new Action<ListViewItem>((i) => listView1.Items.Add(i)), newListViewItem);
                }
                else
                {
                    listView1.Items.Add(newListViewItem);
                    listView1.Refresh();
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
        }

        public void Connect()
        {
            if (listView1.SelectedItems.Count < 1)
                return;

            string ip = listView1.SelectedItems[0].SubItems[3].Text;
            string name = listView1.SelectedItems[0].SubItems[0].Text;
            bool passwordProtected = (listView1.SelectedItems[0].ImageIndex == 0) ? true : false;
            byte[] password = null;
            if (!String.IsNullOrEmpty(ip))
            {
                if (passwordProtected)
                {
                    FormPassword formPassword = new FormPassword(name);
                    int overlay = Helper.ShowOverlay(this);
                    if (formPassword.ShowDialog() == DialogResult.OK)
                    {
                        password = formPassword.Password;
                        Helper.HideOverlay(overlay);
                    }
                    else
                    {
                        Helper.HideOverlay(overlay);
                        return;
                    }
                }

                switch (Connection.EstablishClientConnection(ip, password))
                {
                    case ConnectionResult.Error:
                        int overlay = Helper.ShowOverlay(this);
                        FormAlert formAlertError = new FormAlert("Error", "Some error occured during connection.", true);
                        formAlertError.ShowDialog();
                        Helper.HideOverlay(overlay);
                        StartSearch();
                        break;
                    case ConnectionResult.WrongPassword:
                        int overlay2 = Helper.ShowOverlay(this);
                        FormAlert formAlertPassword = new FormAlert("Wrong password", "Password you entered is incorrect. Prease try again.", true);
                        formAlertPassword.ShowDialog();
                        Helper.HideOverlay(overlay2);
                        Connect();
                        break;
                    case ConnectionResult.Success:
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        break;
                }
            }
            else
            {
                // TODO
            }
        }

        #region Events

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartSearch();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((int)textBox1.Tag == 5)
            {
                textBox1.Tag = 1;
            }

           textBox1.Text = "Searching";

            for (int i = 0; i < (int)textBox1.Tag; i++)
                textBox1.AppendText(".");
            textBox1.Tag = (int)textBox1.Tag + 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            Connect();
        }


        #endregion


    }
}
