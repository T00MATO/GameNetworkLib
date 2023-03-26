using log4net;
using GNServerLib.Match;
using GNServerLib.Room;
using GNServerLib.User;

namespace GNServerLib
{
    internal class GameManager
    {
        private ILog _logger = LogManager.GetLogger(nameof(GameManager));

        public UserManager UserManager { get; private set; }
        public MatchManager MatchManager { get; private set; }
        public RoomManager RoomManager { get; private set; }

        public GameManager()
        {
            UserManager = new UserManager(this);
            MatchManager = new MatchManager(this);
            RoomManager = new RoomManager(this);
        }

        public void Run()
        {
            RoomManager.RunWorkers();

            RunMainThread();
        }

        private void RunMainThread()
        {
            while (true)
            {
                UserManager.HandlePacket();
                MatchManager.HandleMatch();
                RoomManager.HandleMessage();
            }
        }
    }
}
