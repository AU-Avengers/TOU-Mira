using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class VampireRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Death;
    public string YouAreText => TouLocale.Get("YouAreA");
    public string YouWereText => TouLocale.Get("YouWereA");
    public string LocaleKey => "Vampire";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Bite", "Bite"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}BiteWikiDescription"),
                    TouNeutAssets.BiteSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Vampire;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<VampireOptions>.Instance.CanVent,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Phantom),
        Icon = TouRoleIcons.Vampire,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        MaxRoleCount = 1
    };

    public bool HasImpostorVision => OptionGroupSingleton<VampireOptions>.Instance.HasVision;

    public bool WinConditionMet()
    {
        var vampireCount = CustomRoleUtils.GetActiveRolesOfType<VampireRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > vampireCount)
        {
            return false;
        }

        return vampireCount >= Helpers.GetAlivePlayers().Count - vampireCount;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.VampVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Vampire);
        }

        if (!Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
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

    [MethodRpc((uint)TownOfUsRpc.VampireBite)]
    public static void RpcVampireBite(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not VampireRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcVampireBite - Invalid vampire");
            return;
        }

        var touAbilityEvent = new TouAbilityEvent(AbilityType.VampireBite, player, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        target.ChangeRole(RoleId.Get<VampireRole>());
        target.AddModifier<VampireBittenModifier>();

        if (OptionGroupSingleton<VampireOptions>.Instance.CanGuessAsNewVamp)
        {
            target.AddModifier<NeutralKillerAssassinModifier>();
        }
    }
}