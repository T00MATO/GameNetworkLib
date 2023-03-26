namespace GNServerLib.Room
{
    internal class RoomProccess
    {
        public RoomInstance Room;
        public RoomMessage Message;

        public RoomProccess(RoomInstance room, RoomMessage message)
        {
            Room = room;
            Message = message;
        }
    }
}
