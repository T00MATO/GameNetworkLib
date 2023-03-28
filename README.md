# GameNetworkLib

게임 개발용 라이브러리입니다.

해당 라이브러리를 활용하여 만든 라이브러리는 __[GameNetworkApplication](https://github.com/T00MATO/GameNetworkApplication)__ 를 참고해주세요.

# [GNCore](https://github.com/T00MATO/GameNetworkLib/tree/master/GNCore)

__GNServerLib__ 의 __ServerLauncher__를 구동시키는 솔루션입니다.

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



![GNServerLib drawio](https://user-images.githubusercontent.com/127966719/228170165-d1f3ab69-07ed-47b4-a616-69770ade0ee3.png)
