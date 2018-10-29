using ShareP.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareP
{
    static public class ViewerController
    {
        static private FormViewer formViewer;
        static private object _lock = new object();
        
        static public void StartLoadingSlides()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += Connection.clientConnection.DownloadPresentationSlidesOnBackground;
            //backgroundWorker.ProgressChanged += TryLoadImage;
            //backgroundWorker.WorkerReportsProgress = true;
            Connection.clientConnection.downloadingWorker = backgroundWorker;
            backgroundWorker.RunWorkerAsync();

            // Use progress somewhere
            // https://stackoverflow.com/questions/6481304/how-to-use-a-backgroundworker
        }

        static public void DowlonadSlides()
        {

        }

        

        static public void LoadViewer()
        {
            Notification.HideAll();
            formViewer = new FormViewer();
            formViewer.Show();
            formViewer.LoadSlide(1);
        }

        static public void LoadSlide(int slide)
        {
            if (IsWorking)
                formViewer.LoadSlide(slide);
        }

        static public void TryLoadImage(object sender, System.ComponentModel.ProgressChangedEventArgs e) // Delete maybe
        {
            Log.LogInfo("Bot reported progress");
            if (IsWorking)
            {
                //if (formViewer.needsDownload)
                //    formViewer.SetCurrentSlideImage();
            }
            else
            {
                Log.LogInfo("IsWorking returned false");
            }
        }

        static public void CleanTempFiles()
        {
            DirectoryInfo di;
            string path = Helper.GetCurrentFolder() + "tin";
            if (Directory.Exists(path))
            {
                di = new DirectoryInfo(path);
                try
                {
                    di.Delete(true);
                }
                catch (Exception ex)
                {
                    Log.LogException(ex, "Error deleting temp");
                }
            }
        }
        
        static public void EndPresentation()
        {
            formViewer.Close();
            formViewer = null;

            if (Connection.CurrentGroup.settings.Download)
            {
                FormAlert formAlert1 = new FormAlert("Presentation finished", "Would you like to download slides?");
                if (formAlert1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FormProgress formProgress = new FormProgress();
                    formProgress.ShowDialog();
                }
            }
            else
            {
                FormAlert formAlert2 = new FormAlert("Presentation finished", "Thank you for using ShareP!", true);
                formAlert2.ShowDialog();
            }
            Connection.FormMenu.OnViewerClosed();
        }

        static public void OnAppClosing()
        {   if (IsWorking)
                formViewer.Close();
            formViewer = null;
        }

        static public bool IsWorking
        {
            get
            {
                return (formViewer != null && !formViewer.IsDisposed);
            }
        }
    }
}
