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
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
            //Helper.CreateBorder(this);
            labelUsername.Text = Connection.CurrentUser.Username;
            checkBoxAutojoin.Checked = (bool)Properties.Settings.Default["autojoin"];
            checkBoxPresentation.Checked = (bool)Properties.Settings.Default["nPresentation"];
            checkBoxChat.Checked = (bool)Properties.Settings.Default["nChat"];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SaveData()
        {
            Properties.Settings.Default["autojoin"] = checkBoxAutojoin.Checked;
            Properties.Settings.Default["nPresentation"] = checkBoxPresentation.Checked;
            Properties.Settings.Default["nChat"] = checkBoxChat.Checked;

            Properties.Settings.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveData();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonUsername_Click(object sender, EventArgs e)
        {
            int overlay = Helper.ShowOverlay(this);
            FormChangeUsername formChangeUsername = new FormChangeUsername();
            if (Connection.CurrentRole != Role.Notconnected)
            {
                FormAlert formAlert = new FormAlert("Error", "You are not allowed to change username when connected.", true);
                formAlert.ShowDialog();
            }
            else if (formChangeUsername.ShowDialog() == DialogResult.OK)
                labelUsername.Text = Connection.CurrentUser.Username;
            Helper.HideOverlay(overlay);
        }
    }
}
