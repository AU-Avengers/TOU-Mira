using Reactor.Utilities.Attributes;

namespace TownOfUs.Modules;

[RegisterInIl2Cpp]
public sealed class FakePlayer(IntPtr cppPtr) : BaseFakePlayer(cppPtr)
{
    public static readonly List<FakePlayer> FakePlayers = [];

    public static FakePlayer CreateFakePlayer(PlayerControl player)
    {
        return BaseFakePlayerExt.CreateBaseFake<FakePlayer>(player);
    }

    public override void Start()
    {
        base.Start();

        FakePlayers.Add(this);
    }

    public static void UpdateFakePlayerText(bool waitForRole = false)
    {
        UpdateFakePlayerText(waitForRole, FakePlayers);
    }

    public static void ClearAll()
    {
        FakePlayers.ProcessAndClear(x => x.Destroy());
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        FakePlayers.Remove(this);
    }
}