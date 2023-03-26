using System.Collections;

namespace GNServerLib.Room
{
    internal class RoomWork
    {
        public RoomInstance Room;
        public IEnumerator Subroutine;

        public RoomWork(RoomInstance room, IEnumerator subroutine)
        {
            Room = room;
            Subroutine = subroutine;
        }
    }
}
