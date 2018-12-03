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

            if (Connection.CurrentRole == Role.Client)
                backgroundWorker.DoWork += Connection.clientConnection.DownloadPresentationSlidesOnBackground;

            Connection.clientConnection.downloadingWorker = backgroundWorker;
            backgroundWorker.RunWorkerAsync();
        }
        
        static public void LoadViewer()
        {
            Notification.HideAll();
            formViewer = new FormViewer();
            formViewer.Show();
            formViewer.BringToFront();
            formViewer.LoadSlide(1);
        }

        static public void LoadSlide(int slide)
        {
            if (IsWorking)
            {
                if (formViewer.InvokeRequired)
                    formViewer.Invoke(new Action<int>((s) => formViewer.LoadSlide(s)), slide);
                else
                    formViewer.LoadSlide(slide);
            }
        }

        static public void CleanTempFiles()
        {
            try
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
            catch { }
        }
        
        static public void EndPresentation()
        {
            if (formViewer.InvokeRequired)
                formViewer.Invoke(new Action(() => formViewer.Close()));
            else
                formViewer.Close();
            formViewer = null;
            
            if (Connection.CurrentGroup.settings.Download)
            {
                FormAlert formAlert1 = new FormAlert("Presentation finished", "Would you like to download slides?");
                if (formAlert1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FormDownloadSlide formProgress = new FormDownloadSlide();
                    formProgress.ShowDialog();
                }
            }
            else
            {
                FormAlert formAlert2 = new FormAlert("Presentation finished", "Thank you for using ShareP!", true);
                formAlert2.ShowDialog();
            }
            if (Connection.FormMenu.InvokeRequired)
                Connection.FormMenu.Invoke(new Action(() => Connection.FormMenu.OnViewerClosed()));
            else
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
