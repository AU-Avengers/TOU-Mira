using System.Collections;
using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class ScavengerRole(IntPtr cppPtr)
    : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public bool GameStarted { get; set; }
    public float TimeRemaining { get; set; }
    [HideFromIl2Cpp] public PlayerControl? Target { get; set; }
    public bool Scavenging { get; set; }

    [HideFromIl2Cpp]
    public bool IsModifierApplicable(BaseModifier modifier)
    {
        return modifier is not OverclockerModifier;
    }

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not ScavengerRole)
        {
            return;
        }

        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started &&
            !TutorialManager.InstanceExists)
        {
            return;
        }

        if (!Player.AmOwner)
        {
            return;
        }

        if (MeetingHud.Instance || ExileController.Instance)
        {
            Scavenging = false;
            GameStarted = false;
            return;
        }

        var killButton = CustomButtonSingleton<ScavengerKillButton>.Instance;

        if (!GameStarted && killButton.Timer > 0f)
        {
            GameStarted = true;
        }

        // scavenge mode starts once the kill button's cooldown reaches 0
        if (killButton.Timer <= 0f && !Scavenging && GameStarted && !Player.HasDied())
        {
            // Message($"Scavenge Begin");
            Scavenging = true;
            TimeRemaining = OptionGroupSingleton<ScavengerOptions>.Instance.ScavengeDuration;
            RefreshTarget();
        }

        if (TimeRemaining > 0)
        {
            TimeRemaining -= Time.deltaTime;
        }

        if ((TimeRemaining <= 0 || MeetingHud.Instance || Player.HasDied()) && Scavenging)
        {
            Clear();

            // Message($"Scavenge End");
            killButton.SetTimer(killButton.Cooldown);
        }
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<InvestigatorRole>());
    public DoomableType DoomHintType => DoomableType.Hunter;
    public string IdPart => "Scavenger";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Scavenger.LoadAsset(), "TouMira.Role.Impostor.Scavenger", 1.45f),
        Icon = TouRoleIcons.Scavenger,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        UseVanillaKillButton = false,
        IntroSound = TouAudio.WarlockIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        if (Target != null && Scavenging)
        {
            stringB.Append(TownOfUsPlugin.Culture, $"\n<b>{TimerString.Replace("<timeLeft>", TimeRemaining.ToString("0", TownOfUsPlugin.Culture))}</b>");
            stringB.Append(TownOfUsPlugin.Culture, $"\n<b>{TargetString.Replace("<player>", Target.Data.PlayerName)}</b>");
        }

        return stringB;
    }

    [HideFromIl2Cpp] public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        Clear();
    }

    public static string TimerString = MiraLocaleManager.Get("TownOfUsMira.Role.ScavengerTabTimer");
    public static string TargetString = MiraLocaleManager.Get("TownOfUsMira.Role.ScavengerTabTarget");
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        TimerString = MiraLocaleManager.Get("TownOfUsMira.Role.ScavengerTabTimer");
        TargetString = MiraLocaleManager.Get("TownOfUsMira.Role.ScavengerTabTarget");
        if (TutorialManager.InstanceExists && Target == null && Player.AmOwner)
        {
            Coroutines.Start(SetTutorialTarget(this, Player));
        }
    }

    private static IEnumerator SetTutorialTarget(ScavengerRole scav, PlayerControl player)
    {
        yield return new WaitForSeconds(0.01f);
        scav.GameStarted = true;
        scav.Scavenging = false;
        if (CustomButtonSingleton<ScavengerKillButton>.Instance.Timer <= 0f && !player.HasDied())
        {
            // Message($"Scavenge Begin");
            scav.Scavenging = true;
            scav.TimeRemaining = OptionGroupSingleton<ScavengerOptions>.Instance.ScavengeDuration;
            scav.RefreshTarget();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        TouRoleUtils.ClearTaskHeader(Player);
        Clear();
    }

    private PlayerControl? GetNextTarget()
    {
        PlayerControl? nextTarget = Player.GetClosestLivingPlayer(false, float.MaxValue, true,
            x => !x.HasModifier<FirstDeadShield>());

        if (Player.HasModifier<LoverModifier>())
        {
            nextTarget = Player.GetClosestLivingPlayer(false, float.MaxValue, true,
                x => !x.HasModifier<FirstDeadShield>() && !x.HasModifier<LoverModifier>());
        }

        return nextTarget;
    }

    private void RefreshTarget()
    {
        if (Target != null)
        {
            Target.RemoveModifier<ScavengerArrowModifier>();
        }

        Target = GetNextTarget();
        Target?.AddModifier<ScavengerArrowModifier>(Player, TownOfUsColors.Impostor);
    }

    public void Clear()
    {
        var players = ModifierUtils.GetPlayersWithModifier<ScavengerArrowModifier>();

        foreach (var player in players)
        {
            player.RemoveModifier<ScavengerArrowModifier>();
        }

        Scavenging = false;
        TimeRemaining = 0;
        Target = null;
    }

    public void OnPlayerKilled(PlayerControl victim)
    {
        if (!Player.AmOwner)
        {
            return;
        }

        var killButton = CustomButtonSingleton<ScavengerKillButton>.Instance;

        if (Target == null)
        {
            return;
        }

        if (victim == Target)
        {
            TimeRemaining = Mathf.Max(0f,
                TimeRemaining + OptionGroupSingleton<ScavengerOptions>.Instance.ScavengeIncreaseDuration);

            killButton.SetTimer(OptionGroupSingleton<ScavengerOptions>.Instance.ScavengeCorrectKillCooldown);

            RefreshTarget();
            return;
        }

        var baseCooldown = killButton.Cooldown > 0f ? killButton.Cooldown : PlayerControl.LocalPlayer.GetKillCooldown();
        killButton.SetTimer(baseCooldown * OptionGroupSingleton<ScavengerOptions>.Instance.ScavengeIncorrectKillCooldown);

        Clear();
    }
}