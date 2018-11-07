﻿using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            
        }

        static public void StartApp()
        {
            app = new Application();
            ppts = app.Presentations;

            app.SlideShowNextSlide += OnNextSlide;
            app.SlideShowEnd += OnSlideShowEnd;
        }

        static public void LoadPPT(string pptPath)
        {
            StartApp();

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

            //while (app.SlideShowWindows.Count <= 0) ;

            SlideShowWindow ssw = ppt.SlideShowWindow;
            ssv = ssw.View;
        }

        static public void ExportImages(string destinationPath)
        {
            DirectoryInfo di;
            string path = destinationPath + "tout"; 
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
                ppt.Slides[i].Export(di.FullName + @"\" + i + ".dat", "jpg");             
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

        public static void OnAppClosing() 
        {
            CloseApp();
        }

        private static void CloseApp()  //Maybe not the best variant; didn't find other ways
        {
            if (ppt == null)
                return;
            Process[] processes = Process.GetProcessesByName("powerpnt");
            for (int i = 0; i < processes.Count(); i++)
            {
                processes[i].Kill();
            }
        }

        public static void CleanTempFiles()
        {
            DirectoryInfo di;
            string path = Helper.GetCurrentFolder() + "tout";
            if (Directory.Exists(path))
            {
                di = new DirectoryInfo(path);
                di.Delete(true);
            }
        }
       
    }
}
