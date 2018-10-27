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
        public FormLoading(string text)
        {
            InitializeComponent();
            label1.Text = text;
            loadingCircle1.NumberSpoke = 36;
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
    }
}
