﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ShareP
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="User", Namespace="http://schemas.datacontract.org/2004/07/ShareP")]
    public partial class User : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string IPField;
        
        private int IdField;
        
        private string UsernameField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string IP
        {
            get
            {
                return this.IPField;
            }
            set
            {
                this.IPField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Id
        {
            get
            {
                return this.IdField;
            }
            set
            {
                this.IdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Username
        {
            get
            {
                return this.UsernameField;
            }
            set
            {
                this.UsernameField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Message", Namespace="http://schemas.datacontract.org/2004/07/ShareP")]
    public partial class Message : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string SenderField;
        
        private string TextField;
        
        private System.DateTime TimeField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Sender
        {
            get
            {
                return this.SenderField;
            }
            set
            {
                this.SenderField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Text
        {
            get
            {
                return this.TextField;
            }
            set
            {
                this.TextField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime Time
        {
            get
            {
                return this.TimeField;
            }
            set
            {
                this.TimeField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Presentation", Namespace="http://schemas.datacontract.org/2004/07/ShareP")]
    public partial class Presentation : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string AuthorField;
        
        private int CurrentSlideField;
        
        private string NameField;
        
        private int SlidesTotalField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Author
        {
            get
            {
                return this.AuthorField;
            }
            set
            {
                this.AuthorField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int CurrentSlide
        {
            get
            {
                return this.CurrentSlideField;
            }
            set
            {
                this.CurrentSlideField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name
        {
            get
            {
                return this.NameField;
            }
            set
            {
                this.NameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int SlidesTotal
        {
            get
            {
                return this.SlidesTotalField;
            }
            set
            {
                this.SlidesTotalField = value;
            }
        }
    }
}
namespace ShareP.Server
{
    using System.Runtime.Serialization;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ConnectionResult", Namespace="http://schemas.datacontract.org/2004/07/ShareP.Server")]
    public enum ConnectionResult : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Success = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        WrongPassword = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Error = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        UsernameExists = 3,
    }
}


[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace="http://ShareP", ConfigurationName="IShareP", CallbackContract=typeof(ISharePCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
public interface IShareP
{
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/Connect", ReplyAction="http://ShareP/IShareP/ConnectResponse")]
    ShareP.Server.ConnectionResult Connect(ShareP.User user, byte[] password);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/Connect", ReplyAction="http://ShareP/IShareP/ConnectResponse")]
    System.Threading.Tasks.Task<ShareP.Server.ConnectionResult> ConnectAsync(ShareP.User user, byte[] password);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/Reconnect", ReplyAction="http://ShareP/IShareP/ReconnectResponse")]
    ShareP.Server.ConnectionResult Reconnect(ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/Reconnect", ReplyAction="http://ShareP/IShareP/ReconnectResponse")]
    System.Threading.Tasks.Task<ShareP.Server.ConnectionResult> ReconnectAsync(ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/Say")]
    void Say(ShareP.Message msg);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/Say")]
    System.Threading.Tasks.Task SayAsync(ShareP.Message msg);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/IsWriting")]
    void IsWriting(ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/IsWriting")]
    System.Threading.Tasks.Task IsWritingAsync(ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsTerminating=true, Action="http://ShareP/IShareP/Disconnect")]
    void Disconnect(ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsTerminating=true, Action="http://ShareP/IShareP/Disconnect")]
    System.Threading.Tasks.Task DisconnectAsync(ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestServerInfo", ReplyAction="http://ShareP/IShareP/RequestServerInfoResponse")]
    System.Collections.Generic.Dictionary<string, string> RequestServerInfo();
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestServerInfo", ReplyAction="http://ShareP/IShareP/RequestServerInfoResponse")]
    System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, string>> RequestServerInfoAsync();
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestSlide", ReplyAction="http://ShareP/IShareP/RequestSlideResponse")]
    System.IO.Stream RequestSlide(int slide);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestSlide", ReplyAction="http://ShareP/IShareP/RequestSlideResponse")]
    System.Threading.Tasks.Task<System.IO.Stream> RequestSlideAsync(int slide);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestCurrentPresentation", ReplyAction="http://ShareP/IShareP/RequestCurrentPresentationResponse")]
    ShareP.Presentation RequestCurrentPresentation();
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestCurrentPresentation", ReplyAction="http://ShareP/IShareP/RequestCurrentPresentationResponse")]
    System.Threading.Tasks.Task<ShareP.Presentation> RequestCurrentPresentationAsync();
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/ViewerChangeFocus")]
    void ViewerChangeFocus(bool focus, ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/ViewerChangeFocus")]
    System.Threading.Tasks.Task ViewerChangeFocusAsync(bool focus, ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestUsersList", ReplyAction="http://ShareP/IShareP/RequestUsersListResponse")]
    ShareP.User[] RequestUsersList();
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestUsersList", ReplyAction="http://ShareP/IShareP/RequestUsersListResponse")]
    System.Threading.Tasks.Task<ShareP.User[]> RequestUsersListAsync();
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestPresentationStart", ReplyAction="http://ShareP/IShareP/RequestPresentationStartResponse")]
    bool RequestPresentationStart();
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/RequestPresentationStart", ReplyAction="http://ShareP/IShareP/RequestPresentationStartResponse")]
    System.Threading.Tasks.Task<bool> RequestPresentationStartAsync();
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/ClPresentationStarted")]
    void ClPresentationStarted(ShareP.Presentation presentation, ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/ClPresentationStarted")]
    System.Threading.Tasks.Task ClPresentationStartedAsync(ShareP.Presentation presentation, ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/ClPresentationNextSlide")]
    void ClPresentationNextSlide(int slide);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/ClPresentationNextSlide")]
    System.Threading.Tasks.Task ClPresentationNextSlideAsync(int slide);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/ClPresentationEnd")]
    void ClPresentationEnd();
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/ClPresentationEnd")]
    System.Threading.Tasks.Task ClPresentationEndAsync();
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface ISharePCallback
{
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/RefreshUsers")]
    void RefreshUsers(ShareP.User[] users);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/Receive")]
    void Receive(ShareP.Message msg);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/IsWritingCallback")]
    void IsWritingCallback(ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/UserJoin")]
    void UserJoin(ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/UserLeave")]
    void UserLeave(ShareP.User user);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/KickUser")]
    void KickUser();
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/GroupSettingsChanged")]
    void GroupSettingsChanged(System.Collections.Generic.Dictionary<string, string> newSettings);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/PresentationStarted")]
    void PresentationStarted(ShareP.Presentation presentation);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/PresentationNextSlide")]
    void PresentationNextSlide(int slide);
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/PresentationEnd")]
    void PresentationEnd();
    
    [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://ShareP/IShareP/GroupClose")]
    void GroupClose();
    
    [System.ServiceModel.OperationContractAttribute(Action="http://ShareP/IShareP/ClRequestSlide", ReplyAction="http://ShareP/IShareP/ClRequestSlideResponse")]
    byte[] ClRequestSlide(int slide);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface ISharePChannel : IShareP, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class SharePClient : System.ServiceModel.DuplexClientBase<IShareP>, IShareP
{
    
    public SharePClient(System.ServiceModel.InstanceContext callbackInstance) : 
            base(callbackInstance)
    {
    }
    
    public SharePClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
            base(callbackInstance, endpointConfigurationName)
    {
    }
    
    public SharePClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
            base(callbackInstance, endpointConfigurationName, remoteAddress)
    {
    }
    
    public SharePClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(callbackInstance, endpointConfigurationName, remoteAddress)
    {
    }
    
    public SharePClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(callbackInstance, binding, remoteAddress)
    {
    }
    
    public ShareP.Server.ConnectionResult Connect(ShareP.User user, byte[] password)
    {
        return base.Channel.Connect(user, password);
    }
    
    public System.Threading.Tasks.Task<ShareP.Server.ConnectionResult> ConnectAsync(ShareP.User user, byte[] password)
    {
        return base.Channel.ConnectAsync(user, password);
    }
    
    public ShareP.Server.ConnectionResult Reconnect(ShareP.User user)
    {
        return base.Channel.Reconnect(user);
    }
    
    public System.Threading.Tasks.Task<ShareP.Server.ConnectionResult> ReconnectAsync(ShareP.User user)
    {
        return base.Channel.ReconnectAsync(user);
    }
    
    public void Say(ShareP.Message msg)
    {
        base.Channel.Say(msg);
    }
    
    public System.Threading.Tasks.Task SayAsync(ShareP.Message msg)
    {
        return base.Channel.SayAsync(msg);
    }
    
    public void IsWriting(ShareP.User user)
    {
        base.Channel.IsWriting(user);
    }
    
    public System.Threading.Tasks.Task IsWritingAsync(ShareP.User user)
    {
        return base.Channel.IsWritingAsync(user);
    }
    
    public void Disconnect(ShareP.User user)
    {
        base.Channel.Disconnect(user);
    }
    
    public System.Threading.Tasks.Task DisconnectAsync(ShareP.User user)
    {
        return base.Channel.DisconnectAsync(user);
    }
    
    public System.Collections.Generic.Dictionary<string, string> RequestServerInfo()
    {
        return base.Channel.RequestServerInfo();
    }
    
    public System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, string>> RequestServerInfoAsync()
    {
        return base.Channel.RequestServerInfoAsync();
    }
    
    public System.IO.Stream RequestSlide(int slide)
    {
        return base.Channel.RequestSlide(slide);
    }
    
    public System.Threading.Tasks.Task<System.IO.Stream> RequestSlideAsync(int slide)
    {
        return base.Channel.RequestSlideAsync(slide);
    }
    
    public ShareP.Presentation RequestCurrentPresentation()
    {
        return base.Channel.RequestCurrentPresentation();
    }
    
    public System.Threading.Tasks.Task<ShareP.Presentation> RequestCurrentPresentationAsync()
    {
        return base.Channel.RequestCurrentPresentationAsync();
    }
    
    public void ViewerChangeFocus(bool focus, ShareP.User user)
    {
        base.Channel.ViewerChangeFocus(focus, user);
    }
    
    public System.Threading.Tasks.Task ViewerChangeFocusAsync(bool focus, ShareP.User user)
    {
        return base.Channel.ViewerChangeFocusAsync(focus, user);
    }
    
    public ShareP.User[] RequestUsersList()
    {
        return base.Channel.RequestUsersList();
    }
    
    public System.Threading.Tasks.Task<ShareP.User[]> RequestUsersListAsync()
    {
        return base.Channel.RequestUsersListAsync();
    }
    
    public bool RequestPresentationStart()
    {
        return base.Channel.RequestPresentationStart();
    }
    
    public System.Threading.Tasks.Task<bool> RequestPresentationStartAsync()
    {
        return base.Channel.RequestPresentationStartAsync();
    }
    
    public void ClPresentationStarted(ShareP.Presentation presentation, ShareP.User user)
    {
        base.Channel.ClPresentationStarted(presentation, user);
    }
    
    public System.Threading.Tasks.Task ClPresentationStartedAsync(ShareP.Presentation presentation, ShareP.User user)
    {
        return base.Channel.ClPresentationStartedAsync(presentation, user);
    }
    
    public void ClPresentationNextSlide(int slide)
    {
        base.Channel.ClPresentationNextSlide(slide);
    }
    
    public System.Threading.Tasks.Task ClPresentationNextSlideAsync(int slide)
    {
        return base.Channel.ClPresentationNextSlideAsync(slide);
    }
    
    public void ClPresentationEnd()
    {
        base.Channel.ClPresentationEnd();
    }
    
    public System.Threading.Tasks.Task ClPresentationEndAsync()
    {
        return base.Channel.ClPresentationEndAsync();
    }
}
