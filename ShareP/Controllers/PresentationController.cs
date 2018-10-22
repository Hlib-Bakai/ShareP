using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShareP.Controllers
{
    class PresentationController
    {
        static private Application app;
        static private Presentations ppts;
        static private Presentation ppt;

        static PresentationController()
        {
            app = new Application();
            app.Visible = MsoTriState.msoTrue;
            ppts = app.Presentations;
        }

        static public void LoadPPT(string pptPath)
        {
            ppt = ppts.Open(pptPath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
            ExportImages(Helper.GetCurrentFolder());

            SlideShowSettings sss = ppt.SlideShowSettings;
            sss.Run();

            while (app.SlideShowWindows.Count <= 0) ;

            SlideShowWindow ssw = ppt.SlideShowWindow;
            SlideShowView ssv = ssw.View;

        }

        static public void ExportImages(string destinationPath)
        {
            DirectoryInfo di;
            string path = destinationPath + "slides"; 
            if (!Directory.Exists(path))
            {
                di = Directory.CreateDirectory(path);
            }
            else
            {
                di = new DirectoryInfo(path);
            }
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            for (int i = 1; i <= ppt.Slides.Count; i++)
            {
                ppt.Slides[i].Export(di.FullName + @"\slide" + i + ".jpg", "jpg");
               
                //TryUntilSuccess(() =>
                //{
                //    ppt.Slides[i].Export(di.FullName + @"\slide" + i + ".jpg", "jpg");
                //});
            }
        }

        private static void TryUntilSuccess(Action action)  //Delete if no need
        {
            //bool success = false;
            //while (!success)
            //{
            //    try
            //    {
            //        action();
            //        success = true;
            //    }

            //    catch (System.Runtime.InteropServices.COMException e)
            //    {
            //        // Excel is busy
            //        Thread.Sleep(500); // Wait, and...
            //        success = false;  // ...try again
            //    }
            //}
        }
    }
}
