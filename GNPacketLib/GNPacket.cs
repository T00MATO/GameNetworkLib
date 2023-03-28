using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace GNPacketLib
{
    [Serializable]
    public abstract class GNPacket
    {
        public static readonly int SEND_BUFFER_SIZE = (int)SocketOptionName.SendBuffer;
        public static readonly int RECV_BUFFER_SIZE = (int)SocketOptionName.ReceiveBuffer;

        public static readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public static void CheckDataBytes(int bytesLength, SocketError error)
        {
            if (bytesLength == 0 || error != SocketError.Success)
                throw new SocketException();
        }

        public byte[] ToBytes() => ToBytes(this);

        public static byte[] ToBytes(GNPacket packet)
        {
            using (var stream = new MemoryStream())
            {
                _binaryFormatter.Serialize(stream, packet);
                return stream.ToArray();
            }
        }

        public static GNPacket FromBytes(byte[] dataBytes, int bytesLength)
        {
            using (var stream = new MemoryStream(dataBytes, 0, bytesLength))
            {
                return (GNPacket)_binaryFormatter.Deserialize(stream);
            }
        }
    }
}
