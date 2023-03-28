namespace GNServerLib
{
    public class ServerLauncher
    {
        private GameManager _gameManager;
        private Server _server;

        public ServerLauncher(ushort port)
        {
            _gameManager = new GameManager();
            _server = new Server(port, _gameManager);
        }

        public void Start()
        {
            _server.StartAcceptSocket();
            _gameManager.Run();
        }
    }
}
