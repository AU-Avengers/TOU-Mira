using System.Diagnostics.CodeAnalysis;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace TownOfUs.Modules.Cosmetics;

public interface IBaseLoader
{
    public void InstallCosmetics(ReferenceData refData);

    public void LoadCosmetics(string directory);

    public bool LocateCosmetic(string id, string type, [NotNullWhen(true)] out Il2CppSystem.Type? il2CPPType);

    public bool ProvideCosmetic(ProvideHandle handle, string id, string type);
}