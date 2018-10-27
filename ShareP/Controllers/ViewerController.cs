using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareP.Controllers
{
    static public class ViewerController
    {
        static public void StartLoadingSlides()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += Connection.clientConnection.DownloadPresentationSlidesOnBackground;
            backgroundWorker.RunWorkerAsync();

            // Use progress somewhere
            // https://stackoverflow.com/questions/6481304/how-to-use-a-backgroundworker
        }

        static public void LoadViewer()
        {

        }
    }
}
