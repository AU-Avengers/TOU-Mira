using System.Collections;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Random = System.Random;

namespace TownOfUs.Roles.Neutral;

public sealed class GuardianAngelTouRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable,
    IDoomable, IAssignableTargets, ICrewVariant
{
    public PlayerControl? Target { get; set; }
    public int Priority { get; set; } = 1;

    public void AssignTargets()
    {
        if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Error($"SelectGATargets");
        var evilTargetPercent = (int)OptionGroupSingleton<GuardianAngelOptions>.Instance.EvilTargetPercent;

        var gas = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => x.IsRole<GuardianAngelTouRole>() && !x.HasDied());

        foreach (var ga in gas)
        {
            var filtered = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => !x.IsRole<GuardianAngelTouRole>() && !x.HasDied() &&
                            !x.HasModifier<ExecutionerTargetModifier>() && !x.HasModifier<AllianceGameModifier>())
                .ToList();

            if (evilTargetPercent > 0f)
            {
                Random rnd = new();
                var chance = rnd.Next(1, 101);

                if (chance <= evilTargetPercent)
                {
                    filtered = [.. filtered.Where(x => x.IsImpostor() || x.Is(RoleAlignment.NeutralKilling))];
                }
            }
            else
            {
                filtered = [.. filtered.Where(x => x.Is(ModdedRoleTeams.Crewmate))];
            }

            filtered = [.. filtered.Where(x => !x.Is(RoleAlignment.NeutralEvil))];

            Random rndIndex = new();
            var randomTarget = filtered[rndIndex.Next(0, filtered.Count)];

            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Info($"Setting GA Target: {randomTarget.Data.PlayerName}");
            RpcSetGATarget(ga, randomTarget);
        }
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ClericRole>());
    public DoomableType DoomHintType => DoomableType.Protective;
    public string RoleName => TouLocale.Get(TouNames.GuardianAngel, "Guardian Angel");
    public string RoleDescription => TargetString();
    public string RoleLongDescription => TargetString();
    public Color RoleColor => TownOfUsColors.GuardianAngel;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.GuardianAngel,
        IntroSound = TouAudio.GuardianAngelSound,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public bool SetupIntroTeam(IntroCutscene instance,
        ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        if (Player != PlayerControl.LocalPlayer)
        {
            return true;
        }

        var gaTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();

        gaTeam.Add(PlayerControl.LocalPlayer);
        gaTeam.Add(Target!);

        yourTeam = gaTeam;

        return true;
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Protect",
            "Protect your target from getting killed.",
            TouNeutAssets.ProtectSprite)
    ];

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Neutral Benign that needs to protect their target (signified by <color=#B3FFFFFF>★</color>) from getting killed/ejected." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (TutorialManager.InstanceExists && Target == null && Player.AmOwner && Player.IsHost() &&
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            Coroutines.Start(SetTutorialTargets(this));
        }
    }

    private static IEnumerator SetTutorialTargets(GuardianAngelTouRole ga)
    {
        yield return new WaitForSeconds(0.01f);
        ga.AssignTargets();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (TutorialManager.InstanceExists && Player.AmOwner)
        {
            var players = ModifierUtils
                .GetPlayersWithModifier<GuardianAngelTargetModifier>([HideFromIl2Cpp](x) => x.OwnerId == Player.PlayerId)
                .ToList();
            players.Do(x => x.RpcRemoveModifier<GuardianAngelTargetModifier>());
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var gaMod = ModifierUtils.GetActiveModifiers<GuardianAngelTargetModifier>().FirstOrDefault(x => x.OwnerId == Player.PlayerId);
        if (gaMod == null)
        {
            return false;
        }
        return gaMod.Player.Data.Role.DidWin(gameOverReason) || gaMod.Player.GetModifiers<GameModifier>().Any(x => x.DidWin(gameOverReason) == true);
    }

    public static bool GASeesRoleVisibilityFlag(PlayerControl player)
    {
        var gaKnowsTargetRole = OptionGroupSingleton<GuardianAngelOptions>.Instance.GAKnowsTargetRole &&
                                PlayerControl.LocalPlayer.IsRole<GuardianAngelTouRole>() &&
                                PlayerControl.LocalPlayer.GetRole<GuardianAngelTouRole>()!.Target == player;

        return gaKnowsTargetRole;
    }

    public static bool GATargetSeesVisibilityFlag(PlayerControl player)
    {
        var gaTargetKnows =
            OptionGroupSingleton<GuardianAngelOptions>.Instance.ShowProtect is ProtectOptions.SelfAndGA &&
            player.HasModifier<GuardianAngelTargetModifier>();

        var gaKnowsTargetRole = PlayerControl.LocalPlayer.IsRole<GuardianAngelTouRole>() &&
                                PlayerControl.LocalPlayer.GetRole<GuardianAngelTouRole>()!.Target == player;

        return gaTargetKnows || gaKnowsTargetRole;
    }

    public void CheckTargetDeath(PlayerControl victim)
    {
        if (Player.HasDied())
        {
            return;
        }

        if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Error($"OnPlayerDeath '{victim.Data.PlayerName}'");
        if (Target == null || victim == Target)
        {
            var roleType = OptionGroupSingleton<GuardianAngelOptions>.Instance.OnTargetDeath switch
            {
                BecomeOptions.Crew => (ushort)RoleTypes.Crewmate,
                BecomeOptions.Jester => RoleId.Get<JesterRole>(),
                BecomeOptions.Survivor => RoleId.Get<SurvivorRole>(),
                BecomeOptions.Amnesiac => RoleId.Get<AmnesiacRole>(),
                BecomeOptions.Mercenary => RoleId.Get<MercenaryRole>(),
                _ => (ushort)RoleTypes.Crewmate
            };

            if (TownOfUsPlugin.IsDevBuild) Logger<TownOfUsPlugin>.Error($"OnPlayerDeath - ChangeRole: '{roleType}'");
            Player.ChangeRole(roleType);

            if ((roleType == RoleId.Get<JesterRole>() && OptionGroupSingleton<JesterOptions>.Instance.ScatterOn) ||
                (roleType == RoleId.Get<SurvivorRole>() && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn))
            {
                StartCoroutine(Effects.Lerp(0.2f,
                    new Action<float>(p => { Player.GetModifier<ScatterModifier>()?.OnRoundStart(); })));
            }
        }
    }

    private string TargetString()
    {
        if (!Target)
        {
            return "Protect Your Target With Your Life!";
        }

        return $"Protect {Target?.Data.PlayerName} With Your Life!";
    }

    [MethodRpc((uint)TownOfUsRpc.SetGATarget, SendImmediately = true)]
    public static void RpcSetGATarget(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not GuardianAngelTouRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcSetGATarget - Invalid guardian angel");
            return;
        }

        if (target == null)
        {
            return;
        }

        var role = player.GetRole<GuardianAngelTouRole>();

        if (role == null)
        {
            return;
        }

        // Logger<TownOfUsPlugin>.Message($"RpcSetGATarget - Target: '{target.Data.PlayerName}'");
        role.Target = target;

        target.AddModifier<GuardianAngelTargetModifier>(player.PlayerId);
    }
}