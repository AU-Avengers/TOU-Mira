using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class WerewolfRole(IntPtr cppPtr)
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

    public bool Rampaging { get; set; }
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<HunterRole>());
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string IdPart => "Werewolf";
    public string RoleMedDescriptionLocale => $"TownOfUsMira.Role.{IdPart}.TabDescription";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Rampage", "Rampage"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Rampage.WikiDescription"),
                    TouNeutAssets.RampageSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Werewolf;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Werewolf.LoadAsset(), "TouMira.Role.Neutral.Werewolf", 1.45f),
        CanUseVent = OptionGroupSingleton<WerewolfOptions>.Instance.CanVent /* && (Rampaging || Player.inVent)*/,
        IntroSound = TouAudio.WerewolfRampageSound,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        Icon = TouRoleIcons.Werewolf,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => Rampaging;

    public bool WinConditionMet()
    {
        var wwCount = CustomRoleUtils.GetActiveRolesOfType<WerewolfRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > wwCount || MiscUtils.KillersAliveCount == 0)
        {
            return false;
        }

        return wwCount >= MiscUtils.GetImpactfulLivingPlayers().Count - wwCount;
    }



    public void OffsetButtons()
    {
        var canVent = OptionGroupSingleton<WerewolfOptions>.Instance.CanVent || LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.OffsetButtonsToggle.Value;
        var rampage = CustomButtonSingleton<WerewolfRampageButton>.Instance;
        var kill = CustomButtonSingleton<WerewolfKillButton>.Instance;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(rampage, !canVent));
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(kill, !canVent));
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
            if (!LegacyAssets.IsLegacy)
            {
                HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.WerewolfVentSprite.LoadAsset();
                HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Werewolf);
            }
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        if (Player.AmOwner && !LegacyAssets.IsLegacy)
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
}