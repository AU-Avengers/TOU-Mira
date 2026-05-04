using Il2CppInterop.Runtime.Injection;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace TownOfUs.Modules.Cosmetics.Unity;

public class HatProvider : ResourceProviderBase
{
    private static HatProvider? _instance;
    private static IResourceProvider? _provider;
    
    public static void Initialize()
    {
        _instance = new HatProvider();
        // interfaces r broken in il2cpp so we have to use pointer magic
        _provider = new IResourceProvider(_instance.Pointer);
        Addressables.ResourceManager.ResourceProviders.Insert(0, _provider);
    }

    public HatProvider(IntPtr intPtr) : base(intPtr) { }

    public HatProvider() : base(ClassInjector.DerivedConstructorPointer<HatProvider>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public override bool CanProvide(Il2CppSystem.Type t, IResourceLocation location)
    {
        return location.InternalId.StartsWith("toum.");
    }

    public override Il2CppSystem.Type GetDefaultType(IResourceLocation location)
    {
        return location.ResourceType;
    }

    public override void Provide(ProvideHandle provideHandle)
    {
        string internalId = provideHandle.Location.InternalId;
        Debug($"Processing {internalId}");

        if (!internalId.StartsWith("toum"))
        {
            Error($"{internalId} is not a TOU Mira cosmetic");
            provideHandle.Complete<UnityEngine.Object>(null!, false, new Il2CppSystem.Exception("Not a TOU Mira cosmetic"));
            return;
        }

        var idAndType = internalId.Split("/");
        if (idAndType.Length != 2) 
        {
            Error($"Invalid identifier: {idAndType}");
            provideHandle.Complete<UnityEngine.Object>(null!, false, new Il2CppSystem.Exception("Invalid TOU Mira ID"));
            return;
        }

        var id = idAndType[0];
        var type = idAndType[1];

        if (CosmeticsLoader.Instance.ProvideCosmetic(provideHandle, id, type, out var exception))
        {
            Debug($"Successfully provided cosmetic {id} of type {type}");
        }
        else
        {
            Error($"Failed to provide cosmetic {id} of type {type}:\n{exception.ToString()}");
            provideHandle.Complete<UnityEngine.Object>(null!, false, 
                new Il2CppSystem.Exception(exception.ToString()));
        }
    }

    public override void Release(IResourceLocation location, Il2CppSystem.Object obj)
    {
        Warning("I don't know how to release cosmetic yet");
    }
}