using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Events;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class CrewpostorModifier : AllianceGameModifier, IWikiDiscoverable, IContinuesGame, IAssignableTargets
{
    public bool ContinuesGame => !Player.HasDied() && Player.IsCrewmate() && !Helpers.GetAlivePlayers().Any(x => x.IsImpostor());
    public override string LocaleKey => "Crewpostor";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public override string Symbol => "*";
    public override float IntroSize => 4f;
    public override bool DoesTasks => false;
    public override bool GetsPunished => false;
    public override bool CrewContinuesGame => false;
    public override ModifierFaction FactionType => ModifierFaction.CrewmateAlliance;
    public override Color FreeplayFileColor => new Color32(220, 220, 220, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Telepath;

    public int Priority { get; set; } = 1;
    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public void AssignTargets()
    {
        if (!OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment)
        {
            return;
        }

        System.Random rnd = new();
        var chance = rnd.Next(1, 101);

        if (chance <=
            (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.CrewpostorChance)
        {
            var filtered = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => x.IsCrewmate() &&
                            !x.HasDied() &&
                            !x.HasModifier<AllianceGameModifier>() &&
                            !x.HasModifier<ExecutionerTargetModifier>()).ToList();

            if (filtered.Count == 0)
            {
                return;
            }

            var randomTarget = filtered[rnd.Next(0, filtered.Count)];

            randomTarget.RpcAddModifier<CrewpostorModifier>();
            var imps = Helpers.GetAlivePlayers().Where(x => x.IsImpostor()).ToList();
            if (OptionGroupSingleton<CrewpostorOptions>.Instance.CrewpostorReplacesImpostor.Value && imps.Count > 1)
            {
                var textlognotfound = $"Replacing an impostor with a crewmate. Impostors: {imps.Count}.";
                MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlognotfound);
                var discardedImp = imps.Where(x => x.Data.Role is not ISpawnChange).Random();
                var curAlignment = MiscUtils.GetRoleAlignment(discardedImp!.Data.Role);
                var crewAlignment = curAlignment switch
                {
                    RoleAlignment.ImpostorConcealing => RoleAlignment.CrewmateInvestigative,
                    RoleAlignment.ImpostorKilling => RoleAlignment.CrewmateKilling,
                    RoleAlignment.ImpostorPower => RoleAlignment.CrewmatePower,
                    _ => RoleAlignment.CrewmateSupport
                };
                var neutAlignment = curAlignment switch
                {
                    RoleAlignment.ImpostorConcealing => RoleAlignment.NeutralEvil,
                    RoleAlignment.ImpostorKilling => RoleAlignment.NeutralKilling,
                    RoleAlignment.ImpostorPower => RoleAlignment.NeutralOutlier,
                    _ => RoleAlignment.NeutralBenign
                };
                var randomInt = UnityEngine.Random.RandomRangeInt(0, 10);
                if (randomInt < 4)
                {
                    curAlignment = neutAlignment;
                }
                else
                {
                    curAlignment = crewAlignment;
                }

                var roles = MiscUtils.GetRegisteredRoles(curAlignment);

                var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                var roleOptions = currentGameOptions.RoleOptions;

                var assignmentData = roles.Where(x => !x.IsDead).Select(role =>
                    new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role),
                        roleOptions.GetChancePerGame(role.Role))).ToList();
                var assignmentDataUnique = roles
                    .Where(x => !x.IsDead && Helpers.GetAlivePlayers().All(y => y.Data.Role != x)).Select(role =>
                        new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role.Role),
                            roleOptions.GetChancePerGame(role.Role))).ToList();
                var checktext = $"Forcing {randomTarget.Data.PlayerName} into a crewmate/neutral role.";
                MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, checktext);
                if (assignmentDataUnique.Count == 0 && assignmentData.Count == 0)
                {
                    discardedImp.RpcSetRole(RoleTypes.Crewmate);
                    var newtext = $"Forcing {randomTarget.Data.PlayerName} into Crewmate.";
                    MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, newtext);
                }
                else
                {
                    var chosenRole = assignmentDataUnique.Count != 0
                        ? assignmentDataUnique.Random()!.Role
                        : assignmentData.Random()!.Role;

                    discardedImp.RpcSetRole(chosenRole.Role);
                    var newtext = $"Forcing {randomTarget.Data.PlayerName} into {chosenRole.GetRoleName()}.";
                    MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, newtext);
                }
            }
            else
            {
                var textlognotfound = $"Could not replace an impostor with a crewmate. | Can Replace: {OptionGroupSingleton<CrewpostorOptions>.Instance.CrewpostorReplacesImpostor.Value}, Enough Impostors: {imps.Count > 1}";
                MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlognotfound);
            }
        }
    }

    public override int GetAmountPerGame()
    {
        return 0;
    }

    public override int GetAssignmentChance()
    {
        return 0;
    }

    public override void OnActivate()
    {
        base.OnActivate();
        if (!Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
        }

        if (Player.HasModifier<ToBecomeTraitorModifier>())
        {
            Player.RemoveModifier<ToBecomeTraitorModifier>();
        }
    }

    public override int CustomAmount => (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.CrewpostorChance != 0 ? 1 : 0;
    public override int CustomChance => (int)OptionGroupSingleton<AllianceModifierOptions>.Instance.CrewpostorChance;

    public static bool CrewpostorVisibilityFlag(PlayerControl player)
    {
        var genOpt = OptionGroupSingleton<GeneralOptions>.Instance;
        var isImp = PlayerControl.LocalPlayer.IsImpostor() && genOpt.ImpsKnowRoles && !genOpt.FFAImpostorMode;

        return !player.HasModifier<TraitorCacheModifier>() && (player.AmOwner || player.Data != null && !player.Data.Disconnected && isImp);
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) && role.IsCrewmate();
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return reason is GameOverReason.ImpostorsByKill || reason is GameOverReason.ImpostorsBySabotage || reason is GameOverReason.ImpostorsByVote;
    }
}