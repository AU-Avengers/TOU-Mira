using HarmonyLib;
using Reactor.Utilities.Attributes;
using UnityEngine;

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
        FakePlayers.Do(x => x.Destroy());
        FakePlayers.Clear();
    }
}