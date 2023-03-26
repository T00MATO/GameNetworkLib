using System;

namespace GNPacketLib
{
    [Serializable]
    public class GNP_Connect : GNPacket
    {
    }

    [Serializable]
    public class GNP_Disconnect : GNPacket
    {
    }

    [Serializable]
    public class GNP_Login : GNPacket
    {
        public string Username;

        public GNP_Login(string username)
        {
            Username = username;
        }
    }

    [Serializable]
    public class GNP_LoginRes : GNPacket
    {
        public enum RESULTS : byte
        {
            NONE,
            SUCCESS,
            FAILURE,
        }

        public RESULTS Result;

        public ulong Uid;
        public string Username;

        public GNP_LoginRes(RESULTS result, ulong uid, string username)
        {
            Result = result;
            Uid = uid;
            Username = username;
        }
    }

    [Serializable]
    public class GNP_Match : GNPacket
    {
        public enum REQUESTS : byte
        {
            NONE,
            START,
            CANCEL,
            SUCCESS,
        }

        public REQUESTS Request;

        public GNP_Match(REQUESTS request)
        {
            Request = request;
        }
    }

    [Serializable]
    public class GNP_MatchRes : GNPacket
    {
        public enum RESULTS : byte
        {
            NONE,
            START,
            CANCEL,
            SUCCESS,
        }

        public RESULTS Result;

        public GNP_MatchRes(RESULTS result)
        {
            Result = result;
        }
    }

    [Serializable]
    public class GNP_RoomCreate : GNPacket
    {
    }

    [Serializable]
    public class GNP_UserJoin : GNPacket
    {
    }

    [Serializable]
    public class GNP_UserJoinRes : GNPacket
    {
        public enum RESULTS : byte
        {
            NONE,
            SUCCESS,
        }

        public RESULTS Result;

        public GNP_UserJoinRes(RESULTS result)
        {
            Result = result;
        }
    }

    [Serializable]
    public class GNP_UserExit : GNPacket
    {
    }

    [Serializable]
    public class GNP_UserExitRes : GNPacket
    {
        public enum RESULTS : byte
        {
            NONE,
            SUCCESS,
        }

        public RESULTS Result;

        public GNP_UserExitRes(RESULTS result)
        {
            Result = result;
        }
    }

    [Serializable]
    public class GNP_RoomInfo : GNPacket
    {
        public ulong[] Uids;
        public string[] Usernames;

        public GNP_RoomInfo(int userCount)
        {
            Uids = new ulong[userCount];
            Usernames = new string[userCount];
        }
    }
}
