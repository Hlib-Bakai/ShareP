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
    public partial class FormPassword : Form
    {
        public byte[] Password
        {
            get;
            set;
        }

        private string groupName;

        public FormPassword(string groupName)
        {
            InitializeComponent();
            //Helper.CreateBorder(this);
            this.groupName = groupName;
            textBox1.Focus();
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
            SaveAndExit();
        }

        private void SaveAndExit()
        {
            Password = Helper.GenerateSaltedHash(Encoding.UTF8.GetBytes(textBox1.Text),
                                                                    Encoding.UTF8.GetBytes(groupName));
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SaveAndExit();
        }
    }
}
