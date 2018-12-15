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
    public partial class FormHelpCheater : Form
    {
        public FormHelpCheater()
        {
            InitializeComponent();
            //Helper.CreateBorder(this);
            label2.Text = "is a function designed mostly for teachers. During test students " +
            "should not use external software, internet etc. "+
            "When starting presentation, you have possibility to turn on \"Cheater\". "+
            "While presenting, additional window will be shown. If viewer hides presentation "+
            "window or goes to any other application(window lost focus), his name will "+
            "appear on CHEATER window .If viewer returns to presentation, his icon "+
            "will become green. After 10 seconds it will disappear, unless he'll leave "+
            "presentation again.";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
