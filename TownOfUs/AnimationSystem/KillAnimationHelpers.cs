using TownOfUs.Networking;

namespace TownOfUs.AnimationSystem;

public static class KillAnimationHelpers
{
    private static int _nextKillEnum = 129;

    public static KillAnimType CreateNewKillAnimType()
    {
        return (KillAnimType)_nextKillEnum++;
    }

    public static OverlayKillAnimation GetAndDisplayGenericKillAnim(KillOverlay killOverlay, NetworkedPlayerInfo killer,
        NetworkedPlayerInfo victim, ExtendedKillAnimType killType)
    {
        OverlayKillAnimation original = CustomTouMurderRpcs.GetKillAnimation(HudManager.Instance.KillOverlay, killType);

        killOverlay.ShowKillAnimation(original, killer, victim);
        return original;
    }
}