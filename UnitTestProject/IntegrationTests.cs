using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShareP;
using ShareP.Controllers;
using ShareP.Forms;
using System.IO;

namespace UnitTestProject
{
    [TestClass]
    public class SharePIntegrationTests
    {
        [TestMethod]
        public void TestDownloadSlideToFolder()
        {
            var ip = LaunchFakeServer();
            Connection.EstablishClientConnection(ip);
            ViewerController.StartLoadingSlides();
            string path = Helper.GetCurrentFolder() + @"Animals(" + DateTime.Now.ToShortDateString() +")";
            Assert.IsTrue(Directory.Exists(path));
            var din = new DirectoryInfo(path);
            Assert.AreEqual(20, din.GetFiles().GetLength(0));
        }

        [TestMethod]
        public void TestServerUserList()
        {
            var ip = LaunchFakeServer();
            Connection.EstablishClientConnection(ip);
            var users1 = Connection.clientConnection.RequestUsersList();
            Assert.AreEqual(1, users1.Count);
            ConnectFakeClient(ip);
            var users2 = Connection.clientConnection.RequestUsersList();
            Assert.AreEqual(2, users2.Count);
            DisconnectAllFakeClient(ip);
            var users3 = Connection.clientConnection.RequestUsersList();
            Assert.AreEqual(1, users3.Count);
        }

        private string LaunchFakeServer()
        {
            return "";
        }

        private void ConnectFakeClient(string ip)
        {
            //
        }

        private void DisconnectAllFakeClient(string ip)
        {
            //
        }
    }
}
