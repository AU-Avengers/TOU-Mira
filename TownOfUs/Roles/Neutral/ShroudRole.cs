using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class ShroudRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public PlayerControl? EnshroudedTarget { get; set; }
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string RoleName => TouLocale.Get(TouNames.Shroud, "Shroud");
    public string RoleDescription => $"Make Crewmates Kill For You";
    public string RoleLongDescription => "Enshroud crewmates to make them do your bidding";
    public Color RoleColor => TownOfUsColors.Shroud;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<ShroudOptions>.Instance.CanVent,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Phantom),
        Icon = TouRoleIcons.Shroud,
        MaxRoleCount = 15,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => true;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        stringB.Append(CultureInfo.InvariantCulture);

        return stringB;
    }

    // public bool WinConditionMet()
    // {
    //     if (Player.HasDied())
    //     {
    //         return false;
    //     }

    //     var result = Helpers.GetAlivePlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;
    //     return result;
    // }

    public bool WinConditionMet()
    {
        var shroudCount = CustomRoleUtils.GetActiveRolesOfType<ShroudRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > shroudCount)
        {
            return false;
        }

        return shroudCount >= Helpers.GetAlivePlayers().Count - shroudCount;
    }
    

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Neutral Killing role that wins by being the last killer alive. They can enshroud a player then make them kill the closest player to them within kill range." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Enshroud",
            $"Enshroud a player, marking them for Compel.",
            TouNeutAssets.EnshroudSprite),
        new("Compel",
            $"Kill the closest player to your enshrouded target that's within kill range.\n\nYou cannot use Compel if nobody is in range of your enshrouded player.",
            TouNeutAssets.CompelSprite),
    ];
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.ShroudVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Shroud);
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }
    public static void RpcCompel(PlayerControl player, PlayerControl target)
    {
        var enshrouded = player.CachedPlayerData.PlayerName;
        var victim = target.CachedPlayerData.PlayerName;
        var shroud = PlayerControl.LocalPlayer;
        var icon = TouRoleIcons.Shroud.LoadAsset();

        if (player.HasDied())
        {
            if (shroud.AmOwner)
            {
                ShowNotification($"{enshrouded} could not be compelled.");
            }
            return;
        }

        if (target.HasModifier<BaseShieldModifier>())
        {
            if (shroud.AmOwner)
            {
                ShowNotification($"{victim} was protected from {enshrouded}!");
            }
            return;
        }

        if (target.HasModifier<FirstDeadShield>())
        {
            if (shroud.AmOwner)
            {
                ShowNotification($"{victim}'s first dead shield protected them from {enshrouded}!");
            }
            return;
        }

        
        if (shroud.AmOwner)
        {
            ShowNotification($"{enshrouded}, your enshrouded target, has killed {victim}.");
        }

        if (target.AmOwner)
        {
            ShowNotification($"{enshrouded} was compelled by a {TownOfUsColors.Shroud.ToTextColor()}Shroud</color> to kill you.");
        }

        shroud.RpcAddModifier<IndirectAttackerModifier>();

        shroud?.RpcCustomMurder(target, teleportMurderer: false);
        DeathHandlerModifier.RpcUpdateDeathHandler(target, "Compelled At", DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetTrue, $"By {shroud?.Data.PlayerName}", lockInfo: DeathHandlerOverride.SetTrue);
        shroud?.RpcRemoveModifier<IndirectAttackerModifier>();


        void ShowNotification(string message)
        {
            var notif = Helpers.CreateAndShowNotification($"<b>{message}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: icon);
            notif.Text.SetOutlineThickness(0.35f);
        }
    }
}