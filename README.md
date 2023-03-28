# GameNetworkLib

[C#](https://learn.microsoft.com/ko-kr/dotnet/csharp/tour-of-csharp/)에 기반한 게임 개발용 라이브러리입니다.

해당 라이브러리를 활용하여 만든 클라이언트 어플리케이션은 **[GameNetworkApplication](https://github.com/T00MATO/GameNetworkApplication)** 를 참고해주세요.

# [GNCore](https://github.com/T00MATO/GameNetworkLib/tree/master/GNCore)

**GNServerLib** 의 [ServerLauncher](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/ServerLauncher.cs)를 구동시키는 솔루션입니다.

```csharp
using GNServerLib;

public class Program
{
    private static readonly ushort SERVER_PORT = 13000;

    public static void Main(string[] args)
    {
        var launcher = new ServerLauncher(SERVER_PORT);
        launcher.Start();
    }
}
```

# [GNServerLib](https://github.com/T00MATO/GameNetworkLib/tree/master/GNServerLib)

클라이언트와 통신하여 실제 게임 처리를 담당하는 서버의 라이브러리입니다. 
[ServerLauncher](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/ServerLauncher.cs)를 실행하여 게임 서버를 실행할 수 있습니다.

![GNServerLib drawio](https://user-images.githubusercontent.com/127966719/228170165-d1f3ab69-07ed-47b4-a616-69770ade0ee3.png)
*GNServerLib의 구조*

## [Server](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Server.cs)

```csharp
//  Server.cs

public void StartAcceptSocket()
{
    _logger.Info("Started to accept client sockets.");

    _socket.BeginAccept(OnAcceptedSocket, null);
}

private void OnAcceptedSocket(IAsyncResult result)
{
    try
    {
        var connSocket = _socket.EndAccept(result);
        var userSocket = new UserSocket(connSocket);
        _gameManager.UserManager.CreateConnection(userSocket);
    }
    catch (Exception exception)
    {
        _logger.Error(exception);
    }

    _socket.BeginAccept(OnAcceptedSocket, null);
}
```

클라이언트로부터 연결된 소켓으로 [UserSocket](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserConnection/UserSocket.cs)객체를 생성합니다.

그 후, 생성된 UserSocket을 [UserManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserManager.cs)에게 보내 
[UserConnection](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserConnection/UserConnection.cs)객체를 생성합니다.

## [GameManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/GameManager.cs)

하위 Manager들을 생성하며, Manager들 사이를 연결해주는 [싱글톤](https://ko.wikipedia.org/wiki/%EC%8B%B1%EA%B8%80%ED%84%B4_%ED%8C%A8%ED%84%B4) 객체입니다.

```csharp
//  GameManager.cs

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
```

[RoomManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomManager.cs)의 방 메세지 처리 작업는 별도의 워크 스레드(Work Thread)에서 처리하며,

그 외의 작업들은 메인 스레드(Main Thread)에서 GameManager의 하위 Manager들이 처리합니다.

## [UserManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserManager.cs)

```csharp
//  UserSocket.cs

public UserSocket(Socket socket)
{
    _socket = socket;
    _recvBuffer = new byte[GNPacket.RECV_BUFFER_SIZE];

    _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceivedData, null);
}
```

Server로부터 클라이언트 소켓을 받아 [UserSocket](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserConnection/UserSocket.cs)객체가 생성됩니다.

이렇게 생성된 UserSocekt객체는 [UserManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserManager.cs)에 추가되어 관리됩니다.

```csharp
//  UserSocket.cs

private void OnReceivedData(IAsyncResult result)
{
    try
    {
        var dataBytes = _socket.EndReceive(result, out var error);

        GNPacket.CheckDataBytes(dataBytes, error);

        var packet = GNPacket.FromBytes(_recvBuffer, dataBytes);

        Connection.EnqueuePacket(packet);
    }
    catch (Exception exception)
    {
        _logger.Error(exception);

        if (exception is SocketException)
        {
            Connection.EnqueuePacket(new GNP_Disconnect());
            Dispose();
            return;
        }
    }

    _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceivedData, null);
}
```

UserSocket객체는 클라이언트로부터 패킷을 응답(Response)받아 
[UserConnection](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserConnection/UserConnection.cs)에게 넘기고

```csharp
//  UserConnection.cs

public void EnqueuePacket(GNPacket packet)
{
    _userManager.EnqueueProc(this, packet);
}
```

UserConnection은 넘겨받은 패킷의 처리(Process)를 UserManager의 프로세스 큐(Process Queue)에 추가합니다.
