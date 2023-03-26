using GNServerLib.Match;
using GNServerLib.Room;
using GNServerLib.User;

namespace GNServerLib
{
    internal abstract class SubManager
    {
        protected GameManager gameManager { get; private set; }
        protected UserManager userManager => gameManager.UserManager;
        protected MatchManager matchManager => gameManager.MatchManager;
        protected RoomManager roomManager => gameManager.RoomManager;

        public SubManager(GameManager gameManager) 
        {
            this.gameManager = gameManager;
        }
    }
}
