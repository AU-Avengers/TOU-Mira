using System.Collections;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
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

public sealed class ExecutionerRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable,
    IAssignableTargets, ICrewVariant
{
    public PlayerControl? Target { get; set; }
    public bool TargetVoted { get; set; }
    public bool AboutToWin { get; set; }

    [HideFromIl2Cpp] public List<byte> Voters { get; set; } = [];

    public int Priority { get; set; } = 2;

    public void AssignTargets()
    {
        // Logger<TownOfUsPlugin>.Error($"SelectExeTargets");
        var exes = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => x.IsRole<ExecutionerRole>() && !x.HasDied());

        foreach (var exe in exes)
        {
            var filtered = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => !x.IsRole<ExecutionerRole>() && !x.HasDied() &&
                            x.Is(ModdedRoleTeams.Crewmate) &&
                            !x.HasModifier<GuardianAngelTargetModifier>() &&
                            !x.HasModifier<AllianceGameModifier>() &&
                            x.Data.Role is not SwapperRole &&
                            x.Data.Role is not ProsecutorRole &&
                            x.Data.Role is not PoliticianRole &&
                            x.Data.Role is not JailorRole &&
                            x.Data.Role is not VigilanteRole).ToList();

            if (filtered.Count > 0)
            {
                // filtered.ForEach(x => Logger<TownOfUsPlugin>.Error($"EXE Possible Target: {x.Data.PlayerName}"));
                Random rndIndex = new();
                var randomTarget = filtered[rndIndex.Next(0, filtered.Count)];

                RpcSetExeTarget(exe, randomTarget);
            }
            else
            {
                exe.GetRole<ExecutionerRole>()!.CheckTargetDeath(null);
            }
        }
    }

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SnitchRole>());
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string RoleName => TouLocale.Get(TouNames.Executioner, "Executioner");
    public string RoleDescription => TargetString();
    public string RoleLongDescription => TargetString();
    public Color RoleColor => TownOfUsColors.Executioner;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.DiscoveredSound,
        Icon = TouRoleIcons.Executioner,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public bool MetWinCon => TargetVoted;

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        return OptionGroupSingleton<ExecutionerOptions>.Instance.ExeWin is ExeWinOptions.EndsGame && TargetVoted;
    }

    public string GetAdvancedDescription()
    {
        return
            $"The {RoleName} is a Neutral Evil role that wins by getting their target (signified by <color=#643B1FFF>X</color>) ejected in a meeting." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (!OptionGroupSingleton<ExecutionerOptions>.Instance.CanButton)
        {
            player.RemainingEmergencies = 0;
        }

        // if Exe was revived Target will be null but their old target will still have the ExecutionerTargetModifier
        if (Target == null)
        {
            Target = ModifierUtils
                .GetPlayersWithModifier<ExecutionerTargetModifier>([HideFromIl2Cpp](x) => x.OwnerId == Player.PlayerId)
                .FirstOrDefault();
        }

        if (TutorialManager.InstanceExists && Target == null &&
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started && Player.AmOwner &&
            Player.IsHost())
        {
            Coroutines.Start(SetTutorialTargets(this));
        }
    }

    private static IEnumerator SetTutorialTargets(ExecutionerRole exe)
    {
        yield return new WaitForSeconds(0.01f);
        exe.AssignTargets();
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (TutorialManager.InstanceExists && Player.AmOwner)
        {
            var players = ModifierUtils
                .GetPlayersWithModifier<ExecutionerTargetModifier>([HideFromIl2Cpp](x) => x.OwnerId == Player.PlayerId)
                .ToList();
            players.Do(x => x.RpcRemoveModifier<ExecutionerTargetModifier>());
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);

        Target = null;
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
        return TargetVoted;
    }

    private string TargetString()
    {
        if (!Target)
        {
            return "Get your target voted out to win.";
        }

        return $"Get {Target?.Data.PlayerName} voted out to win.";
    }

    public void CheckTargetDeath(PlayerControl? victim)
    {
        if (Player.HasDied() || AboutToWin || TargetVoted)
        {
            return;
        }

        // Logger<TownOfUsPlugin>.Error($"OnPlayerDeath '{victim.Data.PlayerName}'");
        if (Target == null || victim == Target)
        {
            var roleType = OptionGroupSingleton<ExecutionerOptions>.Instance.OnTargetDeath switch
            {
                BecomeOptions.Crew => (ushort)RoleTypes.Crewmate,
                BecomeOptions.Jester => RoleId.Get<JesterRole>(),
                BecomeOptions.Survivor => RoleId.Get<SurvivorRole>(),
                BecomeOptions.Amnesiac => RoleId.Get<AmnesiacRole>(),
                BecomeOptions.Mercenary => RoleId.Get<MercenaryRole>(),
                _ => (ushort)RoleTypes.Crewmate
            };

            // Logger<TownOfUsPlugin>.Error($"OnPlayerDeath - ChangeRole: '{roleType}'");
            Player.ChangeRole(roleType);

            if ((roleType == RoleId.Get<JesterRole>() && OptionGroupSingleton<JesterOptions>.Instance.ScatterOn) ||
                (roleType == RoleId.Get<SurvivorRole>() && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn))
            {
                StartCoroutine(Effects.Lerp(0.2f,
                    new Action<float>(p => { Player.GetModifier<ScatterModifier>()?.OnRoundStart(); })));
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SetExeTarget, SendImmediately = true)]
    public static void RpcSetExeTarget(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not ExecutionerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcSetExeTarget - Invalid executioner");
            return;
        }

        if (target == null)
        {
            return;
        }

        var role = player.GetRole<ExecutionerRole>();

        if (role == null)
        {
            return;
        }

        // Logger<TownOfUsPlugin>.Message($"RpcSetExeTarget - Target: '{target.Data.PlayerName}'");
        role.Target = target;

        target.AddModifier<ExecutionerTargetModifier>(player.PlayerId);
    }
}