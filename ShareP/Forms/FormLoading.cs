using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareP
{
    public partial class FormLoading : Form
    {
        int timeout;
        string basicText;

        public FormLoading(string text, int timeout = -1)
        {
            InitializeComponent();
            Helper.CreateBorder(this);
            label1.Text = text;
            loadingCircle1.NumberSpoke = 36;

            basicText = text;
            this.timeout = timeout;
            
            //loadingCircle1.Focus();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_MouseUp(object sender, MouseEventArgs e)
        {
        }

        private void loadingCircle1_Click(object sender, EventArgs e)
        {

        }

        private void FormLoading_Shown(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timeout > 0)
            {
                label1.Text = basicText + string.Format(" [{0}]", timeout);
                timeout--;
            }
        }
    }
}
