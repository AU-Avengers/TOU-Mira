using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace TownOfUs.Assets;

public static class TouAudio
{
    // THIS FILE SHOULD ONLY HOLD AUDIO
    public static LoadableAsset<AudioClip> NoisemakerDeathSound { get; } =
        new LoadableBundleAsset<AudioClip>("NoisemakerDeath", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> TrackerActivateSound { get; } =
        new LoadableBundleAsset<AudioClip>("TrackerActivate", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> TrackerDeactivateSound { get; } =
        new LoadableBundleAsset<AudioClip>("TrackerDeactivate", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> TrapperPlaceSound { get; } =
        new LoadableBundleAsset<AudioClip>("TrapperPlace", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> QuestionSound { get; } =
        new LoadableBundleAsset<AudioClip>("QuestionSound", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> ToppatIntroSound { get; } =
        new LoadableBundleAsset<AudioClip>("ToppatIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> ProsIntroSound { get; } =
        new LoadableBundleAsset<AudioClip>("ProsIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> SpyIntroSound { get; } =
        new LoadableBundleAsset<AudioClip>("SpyIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> OtherIntroSound { get; } =
        new LoadableBundleAsset<AudioClip>("OtherIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> SheriffIntroSound { get; } =
        new LoadableBundleAsset<AudioClip>("SheriffIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> VigiIntroSound { get; } =
        new LoadableBundleAsset<AudioClip>("VigilanteIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> AltruistReviveSound { get; } =
        new LoadableBundleAsset<AudioClip>("AltruistIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> JailSound { get; } = new LoadableBundleAsset<AudioClip>("JailSound", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> MayorRevealSound { get; set; } =
        new LoadableBundleAsset<AudioClip>("MayorRevealSound", TouAssets.MainBundle);
    
    public static LoadableAsset<AudioClip> PoliticianIntroSound { get; set; } =
        new LoadableBundleAsset<AudioClip>("MayorRevealSound", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> MediumIntroSound { get; } =
        new LoadableBundleAsset<AudioClip>("MediumIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> TimeLordIntroSound { get; } =
        new LoadableBundleAsset<AudioClip>("TimeLordIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> EngiFix1 { get; } = new LoadableBundleAsset<AudioClip>("Fix1", TouAssets.MainBundle);
    public static LoadableAsset<AudioClip> EngiFix2 { get; } = new LoadableBundleAsset<AudioClip>("Fix2", TouAssets.MainBundle);
    public static LoadableAsset<AudioClip> EngiFix3 { get; } = new LoadableBundleAsset<AudioClip>("Fix3", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> GuardianAngelSound { get; } =
        new LoadableBundleAsset<AudioClip>("GuardianAngelProtect", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> DiscoveredSound { get; } =
        new LoadableBundleAsset<AudioClip>("Discovered", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> GlitchSound { get; } =
        new LoadableBundleAsset<AudioClip>("GlitchIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> HackedSound { get; } =
        new LoadableBundleAsset<AudioClip>("HackedSound", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> UnhackedSound { get; } =
        new LoadableBundleAsset<AudioClip>("UnhackedSound", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> MimicSound { get; } =
        new LoadableBundleAsset<AudioClip>("MimicSound", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> UnmimicSound { get; } =
        new LoadableBundleAsset<AudioClip>("UnmimicSound", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> ArsoIgniteSound { get; } =
        new LoadableBundleAsset<AudioClip>("ArsonistIgnite", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> WerewolfRampageSound { get; } =
        new LoadableBundleAsset<AudioClip>("WerewolfRampage", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> GrenadeSound { get; } =
        new LoadableBundleAsset<AudioClip>("GrenadeThrow", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> WarlockIntroSound { get; } =
        new LoadableBundleAsset<AudioClip>("WarlockIntro", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> SwooperActivateSound { get; } =
        new LoadableBundleAsset<AudioClip>("SwooperActivate", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> SwooperDeactivateSound { get; } =
        new LoadableBundleAsset<AudioClip>("SwooperDeactivate", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> MineSound { get; } = new LoadableBundleAsset<AudioClip>("MineSound", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> EscapistMarkSound { get; } =
        new LoadableBundleAsset<AudioClip>("EscapistMark", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> EscapistRecallSound { get; } =
        new LoadableBundleAsset<AudioClip>("EscapistRecall", TouAssets.MainBundle);

    public static LoadableAsset<AudioClip> JanitorCleanSound { get; } =
        new LoadableBundleAsset<AudioClip>("JanitorClean", TouAssets.MainBundle);

    public static void PlaySound(LoadableAsset<AudioClip> clip, float vol = 1f)
    {
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(clip.LoadAsset(), false, vol);
        }
    }
}