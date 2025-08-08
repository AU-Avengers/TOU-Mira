using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Impostor;
using MiraAPI.Networking;
using TownOfUs.Modifiers;
// using TownOfUs.Events;
// using TownOfUs.Modifiers;
// using TownOfUs.Events;

namespace TownOfUs.Roles.Impostor;

public sealed class SpellslingerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    private bool _bombed;

    public void FixedUpdate()
    {
        if (_bombed || Player == null || Player.Data?.Role is not SpellslingerRole || Player.HasDied())
            return;

        if (!EveryoneHexed())
            return;

        _bombed = true;
        RpcHexBomb(Player);
    }

    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => TouLocale.Get(TouNames.Spellslinger, "Spellslinger");
    public string RoleDescription => "Hex Everyone Then Bomb Them All";
    public string RoleLongDescription => "Hex all non-Impostors to set them off in a Hex Bomb";
    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Spellslinger,
        MaxRoleCount = 1,
        IntroSound = TouAudio.ArsoIgniteSound,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var allAlive = Helpers.GetAlivePlayers();

        var hexed = allAlive
            .Where(p => p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        var unhexedNonImpostors = allAlive
            .Where(p => !p.IsImpostor() && !p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        if (hexed.Count > 0)
        {
            sb.Append("\n<b>Hexed Players:</b>");
            foreach (var player in hexed)
            {
                var color = player.IsImpostor() ? "red" : "white";
                sb.Append(TownOfUsPlugin.Culture, $"\n<color={color}><size=75%>{player.Data.PlayerName}</size></color>");
            }
        }

        sb.Append(TownOfUsPlugin.Culture, $"\n\n<b>Players Left to Hex: {unhexedNonImpostors.Count}</b>");
        // foreach (var player in unhexedNonImpostors)
        // {
        //     sb.Append(TownOfUsPlugin.Culture, $"\n{player.Data.PlayerName}");
        // }

        return sb;
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is an Impostor Power role that can hex a player, priming them for detonation.\n\nOnce all non Impostors are hexed, all hexed players will die."
            + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new CustomButtonWikiDescription(
            "Hex",
            $"Hex a player, priming them for detonation.",
            TouImpAssets.HexButtonSprite),

        new CustomButtonWikiDescription(
            "Hex Bomb (Passive)",
            $"Kill all hexed players.\nHex Bomb will activate automatically once all non Impostors are hexed.",
            TouImpAssets.HexButtonSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.Hex, SendImmediately = true)]
    public static void RpcHex(PlayerControl player, PlayerControl target)
    {
        var canBeHexed = PlayerControl.AllPlayerControls.ToArray().Where(x => !x.HasDied() && !x.IsImpostor()).ToList();

        if (player.Data.Role is not SpellslingerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcHex - Invalid Spellslinger");
            return;
        }

        if (canBeHexed.Contains(target))
        {
            target.AddModifier<SpellslingerHexedModifier>();

            if (player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{target.CachedPlayerData.PlayerName} is hexed!</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spellslinger.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
            }

            return;
        }

        if (player.AmOwner)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{target.CachedPlayerData.PlayerName} could not be hexed! <color=#ff0000>(A bug occurred)</color></b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spellslinger.LoadAsset());
            notif1.Text.SetOutlineThickness(0.35f);
        }

    }

    [MethodRpc((uint)TownOfUsRpc.HexBomb, SendImmediately = true)]
    public static void RpcHexBomb(PlayerControl player)
    {
        if (player.Data.Role is not SpellslingerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcHexBomb - Invalid Spellslinger");
            return;
        }

        var hexed = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.HasDied() && p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        if (hexed.Count == 0)
        {
            if (player.AmOwner)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>Nobody is hexed?? <color=#ff0000>(A bug occurred)</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spellslinger.LoadAsset());
                notif1.Text.SetOutlineThickness(0.35f);
            }
            return;
        }

        TouAudio.PlaySound(TouAudio.ArsoIgniteSound);
        Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Impostor));
        foreach (var target in hexed)
        {
            player.RpcAddModifier<IndirectAttackerModifier>(true);
            player.RpcCustomMurder(target, teleportMurderer: false, playKillSound: false);
            // DeathHandlerModifier.RpcUpdateDeathHandler(target, "Disintegrated", DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetTrue, $"By {player.Data.PlayerName}", lockInfo: DeathHandlerOverride.SetTrue);
            target.RemoveModifier<SpellslingerHexedModifier>();

            if (player.AmOwner && target == player)
            {
                var selfNotif = Helpers.CreateAndShowNotification(
                    $"<b>You hexed... yourself?</b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Spellslinger.LoadAsset());
                selfNotif.Text.SetOutlineThickness(0.4f);
            }
        }

        if (player.AmOwner)
        {
            var notif = Helpers.CreateAndShowNotification(
                $"<b>Disintegrated {hexed.Count} hexed players!</b>", 
                Color.white, new Vector3(0f, 1f, -20f), 
                spr: TouRoleIcons.Spellslinger.LoadAsset());
            notif.Text.SetOutlineThickness(0.4f);
        }

        player.RpcRemoveModifier<IndirectAttackerModifier>();
    }

    public static bool EveryoneHexed()
    {
        return PlayerControl.AllPlayerControls
            .ToArray()
            .Where(p => !p.HasDied() && !p.IsImpostor())
            .All(p => p.HasModifier<SpellslingerHexedModifier>());
    }

}