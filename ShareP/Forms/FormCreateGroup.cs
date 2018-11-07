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
    public partial class FormCreateGroup : Form
    {
        public FormCreateGroup()
        {
            InitializeComponent();
            comboBoxNavigation.SelectedIndex = 0;
        }

        private void CreateGroup()
        {
            NewGroup = new Group();
            NewGroup.name = textBoxName.Text;
            NewGroup.passwordProtected = checkBoxPassword.Checked;
            if (NewGroup.passwordProtected)
                NewGroup.password = Helper.GenerateSaltedHash(Encoding.UTF8.GetBytes(textBoxPassword.Text), 
                                                                    Encoding.UTF8.GetBytes(NewGroup.name)); // Pass + salt

            switch (comboBoxNavigation.SelectedIndex)
            {
                case 0:
                    NewGroup.navigation = GroupNavigation.FollowOnly;
                    break;
                case 1:
                    NewGroup.navigation = GroupNavigation.Backwards;
                    break;
                case 2:
                    NewGroup.navigation = GroupNavigation.BothDirections;
                    break;
                default:
                    break;
            }
            NewGroup.settings.Download = checkBoxDownload.Checked;
            NewGroup.settings.Viewerspresent = checkBoxViewersPresent.Checked;
            NewGroup.settings.NConnected = checkBoxNConnected.Checked;
            NewGroup.settings.NDisconnected = checkBoxNDisconnected.Checked;
            NewGroup.settings.NChat = checkBoxNChat.Checked;
            NewGroup.settings.NCheater = checkBoxNCheater.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public Group NewGroup
        {
            get;
            set;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBoxPassword.Enabled = !(textBoxPassword.Enabled);
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; //prevent typing
        }

        private bool CheckLength()
        {
            int length = textBoxName.Text.Length;
            if (length < 2 || length > 10)
            {
                labelLength.Show();
                return false;
            }
            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (CheckLength())
                CreateGroup();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
