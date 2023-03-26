using GNPacketLib;
using GNServerLib.User;
using System;

namespace GNServerLib.Room
{
    internal partial struct RoomInfo
    {
        public void BroadcastPacket(GNPacket packet)
        {
            foreach (var conn in _connections.Values)
                conn.EnqueuePacket(packet);
        }

        public void BroadcastPacket(GNPacket packet, Func<UserInfo, bool> condition)
        {
            foreach (var conn in _connections.Values)
            {
                if (condition(conn.Info))
                    conn.EnqueuePacket(packet);
            }
        }

        public void Created(RoomInstance room)
        {
            foreach (var conn in _connections.Values)
                conn.Info.SetRoom(room);

            RoomState = RoomStates.Created;
        }

        public void BeginDestroy()
        {
            if (IsDestroying())
                throw new Exception("Room is already destroying.");

            RoomState = RoomStates.Destroying;
        }

        public void EndDestroy()
        {
            if (RoomState == RoomStates.Destroyed)
                throw new Exception("Room is already destroyed.");

            foreach (var conn in _connections.Values)
            {
                conn.Info.RemoveRoom();
            }
            _connections.Clear();

            RoomState = RoomStates.Destroyed;
        }

        public bool IsDestroying()
        {
            return RoomState == RoomStates.Destroying | RoomState == RoomStates.Destroyed;
        }

        public void UserJoinRoom(ulong uid)
        {
            _connections[uid].Info.JoinRoom();
        }

        public void UserExitRoom(ulong uid)
        {
            _connections[uid].Info.ExitRoom();
        }

        public bool IsAllUserJoined()
        {
            foreach (var conn in _connections.Values)
            {
                if (!conn.Info.Joined)
                    return false;
            }

            return true;
        }

        public bool IsAllUserExited()
        {
            foreach (var conn in _connections.Values)
            {
                if (conn.Info.Joined)
                    return false;
            }

            return true;
        }

        public void BeginHandleState()
        {
            if (HandlingState)
                throw new Exception("Statement handling is already begun.");

            HandlingState = true;
        }

        public void EndHandleState()
        {
            if (!HandlingState)
                throw new Exception("Statement handling is already ended.");

            HandlingState = false;
        }

        public void SetGameState(GameStates state)
        {
            GameState = state;
        }

        public GNP_RoomInfo ToPacket()
        {
            var packet = new GNP_RoomInfo(MAX_USER);

            var userIdx = 0;
            foreach (var conn in _connections.Values)
            {
                packet.Uids[userIdx] = conn.Uid;
                packet.Usernames[userIdx] = conn.Info.Username;
                userIdx++;
            }

            return packet;
        }
    }
}
