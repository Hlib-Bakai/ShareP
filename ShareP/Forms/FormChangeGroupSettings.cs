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
    public partial class FormChangeGroupSettings : Form
    {
        public FormChangeGroupSettings()
        {
            InitializeComponent();
            checkBoxDownload.Checked = Connection.CurrentGroup.settings.Download;
            checkBoxViewersPresent.Checked = Connection.CurrentGroup.settings.Viewerspresent;
            comboBoxNavigation.SelectedIndex = (int)Connection.CurrentGroup.navigation;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Connection.CurrentGroup.settings.Download = checkBoxDownload.Checked;
            Connection.CurrentGroup.settings.Viewerspresent = checkBoxViewersPresent.Checked;
            Connection.CurrentGroup.navigation = (GroupNavigation)comboBoxNavigation.SelectedIndex;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void comboBoxNavigation_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; //prevent typing
        }
    }
}
