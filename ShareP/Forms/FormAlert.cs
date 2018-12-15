using System.Drawing;
using System.Windows.Forms;

namespace ShareP.Forms
{
    public partial class FormAlert : Form
    {
        public FormAlert(string title, string message, bool oneButton = false)
        {
            InitializeComponent();
            Helper.CreateBorder(this);
            label1.Text = title;
            label2.Text = message;

            if (oneButton)
            {
                button2.Hide();
                button3.Hide();
                button4.Show();
                buttonClose.Hide();
            }
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button3_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button4_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void FormAlert_Load(object sender, System.EventArgs e)
        {

        }
    }
}
