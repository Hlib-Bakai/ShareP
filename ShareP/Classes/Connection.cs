﻿using ShareP.Controllers;
using ShareP.Server;
using System.Collections.Generic;

namespace ShareP
{
    static public class Connection
    {
        static private Group currentGroup;
        //private static SharePClient client;
        static private Role role = Role.Notconnected;
        static private User currentUser = null;
        static private Presentation currentPresentation = null;


        static string rcvFilesPath = @"TODO";
        private delegate void FaultedInvoker();
        static List<User> onlineUsers = new List<User>();
        static ClientController clientConnection = new ClientController();


        public static ConnectionResult EstablishClientConnection(string ip, byte[] password = null)
        {
            var result = clientConnection.EstablishClientConnection(ip, password);
            if (result == ConnectionResult.Success)
            {
                LoadGroupParameters(ip);
                CurrentRole = Role.Client;
            }
            return result;
        }

        private static void LoadGroupParameters(string ip)
        {
            Group group = new Group();
            group.hostIp = ip;

            var groupInfo = clientConnection.RequestServerInfo();
            group.name = groupInfo["GroupName"];
            group.hostName = groupInfo["HostName"];
            group.settings.Download = (groupInfo["Download"].CompareTo("True") == 0) ? true : false;
            group.settings.Viewerspresent = (groupInfo["ViewersPresent"].CompareTo("True") == 0) ? true : false;

            CurrentGroup = group;
        }

        public static Dictionary<string, string> GetServiceOnIP(string ip)
        {
            return clientConnection.GetServiceOnIP(ip);
        }

        public static void CreateGroup(Group group)
        {
            ServerController.StartServer();
            CurrentGroup = group;
            CurrentRole = Role.Host;
        }

        public static void Disconnect()
        {
            if (role == Role.Client)
                DisconnectClient();
            else if (role == Role.Host)
                DisconnectServer();
        }

        private static void DisconnectClient()
        {
            //Send a server command about disconnect
            clientConnection.Disconnect();
            CurrentGroup = null;
            role = Role.Notconnected;
        }

        private static void DisconnectServer()
        {
            //Send client commands to disconnect
            ServerController.StopServer();
            CurrentGroup = null;
            role = Role.Notconnected;
        }

        public static void OnUserJoin(User user)
        {
            // Update chat?
        }

        public static void OnUserLeave(User user)
        {
            // Update chat?
        }

        public static Group CurrentGroup
        {
            get
            {
                return currentGroup;
            }
            set
            {
                currentGroup = value;
            }
        }

        public static User CurrentUser
        {
            get
            {
                return currentUser;
            }
            set
            {
                currentUser = value;
            }
        }


        public static Role CurrentRole
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
        }

        public static Presentation CurrentPresentation
        {
            get
            {
                return currentPresentation;
            }
            set
            {
                currentPresentation = value;
            }
        }

        public enum Role
        {
            Host,
            Client,
            Notconnected
        }

        public enum ConnectionResult
        {
            Success,
            WrongPassword,
            Error
        }

    }
}
