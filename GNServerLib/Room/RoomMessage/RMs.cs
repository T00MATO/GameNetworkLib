namespace GNServerLib.Room
{
    internal class RM_Create : RoomMessage
    {
    }

    internal class RM_Destroy : RoomMessage
    {
    }

    internal class RM_UserJoin : RoomMessage
    {
        public ulong Uid;
        public string Username;

        public RM_UserJoin(ulong uid, string username)
        {
            Uid = uid;
            Username = username;
        }
    }

    internal class RM_UserExit : RoomMessage
    {
        public ulong Uid;
        public string Username;

        public RM_UserExit(ulong uid, string username)
        {
            Uid = uid;
            Username = username;
        }
    }

    internal class RM_UpdateStatus : RoomMessage
    {
        public GameStates State;

        public RM_UpdateStatus(GameStates state)
        {
            State = state;
        }
    }
}
