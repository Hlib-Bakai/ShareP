

namespace ShareP
{
    public enum NotificationType
    {
        Connection, 
        Presentation,
        Chat
    }
    
    public enum GroupNavigation
    {
        FollowOnly,
        Backwards,
        BothDirections
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
