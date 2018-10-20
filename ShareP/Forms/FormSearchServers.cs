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
        private ClientController m_clientController;

        public FormSearchServers(ClientController clientController)
        {
            InitializeComponent();
            m_clientController = clientController;
            textBox1.Tag = 1;
        }

        private void StartSearch()
        {
            listView1.Items.Clear();
            timer1.Enabled = true;
            m_clientController.FindServersAsync(this);

        }

        public void SearchStopped(int totalPings)
        {
            timer1.Enabled = false;
            textBox1.Text = "Search finished. Scanned " + totalPings;
        }

        public void AddGroup(Group group)
        {
            try
            {
                string[] newElement = { group.name, group.hostName, group.hostIp };
                var newListViewItem = new ListViewItem(newElement);
                newListViewItem.ImageIndex = (group.passwordProtected) ? 0 : -1;
                listView1.Items.Add(newListViewItem);

                listView1.Refresh();
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
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

#endregion 
    }
}
