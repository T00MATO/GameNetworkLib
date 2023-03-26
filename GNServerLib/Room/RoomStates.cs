namespace GNServerLib.Room
{
    public enum RoomStates : byte
    {
        None,
        Creating,
        Created,
        Destroying,
        Destroyed,
    }

    public enum GameStates : byte
    {
        None,
        Prepare,
        Start,
    }
}
