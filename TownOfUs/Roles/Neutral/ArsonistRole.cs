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
    public string LocaleKey => "Arsonist";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");

    public string RoleLongDescription => OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
        ? TouLocale.GetParsed($"TouRole{LocaleKey}TabDescriptionLegacy")
        : TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            TouLocale.GetParsed(OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
                ? $"TouRole{LocaleKey}WikiAdditionLegacy"
                : $"TouRole{LocaleKey}WikiAddition") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Douse", "Douse"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}DouseWikiDescription"),
                    TouNeutAssets.DouseButtonSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Ignite", "Ignite"),
                    TouLocale.GetParsed(OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
                        ? $"TouRole{LocaleKey}IgniteWikiDescriptionLegacy"
                        : $"TouRole{LocaleKey}IgniteWikiDescription"),
                    TouNeutAssets.IgniteButtonSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Arsonist;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        CanUseVent = OptionGroupSingleton<ArsonistOptions>.Instance.CanVent,
        IntroSound = TouAudio.ArsoIgniteSound,
        MaxRoleCount = 1,
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

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        var result = Helpers.GetAlivePlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;

        return result;
    }

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

        if (button.Ignite != null)
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