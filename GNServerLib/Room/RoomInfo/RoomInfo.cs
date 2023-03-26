using GNServerLib.User;
using System.Collections.Generic;

namespace GNServerLib.Room
{
    internal partial struct RoomInfo
    {
        public static readonly int MAX_USER = 2;

        private Dictionary<ulong, UserConnection> _connections;

        public RoomStates RoomState { get; private set; }
        public GameStates GameState { get; private set; }
        public bool HandlingState { get; private set; }

        public static RoomInfo Create(Dictionary<ulong, UserConnection> conns)
        {
            return new RoomInfo
            {
                _connections = conns,
                RoomState = RoomStates.Creating,
                GameState = GameStates.None,
                HandlingState = false,
            };
        }
    }
}
