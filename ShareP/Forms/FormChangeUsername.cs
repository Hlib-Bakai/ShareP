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
    public partial class FormChangeUsername : Form
    {
        User m_user;
        public String NewUsername
        {
            get;
            set;
        }

        public FormChangeUsername()
        {
            InitializeComponent();
            m_user = Connection.CurrentUser;
            
            textBox2.Text = m_user.Username;
            textBox2.Select(textBox2.Text.Length, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (CheckLength())
                SaveNewUsername();
        }

        private bool CheckLength()
        {
            int length = textBox2.Text.Length;
            if (length < 2 || length > 10)
            {
                labelLength.Show();
                return false;
            }
            return true;
        }

        private void SaveNewUsername()
        {
            NewUsername = textBox2.Text;
            Connection.CurrentUser.Username = NewUsername;
            Properties.Settings.Default["username"] = NewUsername;
            Properties.Settings.Default.Save();
            Log.LogInfo(String.Format("Username changed to {0}.", NewUsername));
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveNewUsername();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
