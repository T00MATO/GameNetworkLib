using GNServerLib.Room;
using System;

namespace GNServerLib.User
{
    internal partial struct UserInfo
    {
        public void SetUsername(string name)
        {
            if (Username != string.Empty)
                throw new Exception($"{nameof(Username)} is already setted.");

            Username = name;
        }

        public void StartMatch()
        {
            if (Joined)
                throw new Exception("Can not start a match while in the room.");

            Matching = true;
        }

        public void CancelMatch()
        {
            if (Joined)
                throw new Exception("Can not cancel a match while in the room.");

            Matching = false;
        }

        public void SetRoom(RoomInstance room)
        {
            if (CurrentRoom != null)
                throw new Exception($"{nameof(CurrentRoom)} is already setted.");

            if (Joined)
                throw new Exception("Can not set a room when user is already joined a room.");

            CurrentRoom = room;
        }

        public void RemoveRoom()
        {
            if (CurrentRoom == null)
                throw new Exception($"{nameof(CurrentRoom)} is already removed.");

            if (Joined)
                throw new Exception("Can not remove a room when user is joined a room.");

            CurrentRoom = null;
        }

        public void JoinRoom()
        {
            if (CurrentRoom == null)
                throw new Exception($"Can not join a room when {nameof(CurrentRoom)} is null.");
            
            if (Joined)
                throw new Exception("User is already joined a room.");
            
            Joined = true;
        }

        public void ExitRoom()
        {
            if (CurrentRoom == null)
                throw new Exception($"Can not exit a room when {nameof(CurrentRoom)} is null.");

            if (!Joined)
                throw new Exception("User is already exited a room.");

            Joined = false;
        }
    }
}
