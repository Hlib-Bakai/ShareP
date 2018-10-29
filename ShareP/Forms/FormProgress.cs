using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareP.Forms
{
    public partial class FormProgress : Form
    {
        BackgroundWorker backgroundWorker;
        string dest = "";

        public FormProgress()
        {
            InitializeComponent();
        }

        private void StartLoading()
        {
            backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += ConvertSlides;
            backgroundWorker.ProgressChanged += ChangeProgress;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerCompleted += WorkerCompleted;  
            backgroundWorker.RunWorkerAsync();
        }

        private void ChangeProgress(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // canceled
            }
            else if (e.Error != null)
            {
                Log.LogException(e.Error, "Error during converting");
                FormAlert formAlert = new FormAlert("Error", "Error occured", true);
                formAlert.ShowDialog();
                this.Close();
            }
            else
            {
                FormAlert formAlert = new FormAlert("Finished", "Slides downloaded to " + dest, true);
                formAlert.ShowDialog();
                this.Close();
            }
        }

        public void ConvertSlides(object sender, DoWorkEventArgs e)
        {
            DirectoryInfo di;
            string sourceFolder = Helper.GetCurrentFolder() + @"tin\";
            string destinationFolder = String.Format("{0}{1}({2})\\", Helper.GetCurrentFolder(), Connection.CurrentPresentation.Name, DateTime.Now.ToShortDateString());
            dest = destinationFolder;
            if (!Directory.Exists(sourceFolder))
            {
                return;
            }
            if (!Directory.Exists(destinationFolder))
            {
                di = Directory.CreateDirectory(destinationFolder);
            }

            try
            {
                Log.LogInfo("Start converting slides (" + Connection.CurrentPresentation.SlidesTotal + ")");
                int total = Connection.CurrentPresentation.SlidesTotal;
                for (int i = 1; i <= Connection.CurrentPresentation.SlidesTotal; i++)
                {
                    File.Copy(sourceFolder + i.ToString() + ".dat", destinationFolder + "Slide" + i.ToString() + ".jpg", true);
                    backgroundWorker.ReportProgress((i / total) * 100);
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Log.LogException(ex, "Error loading slide");
            }
        }

        private void FormProgress_Shown(object sender, EventArgs e)
        {
            StartLoading();
        }
    }
}
