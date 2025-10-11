using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Other;

public sealed class SpectatorRole(IntPtr cppPtr) : RoleBehaviour(cppPtr), ITownOfUsRole, IWikiDiscoverable
{
    private Minigame _hauntMenu = null!;

    public static readonly HashSet<string> TrackedSpectators = [];
    public static readonly List<PlayerControl> TrackedPlayers = [];
    public static bool FixedCam;
    private static int CurrentTarget;
    private bool ShowHud;
    private bool ShowShadows;

    public void Awake()
    {
        var crewGhost = RoleManager.Instance.GetRole(RoleTypes.CrewmateGhost).Cast<CrewmateGhostRole>();
        _hauntMenu = crewGhost.HauntMenu;
        Ability = crewGhost.Ability;
    }

    // reimplement haunt minigame
    public override void UseAbility()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening)
        {
            return;
        }

        if (Minigame.Instance)
        {
            if (Minigame.Instance.TryCast<HauntMenuMinigame>())
            {
                Minigame.Instance.Close();
            }

            return;
        }

        var minigame = Instantiate(_hauntMenu, HudManager.Instance.AbilityButton.transform, false);
        minigame.transform.SetLocalZ(-5f);
        minigame.Begin(null);
        HudManager.Instance.AbilityButton.SetDisabled();
    }

    public string LocaleKey => "Spectator";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Spectator;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.GameOutlier;
    public bool IsHiddenFromList => true;

    public override bool IsDead => true;

    public CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = false,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Noisemaker),
        GhostRole = (RoleTypes)RoleId.Get<SpectatorRole>(),
        Icon = TouRoleIcons.Spectator,
        CanModifyChance = false,
        MaxRoleCount = 0,
        RoleHintType = RoleHintType.RoleTab
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (!Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
        }

        DeathHandlerModifier.UpdateDeathHandler(Player, "Spectating", 0, DeathHandlerOverride.SetFalse);

        if (!Player.AmOwner)
        {
            return;
        }

        ShowHud = false;
        FixedCam = true;
        ShowShadows = false;

        if (!HudManager.InstanceExists)
        {
            return;
        }

        HudManager.Instance.PlayerCam.SetTarget(Player);
        HudManager.Instance.ShadowQuad.gameObject.SetActive(ShowShadows);
        HudManager.Instance.SetHudActive(ShowHud);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        ShowHud = true;
        FixedCam = false;
        ShowShadows = true;

        if (!targetPlayer || !targetPlayer.AmOwner || !HudManager.InstanceExists)
        {
            return;
        }

        HudManager.Instance.PlayerCam.SetTarget(targetPlayer);
        HudManager.Instance.ShadowQuad.gameObject.SetActive(ShowShadows);
        HudManager.Instance.SetHudActive(ShowHud);
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public void Update()
    {
        if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null ||
            PlayerControl.LocalPlayer.Data.Role is not SpectatorRole || LobbyBehaviour.Instance ||
            MeetingHud.Instance ||
            !HudManager.Instance)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            FixedCam = !FixedCam;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            HudManager.Instance.PlayerCam.SetTarget(Player);
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowHud = !ShowHud;
            HudManager.Instance.SetHudActive(ShowHud);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ShowShadows = !ShowShadows;
            HudManager.Instance.ShadowQuad.gameObject.SetActive(ShowShadows);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            CurrentTarget--;

            if (CurrentTarget <= -1)
            {
                CurrentTarget = TrackedPlayers.Count - 1;
            }

            HudManager.Instance.PlayerCam.SetTarget(TrackedPlayers[CurrentTarget]);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            CurrentTarget++;

            if (CurrentTarget >= TrackedPlayers.Count)
            {
                CurrentTarget = 0;
            }

            HudManager.Instance.PlayerCam.SetTarget(TrackedPlayers[CurrentTarget]);
        }
    }

    public override bool CanUse(IUsable console) => false;

    public override bool DidWin(GameOverReason gameOverReason) => false;

    public override void AppendTaskHint(Il2CppSystem.Text.StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }

    public static void InitList()
    {
        foreach (var player in PlayerControl.AllPlayerControls)

        {
            if (!TrackedSpectators.Contains(player.Data.PlayerName))
            {
                TrackedPlayers.Add(player);
            }
        }
    }
}