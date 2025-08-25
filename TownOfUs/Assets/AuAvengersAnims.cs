using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class AuAvengersAnims
{
    /*public static AssetBundle SoundVisionBundle { get; } =
        AssetBundleManager.Load(typeof(AuAvengersAnims).Assembly, "soundvision");*/ // unused?

    // bomb visualizer thing
    public static LoadableAsset<Material> BombMaterial { get; private set; }
    public static LoadableAsset<Material> IgniteMaterial { get; private set; }
    public static LoadableAsset<Material> TrapMaterial { get; private set; }

    public static void Initialize()
    {
        BombMaterial = new LoadableBundleAsset<Material>("bomb", TouAssets.MainBundle);
        IgniteMaterial = new LoadableBundleAsset<Material>("ignite", TouAssets.MainBundle);
        TrapMaterial = new LoadableBundleAsset<Material>("trap", TouAssets.MainBundle);
    }
}