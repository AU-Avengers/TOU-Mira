using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class ArsonistRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => TouLocale.Get(TouNames.Arsonist, "Arsonist");
    public string RoleDescription => "Douse Players And Ignite The Light";

    public string RoleLongDescription => OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
        ? "Douse players and ignite the closest one to kill all doused targets"
        : "Douse players and ignite to kill all nearby doused targets";

    public Color RoleColor => TownOfUsColors.Arsonist;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public bool HasImpostorVision => OptionGroupSingleton<ArsonistOptions>.Instance.HasImpostorVision;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<ArsonistOptions>.Instance.CanVent,
        IntroSound = TouAudio.ArsoIgniteSound,
        MaxRoleCount = 15,
        Icon = TouRoleIcons.Arsonist,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        var allDoused = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            !x.HasDied() && x.GetModifier<ArsonistDousedModifier>()?.ArsonistId == Player.PlayerId);

        if (allDoused.Any())
        {
            stringB.Append("\n<b>Players Doused:</b>");
            foreach (var plr in allDoused)
            {
                stringB.Append(CultureInfo.InvariantCulture,
                    $"\n{Color.white.ToTextColor()}{plr.Data.PlayerName}</color>");
            }
        }

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
        var arsonistCount = CustomRoleUtils.GetActiveRolesOfType<ArsonistRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > arsonistCount)
        {
            return false;
        }

        return arsonistCount >= Helpers.GetAlivePlayers().Count - arsonistCount;
    }

    public string GetAdvancedDescription()
    {
        return $"The {RoleName} is a Neutral Killing role that wins by being the last killer alive. " +
               (OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
                   ? "They can douse players and ignite one of them to ignite all doused players on the map."
                   : "They can douse players and ignite them when close.") + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Douse",
            "Douse a player in gasoline",
            TouNeutAssets.DouseButtonSprite),
        new("Ignite",
            OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
                ? "Kill every doused player on the map as long as you ignite one player close by."
                : "Kill multiple doused players around you, given that they are within your radius.",
            TouNeutAssets.IgniteButtonSprite)
    ];

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.ArsoVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Arsonist);
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

    public override void OnDeath(DeathReason reason)
    {
        var button = CustomButtonSingleton<ArsonistIgniteButton>.Instance;

        if (button != null && button.Ignite != null)
        {
            button.Ignite.Clear();
            button.Ignite = null;
        }

        RoleBehaviourStubs.OnDeath(this, reason);
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

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}