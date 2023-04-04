# GameNetworkLib

[C#](https://learn.microsoft.com/ko-kr/dotnet/csharp/tour-of-csharp/)에 기반한 게임 개발용 라이브러리입니다.

해당 라이브러리를 활용하여 만든 클라이언트 어플리케이션은 **[GameNetworkApplication](https://github.com/T00MATO/GameNetworkApplication)** 를 참고해주세요.

# [GNCore](https://github.com/T00MATO/GameNetworkLib/tree/master/GNCore)

**GNServerLib** 의 [ServerLauncher](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/ServerLauncher.cs)를 구동시키는 솔루션입니다.

```csharp
//  Program.cs -> line: 0

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
//  Server.cs -> line: 41

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
//  GameManager.cs -> line: 23

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

[RoomManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomManager.cs)의 
[RoomMessage](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomMessage/RoomMessage.cs) 처리 작업(Work)은 
별도의 워크 스레드(Work Thread)에서 처리하며,

그 외의 작업들은 메인 스레드(Main Thread)에서 GameManager의 하위 Manager들이 처리합니다.

## [UserManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserManager.cs)

UserManager가 관리하는 [UserSocket](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserConnection/UserSocket.cs)객체는 
Server로부터 클라이언트 소켓을 받아 생성됩니다.

```csharp
//  UserSocket.cs -> line: 17

public UserSocket(Socket socket)
{
    _socket = socket;
    _recvBuffer = new byte[GNPacket.RECV_BUFFER_SIZE];

    _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceivedData, null);
}
```

UserSocekt객체는 생성될때부터 패킷 받기를 시작합니다.

```csharp
//  UserSocket.cs -> line: 37

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

클라이언트로부터 받은 패킷은 
[UserConnection](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserConnection/UserConnection.cs)에게 넘기고

```csharp
//  UserConnection.cs -> line: 40

public void EnqueuePacket(GNPacket packet)
{
    _userManager.EnqueueProc(this, packet);
}

//  UserManager.cs -> line: 41

public void EnqueueProc(UserConnection conn, GNPacket packet)
{
    lock (_procQueue)
    {
        var process = new UserProccess
        {
            Connection = conn,
            Packet = packet,
        };
        _procQueue.Enqueue(process);
    }
}
```

UserConnection은 넘겨받은 패킷의 처리(Process)를 [UserManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserManager.cs)의 
프로세스 큐(Process Queue)에 추가합니다.

```csharp
//  UserManager.cs -> line: 54

public void HandlePacket()
{
    try
    {
        lock (_procQueue)
        {
            if (_procQueue.Count > 0)
            {
                var process = _procQueue.Dequeue();
                process.Connection.ProcPacket(process.Packet);
            }
        }
    }
    catch (Exception exception)
    {
        _logger.Error(exception);
    }
}
```

프로세스 큐는 메인 스레드에서 GameManager가 **HandlePacket** 메서드를 호출하여 프로세스를 하나씩 처리하며,

UserConnection의 **ProcPacket** 메서드로 패킷을 처리합니다.

```csharp
//  UserConnection -> line: 45

public void ProcPacket(GNPacket packet)
{
    switch (packet)
    {
        case GNP_Connect p:
            OnConnected(p);
            break;
        case GNP_Disconnect p:
            OnDisconnected(p);
            break;
        case GNP_Login p:
            OnLogin(p);
            break;
        case GNP_Match p:
            OnMatch(p);
            break;
        case GNP_RoomCreate p:
            OnRoomCreate(p);
            break;
        case GNP_UserJoin p:
            OnUserJoin(p);
            break;
        case GNP_UserExit p:
            OnUserExit(p);
            break;
        case GNP_RoomInfo p:
            OnRoomInfo(p);
            break;
        default:
            throw new Exception($"Can not process packet! : {packet}");
    }
}
```

ProcPacket 메서드는 패킷([GNPacket](https://github.com/T00MATO/GameNetworkLib/blob/master/GNPacketLib/GNPacket.cs))의 유형에 따라 각각 다르게 처리합니다.

```csharp
//  UserConnection.cs -> line: 78

public void SendPacket(GNPacket packet)
{
    lock (_socket)
        _socket.SendData(packet.ToBytes());
}

//  UserSocket.cs -> line: 64

public void SendData(byte[] dataBytes)
{
    _socket?.BeginSend(dataBytes, 0, dataBytes.Length, SocketFlags.None, OnSendedData, null);
}

private void OnSendedData(IAsyncResult result)
{
    try
    {
        var dataBytes = _socket.EndSend(result, out var error);

        GNPacket.CheckDataBytes(dataBytes, error);
    }
    catch (Exception exception)
    {
        _logger.Error(exception);
    }
}
```

UserConnection의 **SendPacket** 메서드를 통해 연결된 클라이언트에 패킷을 전달할 수 있습니다.

### [UserInfo](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserInfo/UserInfo.cs) 

```csharp
//  UserInfo.cs -> line: 7

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
```

UserInfo는 유저의 데이터를 지니고 있는 구조체입니다.

```csharp
//  UserInfoHandler.cs -> line: 8

public void SetUsername(string name)
{
    if (Username != string.Empty)
        throw new Exception($"{nameof(Username)} is already setted.");

    Username = name;
}

public void StartMatch()
{
    if (Joined)
        throw new Exception("Can not start a match while in the room.");

    Matching = true;
}

public void CancelMatch()
{
    if (Joined)
        throw new Exception("Can not cancel a match while in the room.");

    Matching = false;
}

public void SetRoom(RoomInstance room)
{
    if (CurrentRoom != null)
        throw new Exception($"{nameof(CurrentRoom)} is already setted.");

    if (Joined)
        throw new Exception("Can not set a room when user is already joined a room.");

    CurrentRoom = room;
}

...
```

외부에서는 읽기만 가능하며, 내부 데이터를 수정하려면 
[UserInfoHandler](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserInfo/UserInfoHandler.cs)의 처리 메서드를 호출해야 합니다.

UserInfoHandler의 처리 메서드들은 데이터를 갱신할 때 유저의 상태들을 고려하여 갱신합니다.

## [MatchManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Match/MatchManager.cs)

```csharp
//  UserHandler.cs -> line: 41

private void OnMatch(GNP_Match p)
{
    switch (p.Request)
    {
        case GNP_Match.REQUESTS.START:
        {
            Info.StartMatch();
            _matchManager.AddConnection(this);
            _logger.Info($"{Info.Username} started to wait for a match making.");
        }
        break;
        case GNP_Match.REQUESTS.CANCEL:
        {
            Info.CancelMatch();
            _matchManager.RemoveConnection(Uid);
            _logger.Info($"{Info.Username} canceled to wait for a match making.");
        }
        break;
        case GNP_Match.REQUESTS.SUCCESS:
        {
            _logger.Info($"{Info.Username} has been succeed match making.");
        }
        break;
        default:
        {
            throw new Exception($"Can not process request! : {p.Request}");
        }
    }

    var requestIdx = (byte)p.Request;
    var result = new GNP_MatchRes((GNP_MatchRes.RESULTS)requestIdx);

    SendPacket(result);
}

//  MatchManager.cs -> line: 23

public void AddConnection(UserConnection conn)
{
    lock (_connections)
        _connections.Add(conn);
}

public void RemoveConnection(ulong uid)
{
    lock (_connections)
    {
        foreach (var conn in _connections)
        {
            if (conn.Uid == uid)
            {
                _connections.Remove(conn);
                return;
            }
        }
    }
}
```

[UserHandler](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/User/UserConnection/UserHandlers.cs)의 **OnMatch** 메서드는 
클라이언트로부터 **GNP_Match** 패킷([GNPacket](https://github.com/T00MATO/GameNetworkLib/blob/master/GNPacketLib/GNPacket.cs))을 받아 처리하는 메서드입니다.

각각 GNP_Match 패킷의 Request에 따라 다음과 같이 처리합니다.

- **GNP_Match.REQUESTS.START(매치 시작 시):** 해당 유저의 UserConnection을 MatchManager의 대기 리스트에 추가합니다.

- **GNP_Match.REQUESTS.CANCEL(매치 취소 시):** 해당 유저의 UserConnection을 MatchManager의 대기 리스트에서 제거합니다.

**GNP_Match.REQUESTS.SUCCESS(매치 성공 시)** 패킷은 클라이언트가 아닌 MatchManager 측에서 UserConnection에게 알립니다.

각각의 Request에 따라 패킷을 처리한 후, GNP_MatchRes 패킷을 클라이언트에게 보내 처리하도록 합니다.

```csharp
//  MatchManager.cs -> line: 44

public void HandleMatch()
{
    try
    {
        lock (_connections)
        {
            if (_connections.Count > 1)
                MatchConnections();
        }
    }
    catch (Exception exception)
    {
        _logger.Error(exception);
    }
}

private void MatchConnections()
{
    var res = new GNP_Match(GNP_Match.REQUESTS.SUCCESS);

    var conns = new Dictionary<ulong, UserConnection>();
    for (var idx = 0; idx < RoomInfo.MAX_USER; idx++)
    {
        var conn = _connections[0];
        conn.EnqueuePacket(res);

        _connections.Remove(conn);
        conns.Add(conn.Uid, conn);
    }

    roomManager.CreateRoom(conns);
}
```

UserConnection 대기 리스트는 메인 스레드에서 GameManager가 **HandleMatch** 메서드를 호출하여 매치를 하나씩 처리하며,

매치메이킹이 될때마다 Request가 **GNP_Match.REQUESTS.SUCCESS** 인 **GNP_Match** 패킷을 매치메이킹된 유저들에게 보냅니다.

그 후, 매치 메이킹이 된 유저들을 바탕으로 [RoomManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomManager.cs)에서 방을 생성합니다.

## [RoomManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomManager.cs)

```csharp
//  RoomManager.cs -> line: 58

public void CreateRoom(Dictionary<ulong, UserConnection> conns)
{
    lock (_rooms)
    {
        var room = new RoomInstance(conns, gameManager, _roomIdSeq++);
        _rooms.Add(room);
    }
}

//  RoomInstance.cs -> line: 24

public RoomInstance(Dictionary<ulong, UserConnection> conns, GameManager gameManager, ulong roomId)
{
    _gameManager = gameManager;

    RoomId = roomId;
    RoomIdTag = roomId.ToString("D8");

    Info = RoomInfo.Create(conns);

    EnqueueMessage(new RM_Create());
}
```

UserManager는 [MatchManager](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Match/MatchManager.cs)에서 매치메이킹에 성공한 유저들 받아와
[RoomInstance](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomInstance/RoomInstance.cs) 객체를 생성합니다.

그 후, 생성된 RoomInstance는 RoomManager의 방 목록에 추가됩니다.

RoomInstance는 생성될 때 RoomManager에게 
**RM_Create(방 생성)** 메세지([RoomMessage](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomMessage/RoomMessage.cs))를 보냅니다.

```csharp
//  RoomInstance.cs -> line: 36

public void EnqueueMessage(RoomMessage message)
{
    _roomManager.EnqueueProc(this, message);
}

//  RoomManager.cs -> line: 56

public void EnqueueProc(RoomInstance room, RoomMessage message)
{
    lock (_procQueue)
    {
        var process = new RoomProccess(room, message);
        _procQueue.Enqueue(process);
    }
}
```

보내진 방 메세지는 RoomManager의 프로세스 대기 큐(Process Queue)에 추가됩니다.

```csharp
//  RoomManager.cs -> line: 65

public void HandleMessage()
{
    try
    {
        lock (_procQueue)
        {
            if (_procQueue.Count > 0)
            {
                var process = _procQueue.Dequeue();
                var subRoutine = process.Room.HandleMessage(process.Message);
                var procWork = new RoomWork(process.Room, subRoutine);
                LoadBalanceWork(procWork);
            }
        }
    }
    catch (Exception exception)
    {
        _logger.Error(exception);
    }
}

//  RoomInstance.cs -> line: 41

public IEnumerator HandleMessage(RoomMessage message)
{
    if (message is RM_Create create)
        return OnCreate(create);

    if (Info.RoomState == RoomStates.Creating)
        throw new Exception("Can not handle a message while room is creating.");

    switch (message)
    {
        case RM_Destroy m:
            return OnDestroy(m);
        case RM_UserJoin m:
            return OnUserJoin(m);
        case RM_UserExit m:
            return OnUserExit(m);
        case RM_UpdateStatus m:
            return OnStatement(m);
        default:
            throw new Exception($"Can not handle a message! : {message}");
    }
}
```

RoomManager의 프로세스 대기 큐는 메인 스레드에서 GameManager가 **HandleMessage** 메서드를 호출하여 메세지를 하나씩 처리합니다.

프로세스([RoomProcess](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomProcess.cs))는 
RoomInstance의 처리 메서드([SubRoutine](https://ko.wikipedia.org/wiki/%ED%95%A8%EC%88%98_(%EC%BB%B4%ED%93%A8%ED%84%B0_%EA%B3%BC%ED%95%99)))를 받아와 
[RoomWork](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomWork.cs)(방 처리 작업) 생성합니다.

생성한 RoomWork는 [로드밸런싱](https://ko.wikipedia.org/wiki/%EB%B6%80%ED%95%98%EB%B6%84%EC%82%B0)된 후, 방 메세지 작업 풀(Work Pool)에 추가됩니다.

```csharp
//  RoomManager.cs -> line: 11

private static readonly int MAX_THREADS = 4;

//  RoomManager.cs -> line: 22

public RoomManager(GameManager gameManager) : base(gameManager)
{
    _rooms = new List<RoomInstance>();

    _procQueue = new Queue<RoomProccess>();
    _workPools = new List<List<RoomWork>>();
    _workers = new List<Thread>();
    for (var idx = 0; idx < MAX_THREADS; idx++)
    {
        _workPools.Add(new List<RoomWork>());

        var threadIdx = idx;
        var thread = new Thread(() => HandleWork(threadIdx));
        _workers.Add(thread);
    }

    _logger.Info("Successfully initialized.");
}

//  RoomManager.cs -> line: 111

public void RunWorkers()
{
    foreach (var thread in _workers)
        thread.Start();
}

private void HandleWork(int threadIdx)
{
    while (true)
    {
        lock (_workPools[threadIdx])
        {
            for (var idx = 0; idx < _workPools[threadIdx].Count;)
            {
                var work = _workPools[threadIdx][idx];

                try
                {
                    if (!IsWorkDone(work))
                    {
                        idx++;
                        continue;
                    }
                }
                catch (Exception exception)
                {
                    _logger.Error(exception);
                }

                _workPools[threadIdx].Remove(work);
            }
        }
    }
}

private bool IsWorkDone(RoomWork work)
{
    if (work.Room.Info.RoomState != RoomStates.Destroyed)
    {
        if (work.Subroutine.Current is IRMCondition condition && !condition.IsFinished())
            return false;

        if (work.Subroutine.MoveNext() && work.Subroutine.Current != null)
            return false;
    }

    return true;
}
```

RoomManager의 작업 풀(Work Pool)은 총 4개가 존재하고 그 작업 풀들 각각 처리하는 4개의 워커 스레드(Worker Thread)들이 있습니다.

RoomManager의 워커 스레드들은 GameManager가 **RunWorkers** 메서드를 호출하여 스레드들의 메세지 처리 업무를 진행합니다.

각각 워커 스레드에 할당되어 있는 워커(Worker)들은 해당 작업 풀에 있는 모든 서브루틴(메세지 처리 메서드)의 진행도를 확인합니다.

서브루틴의 진행이 완료되었다면 해당 메세지 처리 작업을 작업 풀에서 제거합니다.

완료하지 못했을 경우 [IRMCondition](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomMessage/RMConditions.cs)의 
조건에 따라 서브루틴은 대기합니다.

### [RoomInfo](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomInfo/RoomInfo.cs)

```csharp
//  RoomInfo.cs -> line: 8

public static readonly int MAX_USER = 2;

private Dictionary<ulong, UserConnection> _connections;

public RoomStates RoomState { get; private set; }
public GameStates GameState { get; private set; }
public bool HandlingStatement { get; private set; }

public static RoomInfo Create(Dictionary<ulong, UserConnection> conns)
{
    return new RoomInfo
    {
        _connections = conns,
        RoomState = RoomStates.Creating,
        GameState = GameStates.None,
        HandlingStatement = false,
    };
}
```

RoomInfo느 방의 데이터를 지니고 있는 구조체입니다.

```csharp
//  RoomInfoHandler.cs -> line: 9

public void BroadcastPacket(GNPacket packet)
{
    foreach (var conn in _connections.Values)
        conn.EnqueuePacket(packet);
}

public void BroadcastPacket(GNPacket packet, Func<UserInfo, bool> condition)
{
    foreach (var conn in _connections.Values)
    {
        if (condition(conn.Info))
            conn.EnqueuePacket(packet);
    }
}

public void Created(RoomInstance room)
{
    foreach (var conn in _connections.Values)
        conn.Info.SetRoom(room);

    RoomState = RoomStates.Created;
}
...
```

외부에서는 읽기만 가능하며, 내부 데이터를 수정하려면 
[RoomInfoHandler](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Room/RoomInfo/RoomInfoHandler.cs)의 처리 메서드를 호출해야 합니다.

RoomInfoHandler의 처리 메서드들은 데이터를 갱신할 때 방의 상태들을 고려하여 갱신합니다.

# [GNClientLib](https://github.com/T00MATO/GameNetworkLib/tree/master/GNClientLib)

게임 서버와 통신하는 클라이언트의 라이브러리입니다.

# [GNPacketLib](https://github.com/T00MATO/GameNetworkLib/tree/master/GNPacketLib)

게임 서버와 클라이언트가 통신하는 패킷 라이브러리입니다.

```csharp
//  GNPacket.cs -> line: 11

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
```

[GNPacket](https://github.com/T00MATO/GameNetworkLib/blob/master/GNPacketLib/GNPacket.cs)은 모든 패킷들의 토대가 되는 추상 클래스입니다.

**ToBytes** 메서드로 패킷 객체를 바이너리로, FromBytes 메서드로 바이너리를 패킷 객체로 변환합니다.

```csharp
//  GNPs.cs -> 

[Serializable]
public class GNP_Connect : GNPacket
{
}

[Serializable]
public class GNP_Disconnect : GNPacket
{
}

[Serializable]
public class GNP_Login : GNPacket
{
    public string Username;

    public GNP_Login(string username)
    {
        Username = username;
    }
}

[Serializable]
public class GNP_LoginRes : GNPacket
{
    public enum RESULTS : byte
    {
        NONE,
        SUCCESS,
        FAILURE,
    }

    public RESULTS Result;

    public ulong Uid;
    public string Username;

    public GNP_LoginRes(RESULTS result, ulong uid, string username)
    {
        Result = result;
        Uid = uid;
        Username = username;
    }
}
```

GNPacket을 상속받는 패킷들은 다음과 같은 형태로 구성합니다.
