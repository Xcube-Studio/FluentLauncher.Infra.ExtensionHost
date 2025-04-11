# FluentLauncher.Infra.ExtensionHost
Fluent Launcher 的插件化接口仓库，该仓库源代码基于 [dnchattan/winui-extensions](https://github.com/dnchattan/winui-extensions) 仓库代码修改而来，在原基础上修改了插件加载的步骤顺序，以便支持 Fluent Launcher 的依赖注入  

目前仅 Fluent Launcher `Preview 通道` 的部分版本支持了该接口以便加载插件，  
我们目前正在维护的插件只有 [FluentLauncher.Extension.ConnectX](https://github.com/Xcube-Studio/FluentLauncher.Extension.ConnectX)

## 使用示例

以下代码取自 [FluentLauncher.Extension.ConnectX](https://github.com/Xcube-Studio/FluentLauncher.Extension.ConnectX) 仓库
``` CSharp

using FluentLauncher.Infra.ExtensionHost.Extensions;

// 插件类型，必须继承自 FluentLauncher.Infra.ExtensionHost.Extensions.IExtension 接口
// INavigationProviderExtension 接口用来提供导航栏侧栏的 UI
public class ConnectXExtension : IExtension, INavigationProviderExtension
{
    public string Name => "FluentLauncher.Extension.ConnectX"; // 插件名称

    public static IServiceProvider? Services { get; private set; } // 由 Fluent Launcher 方注入的 IServiceProvider

    public static string ExtensionFolder { get; private set; } = null!; // 由 Fluent Launcher 方分配的插件配置文件目录，从 IExtension.SetExtensionFolder 设置

    // 为 Fluent Launcher 的导航服务注册新的页面，格式 { "页面Key", (typeof(页面Xaml类型), typeof(页面ViewModel类型)) }
    public Dictionary<string, (Type, Type)> RegisteredPages { get; } = new()
    {
        { "ConnectXPage", (typeof(ConnectXPage), typeof(ConnectXViewModel)) },
    };

    // 为 Fluent Launcher 的导航服务注册新的 ConntentDialog
    public Dictionary<string, (Type, Type)> RegisteredDialogs { get; } = new()
    {
        { "ConnectXCreateRoomDialog", (typeof(CreateRoomDialog), typeof(CreateRoomDialogViewModel)) },
        { "ConnectXJoinRoomDialog", (typeof(JoinRoomDialog), typeof(JoinRoomDialogViewModel)) },
    };

    // 由 Fluent Launcher 方提供的 IServiceCollection ，依赖注入服务
    void IExtension.ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<RoomService>();
        services.AddSingleton<ConnectService>();
        services.UseConnectX();

        services.AddSerilog(configure => 
        {
            configure.WriteTo.Logger(l =>
            {
                l.Filter.ByIncludingOnly(Matching.FromSource("ConnectX.Client"))
                    .WriteTo.File(Path.Combine(ExtensionFolder, "ConnectX.Client", "Logs", "Log-.log"),
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true,
                        fileSizeLimitBytes: 1000000,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}][{Level:u3}] <{SourceContext}>: {Message:lj}{NewLine}{Exception}");
            });
        });
    }

    // 
    void IExtension.SetServiceProvider(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;

        List<IHostedService> backgroundServices =
        [
            serviceProvider.GetRequiredService<Router>(),
            serviceProvider.GetRequiredService<IZeroTierNodeLinkHolder>(),
            serviceProvider.GetRequiredService<IRoomInfoManager>(),
            //serviceProvider.GetRequiredService<IServerLinkHolder>(),
            serviceProvider.GetRequiredService<PeerManager>(),
            serviceProvider.GetRequiredService<ProxyManager>(),
            serviceProvider.GetRequiredService<FakeServerMultiCaster>(),
        ];

        backgroundServices.ForEach(s => Task.Run(async () => await s.StartAsync(default)));

        serviceProvider.GetService<ConnectService>();
    }

    void IExtension.SetExtensionFolder(string folder) => ExtensionFolder = folder;

    NavigationViewItem[] INavigationProviderExtension.ProvideNavigationItems() =>
    [
        new()
        {
            Icon = new SymbolIcon(Symbol.Link),
            Content = "多人游戏",
            Tag = "ConnectXPage"
        }
    ];
}

```
