using GNServerLib.Room;

namespace GNServerLib.User
{
    internal partial struct UserInfo
    {
        public string Username { get; private set; }
        public bool Matching { get; private set; }
        public bool Joined { get; private set; }
        public RoomInstance CurrentRoom { get; private set; }

        public static UserInfo Create()
        {
            return new UserInfo
            {
                Username = string.Empty,
                Matching = false,
                Joined = false,
                CurrentRoom = null,
            };
        }
    }
}
