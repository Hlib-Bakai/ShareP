using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareP.Forms
{
    public partial class FormDirectConnect : Form
    {
        public FormDirectConnect()
        {
            InitializeComponent();
            //Helper.CreateBorder(this);
            textBox1.Text = Helper.GetMyIP();
        }

        private void label1_Click(object sender, EventArgs e)
        {

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
            Match match = Regex.Match(textBox1.Text, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.None);

            if (match.Success)
            {
                IP = textBox1.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                labelInvalid.Show();
            }
        }

        public string IP
        {
            get;
            set;
        }
    }
}
