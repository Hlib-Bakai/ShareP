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
        static private Microsoft.Office.Interop.PowerPoint.Presentation ppt;
        static private SlideShowView ssv;

        static PresentationController()
        {
            app = new Application();
            ppts = app.Presentations;

            app.SlideShowNextSlide += OnNextSlide;
            app.SlideShowEnd += OnSlideShowEnd;
        }

        static public void LoadPPT(string pptPath)
        {
            ppt = ppts.Open(pptPath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
        
            ExportImages(Helper.GetCurrentFolder());
        }

        static public void StartSlideShow()
        {
            Connection.CurrentPresentation.SlidesTotal = ppt.Slides.Count;

            ServerController.OnPresentationStart(Connection.CurrentPresentation);

            app.Visible = MsoTriState.msoTrue; // Window showing
            SlideShowSettings sss = ppt.SlideShowSettings;
            sss.Run();

            while (app.SlideShowWindows.Count <= 0) ;

            SlideShowWindow ssw = ppt.SlideShowWindow;
            ssv = ssw.View;
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
            }
        }

        private static void OnNextSlide(SlideShowWindow Wn)
        {
            int currentSlide = Wn.View.CurrentShowPosition;
            ServerController.OnPresentationNextSlide(currentSlide);
            Connection.CurrentPresentation.CurrentSlide = currentSlide;
        }

        private static void OnSlideShowEnd(Microsoft.Office.Interop.PowerPoint.Presentation presentation)
        {
            ServerController.OnPresentationEnd();
            Connection.CurrentPresentation = null;
            Connection.FormMenu.OnPresentationFinished();
            CloseApp();
        }

        public static void OnAppClosing() //Fix this
        {
            return;
            if (ssv != null)
                ssv.Exit();
            CloseApp();
        }

        private static void CloseApp() //TODO
        {
            if (ppt == null)
                return;
            Slides slides = ppt.Slides;
            for (int i = 1; i <= slides.Count; i++)
            {
                Slide slide = slides[i];
                String slideName = slide.Name;
                ReleaseCOM(slide);
            }
        }

        private static void ReleaseCOM(object o)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(o);
            }
            catch { }
            finally
            {
                o = null;
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
