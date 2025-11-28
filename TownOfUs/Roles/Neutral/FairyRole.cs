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
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;
using Random = System.Random;

namespace TownOfUs.Roles.Neutral;

public sealed class FairyRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable,
    IDoomable, IAssignableTargets, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MirrorcasterRole>());
    public PlayerControl? Target { get; set; }
    public int Priority { get; set; } = 1;

    public void AssignTargets()
    {
        if (!OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment)
        {
            return;
        }

        var textlog = $"Selecting Fairy Targets.";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlog);

        var evilTargetPercent = (int)OptionGroupSingleton<FairyOptions>.Instance.EvilTargetPercent;

        var gas = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => x.IsRole<FairyRole>() && !x.HasDied());

        foreach (var ga in gas)
        {
            var filtered = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => !x.IsRole<FairyRole>() && !x.HasDied() &&
                            !x.HasModifier<ExecutionerTargetModifier>() && !x.HasModifier<AllianceGameModifier>() &&
                            !SpectatorRole.TrackedSpectators.Contains(x.Data.PlayerName))
                .ToList();

            if (evilTargetPercent > 0f)
            {
                Random rnd = new();
                var chance = rnd.Next(1, 101);

                if (chance <= evilTargetPercent)
                {
                    filtered = [.. filtered.Where(x => x.IsImpostorAligned() || x.Is(RoleAlignment.NeutralKilling))];
                }
            }
            else
            {
                filtered = [.. filtered.Where(x => x.Is(ModdedRoleTeams.Crewmate))];
            }

            filtered = [.. filtered.Where(x => !x.Is(RoleAlignment.NeutralEvil))];

            Random rndIndex = new();
            var randomTarget = filtered[rndIndex.Next(0, filtered.Count)];

            var textlogtarget = $"Setting Fairy Target: {randomTarget.Data.PlayerName}";
            MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Info, textlogtarget);

            RpcSetFairyTarget(ga, randomTarget);
        }
    }

    public DoomableType DoomHintType => DoomableType.Protective;
    public string LocaleKey => "Fairy";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TargetString(true);
    public string RoleLongDescription => TargetString();

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription")
                .Replace("<symbol>", "<color=#B3FFFFFF>★</color>") +
            MiscUtils.AppendOptionsText(GetType());
    }

    private static string _missingTargetDesc = TouLocale.GetParsed("TouRoleFairyIfNoTarget");
    private static string _targetDesc = TouLocale.GetParsed("TouRoleFairyTabDescription");

    private string TargetString(bool capitalize = false)
    {
        var desc = capitalize ? _missingTargetDesc.ToTitleCase() : _missingTargetDesc;
        if (Target && Target != null)
        {
            desc = capitalize ? _targetDesc.ToTitleCase().Replace("<Target>", "<target>") : _targetDesc;
            desc = desc.Replace("<target>", $"{Target.Data.PlayerName}");
        }

        return desc;
    }

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
        if (Target != null)
        {
            gaTeam.Add(Target);
        }

        yourTeam = gaTeam;

        return true;
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Protect", "Protect"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}ProtectWikiDescription"),
                    TouNeutAssets.ProtectSprite)
            };
        }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        _missingTargetDesc = TouLocale.GetParsed("TouRoleFairyIfNoTarget");
        _targetDesc = TouLocale.GetParsed("TouRoleFairyTabDescription");

        if (TutorialManager.InstanceExists && Target == null && Player.AmOwner && Player.IsHost() &&
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            Coroutines.Start(SetTutorialTargets(this));
        }
    }

    private static IEnumerator SetTutorialTargets(FairyRole ga)
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
                .GetPlayersWithModifier<
                    GuardianAngelTargetModifier>([HideFromIl2Cpp](x) => x.OwnerId == Player.PlayerId)
                .ToList();
            players.Do(x => x.RpcRemoveModifier<GuardianAngelTargetModifier>());
        }

        if (!Player.HasModifier<BasicGhostModifier>() && Player.HasDied())
        {
            Player.AddModifier<BasicGhostModifier>();
        }
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        var gaMod = ModifierUtils.GetActiveModifiers<GuardianAngelTargetModifier>()
            .FirstOrDefault(x => x.OwnerId == Player.PlayerId);
        if (gaMod == null)
        {
            return false;
        }

        return gaMod.Player.Data.Role.DidWin(gameOverReason) ||
               gaMod.Player.GetModifiers<GameModifier>().Any(x => x.DidWin(gameOverReason) == true);
    }

    public static bool FairySeesRoleVisibilityFlag(PlayerControl player)
    {
        var gaKnowsTargetRole = OptionGroupSingleton<FairyOptions>.Instance.FairyKnowsTargetRole &&
                                PlayerControl.LocalPlayer.IsRole<FairyRole>() &&
                                PlayerControl.LocalPlayer.GetRole<FairyRole>()!.Target == player;

        return gaKnowsTargetRole;
    }

    public static bool FairyTargetSeesVisibilityFlag(PlayerControl player)
    {
        var gaTargetKnows =
            OptionGroupSingleton<FairyOptions>.Instance.ShowProtect is ProtectOptions.SelfAndFairy &&
            player.HasModifier<GuardianAngelTargetModifier>();

        var gaKnowsTargetRole = PlayerControl.LocalPlayer.IsRole<FairyRole>() &&
                                PlayerControl.LocalPlayer.GetRole<FairyRole>()!.Target == player;

        return gaTargetKnows || gaKnowsTargetRole;
    }

    public void CheckTargetDeath(PlayerControl victim)
    {
        if (Player.HasDied())
        {
            return;
        }
        var textlogtarget = $"On Fairy Player Death: {victim.Data.PlayerName}";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Info, textlogtarget);

        if (Target == null || victim == Target)
        {
            var roleType = OptionGroupSingleton<FairyOptions>.Instance.OnTargetDeath switch
            {
                BecomeOptions.Crew => (ushort)RoleTypes.Crewmate,
                BecomeOptions.Jester => RoleId.Get<JesterRole>(),
                BecomeOptions.Survivor => RoleId.Get<SurvivorRole>(),
                BecomeOptions.Amnesiac => RoleId.Get<AmnesiacRole>(),
                BecomeOptions.Mercenary => RoleId.Get<MercenaryRole>(),
                _ => (ushort)RoleTypes.Crewmate
            };

            var textlogchange = $"On Fairy Player Death - Change Role: {roleType}";
            MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlogchange);

            Player.ChangeRole(roleType);

            if ((roleType == RoleId.Get<JesterRole>() && OptionGroupSingleton<JesterOptions>.Instance.ScatterOn) ||
                (roleType == RoleId.Get<SurvivorRole>() && OptionGroupSingleton<SurvivorOptions>.Instance.ScatterOn))
            {
                StartCoroutine(Effects.Lerp(0.2f,
                    new Action<float>(p => { Player.GetModifier<ScatterModifier>()?.OnRoundStart(); })));
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SetFairyTarget)]
    public static void RpcSetFairyTarget(PlayerControl player, PlayerControl target)
    {
        if (player.Data.Role is not FairyRole)
        {
            Error("RpcSetFairyTarget - Invalid guardian angel");
            return;
        }

        if (target == null)
        {
            return;
        }

        var role = player.GetRole<FairyRole>();

        if (role == null)
        {
            return;
        }

        // Message($"RpcSetFairyTarget - Target: '{target.Data.PlayerName}'");
        role.Target = target;

        target.AddModifier<GuardianAngelTargetModifier>(player.PlayerId);
    }
}