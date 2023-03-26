using log4net;
using GNServerLib.Match;
using GNServerLib.User;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GNServerLib.Room
{
    internal partial class RoomInstance
    {
        private static readonly ILog _logger = LogManager.GetLogger(nameof(RoomInstance));

        private GameManager _gameManager;
        private UserManager _userManager => _gameManager.UserManager;
        private MatchManager _matchManager => _gameManager.MatchManager;
        private RoomManager _roomManager => _gameManager.RoomManager;

        public ulong RoomId { get; private set; }
        public string RoomIdTag { get; private set; }

        public RoomInfo Info;

        public RoomInstance(Dictionary<ulong, UserConnection> conns, GameManager gameManager, ulong roomId)
        {
            _gameManager = gameManager;

            RoomId = roomId;
            RoomIdTag = roomId.ToString("D8");

            Info = RoomInfo.Create(conns);

            EnqueueMessage(new RM_Create());
        }

        public void EnqueueMessage(RoomMessage message)
        {
            _roomManager.EnqueueProc(this, message);
        }

        public IEnumerator HandleMessage(RoomMessage message)
        {
            if (message is RM_Create create)
                return OnCreate(create);

            if (Info.RoomState == RoomStates.Creating)
                throw new Exception("Can not handle a message while room is creating.");

            switch (message)
            {
                case RM_Destroy m:
                    return OnDestroy(m);
                case RM_UserJoin m:
                    return OnUserJoin(m);
                case RM_UserExit m:
                    return OnUserExit(m);
                case RM_UpdateStatus m:
                    return OnStatement(m);
                default:
                    throw new Exception($"Can not handle a message! : {message}");
            }
        }
    }
}
