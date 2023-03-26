using GNServerLib;

namespace GNServerCore
{
    public class Program
    {
        private static readonly ushort SERVER_PORT = 13000;

        public static void Main(string[] args)
        {
            var launcher = new ServerLauncher(SERVER_PORT);
            launcher.Start();
        }
    }
}
