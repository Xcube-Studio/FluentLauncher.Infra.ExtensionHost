using FluentLauncher.Infra.ExtensionHost.Assemblies;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Runtime.CompilerServices;

namespace FluentLauncher.Infra.ExtensionHost;

public interface IApplicationExtensionHost
{
    void Initialize(object application);

    IExtensionAssembly GetExtensionAssembly(string pathToAssembl);

    IDisposable RegisterXamlTypeMetadataProvider(IXamlMetadataProvider provider);

    Uri LocateResource(object component, [CallerFilePath] string callerFilePath = "");
}
