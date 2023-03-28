# GameNetworkLib

게임 개발용 라이브러리입니다.

해당 라이브러리를 활용하여 만든 클라이언트 어플리케이션은 __[GameNetworkApplication](https://github.com/T00MATO/GameNetworkApplication)__ 를 참고해주세요.

# [GNCore](https://github.com/T00MATO/GameNetworkLib/tree/master/GNCore)

__GNServerLib__ 의 __[ServerLauncher](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/ServerLauncher.cs)__ 를 구동시키는 솔루션입니다.



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
__[ServerLauncher](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/ServerLauncher.cs)__ 를 실행하여 게임 서버를 실행할 수 있습니다.

![GNServerLib drawio](https://user-images.githubusercontent.com/127966719/228170165-d1f3ab69-07ed-47b4-a616-69770ade0ee3.png)
*GNServerLib의 구조*

## [Server](https://github.com/T00MATO/GameNetworkLib/blob/master/GNServerLib/Server.cs)
