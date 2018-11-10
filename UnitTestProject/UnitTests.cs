using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShareP;
using ShareP.Controllers;

namespace UnitTestProject
{
    [TestClass]
    public class SharePTests
    {
        [TestMethod]
        public void TestFailedConnectionNotChangesRole()
        {
            Connection.CurrentRole = Role.Notconnected;
            Connection.EstablishClientConnection("1.2.3.4");
            Assert.AreEqual(Role.Notconnected, Connection.CurrentRole);
        }

        [TestMethod]
        public void TestUserAddToGroup()
        {
            Group group = new Group();
            group.AddUser(new User());
            Assert.AreEqual(group.userList.Count, 1);
        }

        [TestMethod]
        public void TestUserRemovedFromGroup()
        {
            Group group = new Group();
            User user = new User() { Username = "test"};
            group.AddUser(user);
            group.RemoveUser(user);
            Assert.AreEqual(group.userList.Count, 0);
        }

        [TestMethod]
        public void TestGroupUserCount()
        {
            Group group = new Group();
            User user = new User() { Username = "test" };
            group.AddUser(user);
            Assert.AreEqual(group.GetUsersCount(), 1);
            group.RemoveUser(user);
            Assert.AreEqual(group.GetUsersCount(), 0);
        }

        [TestMethod]
        public void TestGetServiceOnFakeIp()
        {
            ClientController clientController = new ClientController();
            Assert.AreEqual(clientController.GetServiceOnIP("1.2.3.4"), null);
        }

        [TestMethod]
        public void TestDisconnectOnNoConnection()
        {
            (new ClientController()).Disconnect();
        }

        [TestMethod]
        public void TestPresentationStartWithNoConnection()
        {
            ClientController clientController = new ClientController();
            clientController.ClPresentationStart(null);
            clientController.ClPresentationNextSlide(0);
            clientController.ClPresentationEnd();
        }

        [TestMethod]
        public void TestPresentationNextSlideWithNoConnection()
        {
            ClientController clientController = new ClientController();
            clientController.ClPresentationNextSlide(0);
        }

        [TestMethod]
        public void TestPresentationEndWithNoConnection()
        {
            ClientController clientController = new ClientController();
            clientController.ClPresentationEnd();
        }

        [TestMethod]
        public void TestRequestNotExistingSlide()
        {
            ClientController clientController = new ClientController();
            Assert.AreEqual(clientController.ClRequestSlide(1), null);
        }

        [TestMethod]
        public void TestCheckPresentationApp()
        {
            Assert.IsInstanceOfType(PresentationController.CheckApp(), typeof(bool));
        }

        [TestMethod]
        public void TestPresentationOnEventWhenNotConnected()
        {
            PresentationController.OnSlideShowEnd(null);
        }

        [TestMethod]
        public void TestOnClosingWhenNoAppLaunched()
        {
            PresentationController.OnAppClosing();
        }

        [TestMethod]
        public void TestStartServer()
        {
            Assert.IsTrue(ServerController.StartServer());
        }
    }
}
