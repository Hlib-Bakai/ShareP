﻿using ShareP.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace ShareP
{
    public static class Helper
    {
        static Dictionary<int, Overlay> overlayList;
        static int nLast;
        
        static Helper()
        {
            overlayList = new Dictionary<int, Overlay>();
            nLast = 0;
            IP = GetMyIP();
        }

        public static string IP { get; set; }

        public static List<string> GetLocalIPs()
        {
            IPHostEntry host;
            var localIPs = new List<string>();
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIPs.Add(ip.ToString());
                }
            }
            return localIPs;
        }

        private static bool IfIPStillAvailable(string ipToCheck)
        {
            IPHostEntry host;
            var localIPs = new List<string>();
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIPs.Add(ip.ToString());
                }
            }
            return localIPs.Contains(ipToCheck);
        }

        public static string GetMyIP()
        {
            if (IP != null && IfIPStillAvailable(IP))
                return IP;
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
            if (array1 == null || array2 == null)
                return false;

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
            overlay.BringToFront();

            if (parent.InvokeRequired)
                parent.Invoke(new Action<Form>((p) => overlay.Show(p)), parent);
            else
                overlay.Show(parent);

            return nLast;
        }

        public static void HideOverlay(int id)
        {
            if (overlayList.ContainsKey(id))
            {
                try
                {
                    if (overlayList[id].Owner.InvokeRequired)
                        overlayList[id].Owner.Invoke(new Action<int>((i) => overlayList[i].Owner.Focus()), id);
                    else
                        overlayList[id].Owner.Focus();
                }
                catch { }
                if (overlayList[id].InvokeRequired)
                    overlayList[id].Invoke(new Action<int>((i) => overlayList[i].Hide()), id);
                else
                    overlayList[id].Hide();
            }
        }

        public static string GenerateUniqueId()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString();
        }

        public static void CreateBorder(Form target)
        {
            //Top
            Panel top = new Panel();
            top.BackColor = Color.FromArgb(0, 162, 232);
            top.Location = target.Location;
            top.Size = new Size(target.Size.Width, 1);
            top.Tag = "nodelete";
            target.Controls.Add(top);
            top.Show();

            //Right
            Panel right = new Panel();
            right.BackColor = Color.FromArgb(0, 162, 232);
            right.Location = new Point(target.Location.X + target.Size.Width - 1, target.Location.Y);
            right.Size = new Size(1, target.Size.Height);
            right.Tag = "nodelete";
            target.Controls.Add(right);
            right.Show();

            //Bottom
            Panel bottom = new Panel();
            bottom.BackColor = Color.FromArgb(0, 162, 232);
            bottom.Location = new Point(target.Location.X, target.Location.Y + target.Size.Height - 1);
            bottom.Size = new Size(target.Size.Width, 1);
            bottom.Tag = "nodelete";
            target.Controls.Add(bottom);
            bottom.Show();

            //Left
            Panel left = new Panel();
            left.BackColor = Color.FromArgb(0, 162, 232);
            left.Location = target.Location;
            left.Size = new Size(1, target.Size.Height);
            left.Tag = "nodelete";
            target.Controls.Add(left);
            left.Show();
        }
    }
}
