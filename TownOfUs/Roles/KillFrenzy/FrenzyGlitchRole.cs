using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.KillFrenzy;

public sealed class FrenzyGlitchRole(IntPtr cppPtr)
    : FrenzyRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    [HideFromIl2Cpp] public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.KillFrenzy;

    public bool CanSpawnOnCurrentMode() => MiscUtils.CurrentGamemode() is TouGamemode.KillFrenzy;

    [HideFromIl2Cpp]
    Func<bool> ICustomRole.VisibleInSettings => () => MiscUtils.CurrentGamemode() is TouGamemode.KillFrenzy;
    public bool WinConditionMet()
    {
        var wwCount = CustomRoleUtils.GetActiveRolesOfType<FrenzyGlitchRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > wwCount)
        {
            return false;
        }

        return wwCount >= Helpers.GetAlivePlayers().Count - wwCount;
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

    public string LocaleKey => "Glitch";
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
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Mimic", "Mimic"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}MimicWikiDescription"),
                    TouNeutAssets.MimicSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Hack", "Hack"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}HackWikiDescription"),
                    TouNeutAssets.HackSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Glitch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.FrenzyKiller;

    public CustomRoleConfiguration Configuration => new(this)
    {
        GhostRole = (RoleTypes)RoleId.Get<FrenzyGhostRole>(),
        FreeplayFolder = "Kill Frenzy",
        CanUseVent = false,
        IntroSound = TouAudio.GlitchSound,
        Icon = TouRoleIcons.Glitch
    };
    public void OffsetButtons()
    {
        // Because Glitch has multiple buttons, there's no need to offset it without a vent button; it looks weird with a random space - Atony
        var canVent = OptionGroupSingleton<GlitchOptions>.Instance.CanVent;
        var hack = CustomButtonSingleton<GlitchHackButton>.Instance;
        var mimic = CustomButtonSingleton<GlitchMimicButton>.Instance;
        var kill = CustomButtonSingleton<GlitchKillButton>.Instance;
        if (!canVent)
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(hack));
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(kill, !canVent));
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(mimic, !canVent));
        }
        else
        {
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(hack, false));
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(mimic, false));
            Coroutines.Start(MiscUtils.CoMoveButtonIndex(kill, false));
        }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.GlitchVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Glitch);
            CustomButtonSingleton<FakeVentButton>.Instance.Show = false;
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
            CustomButtonSingleton<FakeVentButton>.Instance.Show = true;
        }
    }
}