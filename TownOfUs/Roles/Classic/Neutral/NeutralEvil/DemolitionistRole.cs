using System;
using System.Text;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Buttons;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Neutral.NeutralEvil;

public sealed class DemolitionistRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant, IProgressTally, IAltWinConRole
{
    public static readonly Color DemolitionistColor = new Color32(0x28, 0x36, 0x7D, 255);
    public Color RoleColor => DemolitionistColor;
    public string IdPart => "Demolitionist";
    public bool RoleIsDisplayed => /*!OptionGroupSingleton<DemolitionistOptions>.Instance.DemoAnonymizeWin.Value*/ true;

    public LoadableAsset<Sprite> WinIcon => DivaniAssets.DemolitionistIcon;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public DoomableType DoomHintType => DoomableType.Fearmonger;

    public RoleBehaviour CrewVariant =>
        RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<EngineerTouRole>());

    public string GetAdvancedDescription() => this.GetRoleLongDescription() + MiscUtils.AppendOptionsText(GetType());

    public bool HasImpostorVision => true;

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(
            MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Ability.Plant"),
            MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Ability.Plant.Description"),
            DivaniAssets.DemolitionistPlantButton
        ),
        new(
            MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Ability.Defuse"),
            MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Ability.Defuse.Description"),
            DivaniAssets.DemolitionistDefuseButton
        )
    ];

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(DivaniAssets.DemolitionistIcon.LoadAsset(), "DivaniMod.Role.Neutral.Demolitionist", 1.45f),
        OptionsScreenshot = DivaniAssets.DemolitionistBanner,
        Icon = DivaniAssets.DemolitionistIcon,
        IntroSound = DivaniAssets.DemolitionistIntroSound,
        MaxRoleCount = 1,
        CanUseVent = OptionGroupSingleton<DemolitionistOptions>.Instance.CanVent,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
    };

    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (playerControl != PlayerControl.LocalPlayer)
        {
            return;
        }
        var task = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        task.Text =
            $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralEvilTaskHeader")}</color>";
        task.name = "NeutralRoleText";
    }

    public string GetSabotageTally()
    {
        var needed = (int)OptionGroupSingleton<DemolitionistOptions>.Instance.SabotagesToWin.Value;
        var capped = Math.Min(DemolitionistSabotageState.SuccessfulSabotages, needed);
        return $"{RoleColor.ToTextColor()}({capped}/{needed})</color>";
    }

    public bool ProgressOnName(bool localDead, bool inMeeting, bool amOwner, out string progress)
    {
        if (amOwner || (localDead && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow))
        {
            progress = GetSabotageTally();
            return true;
        }

        progress = string.Empty;
        return false;
    }

    public string ProgressOnSummaryNormal => GetSabotageTally();

    public string ProgressOnSummaryDetailed
    {
        get
        {
            var needed = (int)OptionGroupSingleton<DemolitionistOptions>.Instance.SabotagesToWin.Value;
            var capped = Math.Min(DemolitionistSabotageState.SuccessfulSabotages, needed);

            return MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Progress.SuccessfulSabotages")
                .Replace("<count>", capped.ToString(TownOfUsPlugin.Culture))
                .Replace("<needed>", needed.ToString(TownOfUsPlugin.Culture));
        }
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        var needed = (int)OptionGroupSingleton<DemolitionistOptions>.Instance.SabotagesToWin.Value;
        var capped = Math.Min(DemolitionistSabotageState.SuccessfulSabotages, needed);
        var progressText = MiraLocaleManager.Get("TownOfUsMira.Role.Demolitionist.Progress.SuccessfulSabotages")
            .Replace("<count>", capped.ToString(TownOfUsPlugin.Culture))
            .Replace("<needed>", needed.ToString(TownOfUsPlugin.Culture));

        stringB.AppendLine(TownOfUsPlugin.Culture, $"<b>{progressText}</b>");
        return stringB;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (Player.AmOwner)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = DivaniAssets.DemolitionistVentButton.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(DemolitionistColor);
            CustomButtonSingleton<FakeVentButton>.Instance.Show = false;
        }

        DemolitionistSabotageState.RegisterDemolitionist(player);

        AboutToTorment = false;
        HasKilled = false;
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

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public bool ReachedWinCondition
    {
        get
        {
            var needed = (int)OptionGroupSingleton<DemolitionistOptions>.Instance.SabotagesToWin.Value;
            return DemolitionistSabotageState.SuccessfulSabotages >= needed;
        }
    }

    public AltWinResult WinOutcome => OptionGroupSingleton<DemolitionistOptions>.Instance.WinOutcome;

    public AltWinResult EffectiveWinOutcome => WinOutcome;

    public bool AboutToTorment { get; set; }

    public bool HasKilled { get; set; }

    public bool WinConditionMet()
    {
        return WinOutcome is AltWinResult.EndsGame && ReachedWinCondition;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return ReachedWinCondition;
    }
}
