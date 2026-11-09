namespace TownOfUs.AnimationSystem;

public static class KillAnimationHelpers
{
    private static int _nextKillEnum = 129;

    public static KillAnimType CreateNewKillAnimType()
    {
        return (KillAnimType)_nextKillEnum++;
    }
}