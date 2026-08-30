using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class MedusaRole(IntPtr cppPtr)
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

    public static bool AutoPlaceFakePlayers => true;
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MediumRole>());
    public DoomableType DoomHintType => DoomableType.Death;
    public string IdPart => "Medusa";
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
            List<CustomButtonWikiDescription> list =
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Petrify", "Petrify"),
                    MiraLocaleManager.Get($"TouRole{IdPart}PetrifyWikiDescription"),
                    TouNeutAssets.PetrifySprite)
            ];
            if (OptionGroupSingleton<MedusaOptions>.Instance.StoneGazeAvailable.Value)
            {
                list.Add(new(MiraLocaleManager.Get($"TouRole{IdPart}StoneGaze", "Stone Gaze"),
                    MiraLocaleManager.Get($"TouRole{IdPart}StoneGazeWikiDescription"),
                    TouNeutAssets.StoneGazeSprite));
            }
            return list;
        }
    }

    public Color RoleColor => TownOfUsColors.Medusa;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Medusa.LoadAsset(), "TouMira.Role.Neutral.Medusa", 1.45f),
        CanUseVent = OptionGroupSingleton<MedusaOptions>.Instance.CanVent,
        IntroSound = TouAudio.PhantomIntroSound,
        Icon = TouRoleIcons.Medusa,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool HasImpostorVision => true;
    
    public bool WinConditionMet()
    {
        var scCount = CustomRoleUtils.GetActiveRolesOfType<MedusaRole>().Count(x => !x.Player.HasDied());

        if (MiscUtils.KillersAliveCount > scCount || MiscUtils.KillersAliveCount == 0)
        {
            return false;
        }

        return scCount >= MiscUtils.GetImpactfulLivingPlayers().Count - scCount;
    }



    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner && !LegacyAssets.IsLegacy)
        {
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.MedusaVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Medusa);
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
}