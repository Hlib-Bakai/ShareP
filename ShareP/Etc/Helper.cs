using ShareP.Forms;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace ShareP
{
    static class Helper
    {
        static Dictionary<int, Overlay> overlayList;
        static int nLast;

        static Helper()
        {
            overlayList = new Dictionary<int, Overlay>();
            nLast = 0;
        }

        public static string GetMyIP()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        public static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes =
              new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        public static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetCurrentFolder()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static int ShowOverlay(Form parent = null)
        {
            //if (overlay != null && overlay.Visible)
            //    return;

            if (parent == null)
                parent = Connection.FormMenu;
            Overlay overlay = new Overlay();
            overlay.StartPosition = FormStartPosition.Manual;
            
            //overlay.Parent = parent;
            overlay.Left = parent.Left;
            overlay.Top = parent.Top;
            overlay.Size = parent.Size;
            nLast++;
            overlayList.Add(nLast, overlay);
            overlay.Show();
            overlay.BringToFront();
            return nLast;
        }

        public static void HideOverlay(int id)
        {
            if (overlayList.ContainsKey(id))
                overlayList[id].Hide();
        }
    }
}
