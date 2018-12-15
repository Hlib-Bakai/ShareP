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
    public partial class FormChangeIp : Form
    {
        public FormChangeIp()
        {
            InitializeComponent();
            //Helper.CreateBorder(this);
            var IPs = Helper.GetLocalIPs();
            foreach(var ip in IPs)
            {
                comboBoxIPs.Items.Add(ip);
            }
            if (comboBoxIPs.Items.Count > 0)
                comboBoxIPs.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBoxIPs.SelectedItem.ToString() != "")
                Helper.IP = comboBoxIPs.SelectedItem.ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void comboBoxIPs_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; //prevent typing
        }
    }
}
