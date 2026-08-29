using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Buttons;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class GlitchRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text = $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralKillingTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    [HideFromIl2Cpp]
    public bool IsModifierApplicable(BaseModifier modifier)
    {
        return modifier is not OverclockerModifier;
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<BarkeeperRole>());
    public DoomableType DoomHintType => DoomableType.Perception;
    public string IdPart => "Glitch";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Mimic", "Mimic"),
                    MiraLocaleManager.Get($"TouRole{IdPart}MimicWikiDescription"),
                    TouNeutAssets.MimicSprite),
                new(MiraLocaleManager.Get($"TouRole{IdPart}Hack", "Hack"),
                    MiraLocaleManager.Get($"TouRole{IdPart}HackWikiDescription"),
                    TouNeutAssets.HackSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Glitch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Glitch.LoadAsset(), "TouMira.Role.Neutral.Glitch", 1.45f),
        CanUseVent = (GlitchVent)OptionGroupSingleton<GlitchOptions>.Instance.CanVent.Value is not GlitchVent.Never,
        IntroSound = TouAudio.GlitchSound,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        Icon = TouRoleIcons.Glitch,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => true;

    public bool WinConditionMet()
    {
        var glitchCount = CustomRoleUtils.GetActiveRolesOfType<GlitchRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > glitchCount || MiscUtils.KillersAliveCount == 0)
        {
            return false;
        }

        return glitchCount >= MiscUtils.GetImpactfulLivingPlayers().Count - glitchCount;
    }




    public void OffsetButtons()
    {
        // Because Glitch has multiple buttons, there's no need to offset it without a vent button; it looks weird with a random space - Atony
        var canVent = (GlitchVent)OptionGroupSingleton<GlitchOptions>.Instance.CanVent.Value is not GlitchVent.Never;
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
            if (!LegacyAssets.IsLegacy)
            {
                HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.GlitchVentSprite.LoadAsset();
                HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Glitch);
            }
            CustomButtonSingleton<FakeVentButton>.Instance.Show = false;
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        if (Player.AmOwner)
        {
            if (!LegacyAssets.IsLegacy)
            {
                HudManager.Instance.ImpostorVentButton.graphic.sprite = TouAssets.VentSprite.LoadAsset();
                HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Impostor);
            }
            CustomButtonSingleton<FakeVentButton>.Instance.Show = true;
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

    [MethodRpc((uint)TownOfUsRpc.TriggerGlitchHack)]
    public static void RpcTriggerGlitchHack(PlayerControl victim, bool fullRemoval)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(victim);
            return;
        }
        if (victim.TryGetModifier<GlitchHackedModifier>(out var hackMod))
        {
            if (fullRemoval)
            {
                victim.RemoveModifier(hackMod);
            }
            else
            {
                hackMod.ShowHacked();
            }
        }
    }
}