using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class ArsonistRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public override void SpawnTaskHeader(PlayerControl playerControl)
    {
        if (!playerControl.AmOwner)
        {
            return;
        }
        ImportantTextTask orCreateTask = PlayerTask.GetOrCreateTask<ImportantTextTask>(playerControl, 0);
        orCreateTask.Text =
            $"{TownOfUsColors.Neutral.ToTextColor()}{MiraLocaleManager.Get("NeutralKillingTaskHeader")}</color>";
        orCreateTask.name = "NeutralRoleText";
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ClericRole>());
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string IdPart => "Arsonist";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public static void SetDouseUses()
    {
        var button = CustomButtonSingleton<ArsonistDouseButton>.Instance;
        if (button.LimitedUses)
        {
            var dousedCount = ModifierUtils.GetPlayersWithModifier<ArsonistDousedModifier>().Count(x => !x.HasDied());
            var newUses = Math.Clamp(button.MaxUses - dousedCount, 0, button.MaxUses);
            button.SetUses(newUses);
        }
    }

    public string RoleLongDescription => OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
        ? MiraLocaleManager.Get($"TouRole{IdPart}TabDescriptionLegacy")
        : MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription") +
            MiraLocaleManager.Get(OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
                ? $"TouRole{IdPart}WikiAdditionLegacy"
                : $"TouRole{IdPart}WikiAddition") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Douse", "Douse"),
                    MiraLocaleManager.Get($"TouRole{IdPart}DouseWikiDescription"),
                    TouNeutAssets.DouseButtonSprite),
                new(MiraLocaleManager.Get($"TouRole{IdPart}Ignite", "Ignite"),
                    MiraLocaleManager.Get(OptionGroupSingleton<ArsonistOptions>.Instance.LegacyArsonist
                        ? $"TouRole{IdPart}IgniteWikiDescriptionLegacy"
                        : $"TouRole{IdPart}IgniteWikiDescription"),
                    TouNeutAssets.IgniteButtonSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Arsonist;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;

    public bool HasImpostorVision => OptionGroupSingleton<ArsonistOptions>.Instance.ImpostorVision;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Arsonist.LoadAsset(), "TouMira.Role.Neutral.Arsonist", 1.45f),
        CanUseVent = OptionGroupSingleton<ArsonistOptions>.Instance.CanVent,
        IntroSound = TouAudio.ArsoIgniteSound,
        OptionsScreenshot = TouBanners.NeutralRoleBanner,
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

        if (allDoused.HasAny())
        {
            stringB.Append(TownOfUsPlugin.Culture, $"\n<b>{MiraLocaleManager.Get("TouRoleArsonistTabDousedInfo")}</b>");
            foreach (var plr in allDoused)
            {
                stringB.Append(TownOfUsPlugin.Culture,
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

        var result = MiscUtils.GetImpactfulLivingPlayers().Count <= 2 && MiscUtils.KillersAliveCount == 1;

        return result;
    }

    public void OffsetButtons()
    {
        var canVent = OptionGroupSingleton<ArsonistOptions>.Instance.CanVent || LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.OffsetButtonsToggle.Value;
        var douse = CustomButtonSingleton<ArsonistDouseButton>.Instance;
        var ignite = CustomButtonSingleton<ArsonistIgniteButton>.Instance;
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(douse, !canVent));
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(ignite, !canVent));
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (Player.AmOwner)
        {
            OffsetButtons();
            if (!LegacyAssets.IsLegacy)
            {
                HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.ArsoVentSprite.LoadAsset();
                HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Arsonist);
            }
            SetDouseUses();
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

    public override void OnDeath(DeathReason reason)
    {
        var button = CustomButtonSingleton<ArsonistIgniteButton>.Instance;
        button.Ignite?.Clear();
        button.Ignite = null;

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