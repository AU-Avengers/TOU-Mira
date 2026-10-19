using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Buttons.Neutral;
using TownOfUs.GameModes;
using UnityEngine;

namespace TownOfUs.Roles.KillFrenzy;

public sealed class FrenzyWerewolfRole(IntPtr cppPtr)
    : FrenzyRole(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    public bool WinConditionMet()
    {
        var wwCount = CustomRoleUtils.GetActiveRolesOfType<FrenzyWerewolfRole>().Count(x => !x.Player.HasDied());

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

    public bool Rampaging { get; set; }
    public string IdPart => "Werewolf";
    [HideFromIl2Cpp] public bool IsHiddenFromList => MiscUtils.CurrentGamemode() is not TouGamemode.KillFrenzy;

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
            return new List<CustomButtonWikiDescription>
            {
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Rampage", "Rampage"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Rampage.WikiDescription"),
                    TouNeutAssets.RampageSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Werewolf;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.FrenzyKiller;

    public CustomRoleConfiguration Configuration => new(this)
    {
        AssociatedGameMode = typeof(KillFrenzyMode),
        GhostRole = (RoleTypes)RoleId.Get<FrenzyGhostRole>(),
        FreeplayFolder = "Kill Frenzy",
        CanUseVent = false,
        IntroSound = TouAudio.WerewolfRampageSound,
        Icon = TouRoleIcons.Werewolf,
    };

    public bool HasImpostorVision => Rampaging;



    public void OffsetButtons()
    {
        var canVent = LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.OffsetButtonsToggle.Value;
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
            HudManager.Instance.ImpostorVentButton.graphic.sprite = TouNeutAssets.WerewolfVentSprite.LoadAsset();
            HudManager.Instance.ImpostorVentButton.buttonLabelText.SetOutlineColor(TownOfUsColors.Werewolf);
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
        }
    }
}