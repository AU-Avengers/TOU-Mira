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
        var obj = new GameObject($"Fake {player.gameObject.name}")
        {
            layer = LayerMask.NameToLayer("Players")
        };
        var fake = obj.AddComponent<FakePlayer>();
        fake.PlayerId = player.PlayerId;
        fake.OriginalPlayer = player;
        fake.body = obj;
        return fake;
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