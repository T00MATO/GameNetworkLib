using log4net;
using GNPacketLib;
using GNServerLib.Match;
using GNServerLib.Room;
using System;

namespace GNServerLib.User
{
    internal partial class UserConnection
    {
        private static readonly ILog _logger = LogManager.GetLogger(nameof(UserConnection));

        private GameManager _gameManager;
        private UserManager _userManager => _gameManager.UserManager;
        private MatchManager _matchManager => _gameManager.MatchManager;
        private RoomManager _roomManager => _gameManager.RoomManager;

        public ulong Uid { get; private set; }
        public string UidTag { get; private set; }

        private UserSocket _socket;

        public UserInfo Info;

        public UserConnection(UserSocket socket, GameManager gameManager, ulong uid)
        {
            _socket = socket;
            _socket.AttachConnection(this);
            
            _gameManager = gameManager;

            Uid = uid;
            UidTag = Uid.ToString("D8");

            Info = UserInfo.Create();

            EnqueuePacket(new GNP_Connect());
        }

        public void EnqueuePacket(GNPacket packet)
        {
            _userManager.EnqueueProc(this, packet);
        }

        public void ProcPacket(GNPacket packet)
        {
            switch (packet)
            {
                case GNP_Connect p:
                    OnConnected(p);
                    break;
                case GNP_Disconnect p:
                    OnDisconnected(p);
                    break;
                case GNP_Login p:
                    OnLogin(p);
                    break;
                case GNP_Match p:
                    OnMatch(p);
                    break;
                case GNP_RoomCreate p:
                    OnRoomCreate(p);
                    break;
                case GNP_UserJoin p:
                    OnUserJoin(p);
                    break;
                case GNP_UserExit p:
                    OnUserExit(p);
                    break;
                case GNP_RoomInfo p:
                    OnRoomInfo(p);
                    break;
                default:
                    throw new Exception($"Can not process packet! : {packet}");
            }
        }

        public void SendPacket(GNPacket packet)
        {
            lock (_socket)
                _socket.SendData(packet.ToBytes());
        }
    }
}
