using ShareP.Controllers;
using ShareP.Server;
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
        private SearchController m_searchController;
        private object _lock = new object();

        public FormSearchServers(SearchController clientController)
        {
            InitializeComponent();
            m_searchController = clientController;
            textBox1.Tag = 1;
            StartSearch();
        }

        private void StartSearch()
        {
            button2.Enabled = false;
            listView1.Items.Clear();
            timer1.Enabled = true;
            m_searchController.FindServersAsync(this);
        }

        public void FastSearchFinished()
        {
            if (button2.InvokeRequired)
                button2.Invoke(new Action(() => button2.Enabled = true));
            else
                button2.Enabled = true;
        }

        public void SearchStopped()
        {

            timer1.Enabled = false;
            if (textBox1.InvokeRequired)  //Accessing element from another thread
                textBox1.Invoke(new Action<string>((s) => textBox1.Text = s), "Search finished. Found: " + listView1.Items.Count);
            else
                textBox1.Text = "Search finished. Found: " + listView1.Items.Count;

            if (button2.InvokeRequired)
                button2.Invoke(new Action(() => button2.Enabled = true));
            else
                button2.Enabled = true;

        }

        public void AddGroup(Group group, string nUsers)
        {
            try
            {
                Func<string, bool> groupExists = s =>
                {
                    foreach (ListViewItem item in listView1.Items)
                    {
                        if (item.SubItems[0].Text.CompareTo(s) == 0)
                            return true;
                    }
                    return false;
                };

                lock (_lock)
                {
                    if (listView1.InvokeRequired)
                    {
                        if ((bool)listView1.Invoke(groupExists, group.name))
                            return;
                    }
                    else
                        if (groupExists(group.name))
                        return;

                    string[] newElement = { group.name, nUsers, group.hostName, group.hostIp };
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
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }

        }

        public void Connect(string ip = null, string name = null, bool passwordProtected = false)
        {
            if (listView1.SelectedItems.Count < 1 && ip == null)
                return;

            m_searchController.StopSearch();
            
            if (ip == null)
            {
                ip = listView1.SelectedItems[0].SubItems[3].Text;
                name = listView1.SelectedItems[0].SubItems[0].Text;
                passwordProtected = (listView1.SelectedItems[0].ImageIndex == 0) ? true : false;
            }
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
                        FormAlert formAlertError2 = new FormAlert("Wrong password", "Password you entered is incorrect. Prease try again.", true);
                        formAlertError2.ShowDialog();
                        Helper.HideOverlay(overlay2);
                        Connect();
                        break;
                    case ConnectionResult.UsernameExists:
                        int overlay3 = Helper.ShowOverlay(this);
                        FormAlert formAlertError3 = new FormAlert("Error", "Such username already exists in a group", true);
                        formAlertError3.ShowDialog();
                        Helper.HideOverlay(overlay3);
                        StartSearch();
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

        private async void buttonDirect_ClickAsync(object sender, EventArgs e)
        {
            int ov1 = Helper.ShowOverlay(this);
            FormDirectConnect formDirectConnect = new FormDirectConnect();
            if (formDirectConnect.ShowDialog() == DialogResult.OK)
            {
                string ipToConnect = formDirectConnect.IP;
                int ov2 = Helper.ShowOverlay(this);
                FormLoading formLoading = new FormLoading("Connecting to " + ipToConnect, 20);
                formLoading.Show();
                var result = await Task.Run(() => Connection.GetServiceOnIP(ipToConnect));
                formLoading.Close();
                Helper.HideOverlay(ov2);
                if (result != null)
                {
                    bool pass = (result["Password"].CompareTo("True") == 0) ? true : false;
                    string groupName = result["GroupName"];
                    Connect(ipToConnect, groupName, pass);
                }
                else
                {
                    int ov3 = Helper.ShowOverlay(this);
                    FormAlert formAlert = new FormAlert("Error", "Can't establish connection to the server. Check provided IP", true);
                    formAlert.ShowDialog();
                    Helper.HideOverlay(ov3);
                }
            }
            Helper.HideOverlay(ov1);
        }
    }
}
