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
    public partial class FormReconnecting : Form
    {
        private int dots = 1;
        public FormReconnecting()
        {
            InitializeComponent();
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(Connection.clientConnection.CancelReconnecting);
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (dots == 5)
                dots = 1;
            label1.Text = "Some connection problems occured. Trying to reconnect";
            label1.Text += new String('.', dots);
            dots++;
        }
    }
}
