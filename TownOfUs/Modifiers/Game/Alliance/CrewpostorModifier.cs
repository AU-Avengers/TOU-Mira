using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
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
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Alliance;

public sealed class CrewpostorModifier : AllianceGameModifier, IWikiDiscoverable, IAssignableTargets
{
    public override string LocaleKey => "Crewpostor";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public string ShortName => TouLocale.Get($"TouModifier{LocaleKey}ShortName");
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
    public override AlliedFaction TrueFactionType => AlliedFaction.Impostor;

    public override bool CountTowardsTrueFaction =>
        OptionGroupSingleton<CrewpostorOptions>.Instance.CrewpostorReplacesImpostor.Value;
    public override Color FreeplayFileColor => new Color32(220, 220, 220, 255);
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Crewpostor;

    public int Priority { get; set; } = -1;
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
                            !SpectatorRole.TrackedSpectators.Contains(x.Data.PlayerName) &&
                            (x.Data.Role is not ILoyalCrewmate loyalCrew || loyalCrew.CanBeCrewpostor) &&
                            !x.HasModifier<AllianceGameModifier>() &&
                            !x.HasModifier<ExecutionerTargetModifier>()).ToList();

            if (filtered.Count == 0)
            {
                return;
            }

            var randomTarget = filtered[rnd.Next(0, filtered.Count)];

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

                var roles = MiscUtils.GetPotentialRoles().Where(x =>
                    x.GetRoleAlignment() == curAlignment &&
                    !Helpers.GetAlivePlayers().Any(y => y.Data.Role.Role == x.Role)).ToList();
                if (roles.Count < 2)
                {
                    roles = MiscUtils.GetPotentialRoles().Where(x =>
                        x.GetRoleAlignment() is RoleAlignment.CrewmateSupport or RoleAlignment.CrewmateProtective
                            or RoleAlignment.NeutralBenign &&
                        !Helpers.GetAlivePlayers().Any(y => y.Data.Role.Role == x.Role)).ToList();
                }

                if (roles.Count < 2)
                {
                    roles = MiscUtils.GetPotentialRoles().Where(x =>
                        x.GetRoleAlignment() is RoleAlignment.CrewmateInvestigative or RoleAlignment.NeutralEvil &&
                        !Helpers.GetAlivePlayers().Any(y => y.Data.Role.Role == x.Role)).ToList();
                }

                if (roles.Count < 2)
                {
                    roles = MiscUtils.GetPotentialRoles().Where(x =>
                        x.GetRoleAlignment() is RoleAlignment.CrewmateKilling or RoleAlignment.NeutralKilling &&
                        !Helpers.GetAlivePlayers().Any(y => y.Data.Role.Role == x.Role)).ToList();
                }

                if (roles.Count < 2)
                {
                    roles = MiscUtils.GetPotentialRoles().Where(x =>
                        x.GetRoleAlignment() is RoleAlignment.CrewmatePower or RoleAlignment.NeutralOutlier &&
                        !Helpers.GetAlivePlayers().Any(y => y.Data.Role.Role == x.Role)).ToList();
                }

                var checktext = $"Forcing {discardedImp.Data.PlayerName} into a crewmate/neutral role.";
                MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, checktext);
                var newRole = RoleTypes.Crewmate;
                if (roles.Count == 0)
                {
                    var newtext = $"Forcing {discardedImp.Data.PlayerName} into Crewmate.";
                    MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, newtext);
                }
                else
                {
                    var chosenRole = roles.Random()!;
                    newRole = chosenRole.Role;

                    var newtext = $"Forcing {discardedImp.Data.PlayerName} into {chosenRole.GetRoleName()}.";
                    MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, newtext);
                }
                RpcSetUpCrewpostor(randomTarget, discardedImp, newRole);
            }
            else
            {
                var textlognotfound =
                    $"Could not replace an impostor with a crewmate. | Can Replace: {OptionGroupSingleton<CrewpostorOptions>.Instance.CrewpostorReplacesImpostor.Value}, Enough Impostors: {imps.Count > 1}";
                MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, textlognotfound);
                RpcSetUpCrewpostor(randomTarget, randomTarget, RoleTypes.Crewmate);
            }
        }
    }

    [MethodRpc((uint)TownOfUsRpc.SetUpCrewpostor)]
    public static void RpcSetUpCrewpostor(PlayerControl newCrewpostor, PlayerControl discardedImp, RoleTypes newRole)
    {
        newCrewpostor.AddModifier<CrewpostorModifier>();
        if (newCrewpostor.PlayerId == discardedImp.PlayerId)
        {
            return;
        }
        foreach (var mod in discardedImp.GetModifiers<TouGameModifier>())
        {
            var faction = mod.FactionType.ToString();
            if (faction.Contains("Impostor") && !faction.Contains("Non"))
            {
                discardedImp.RemoveModifier(mod);
            }
        }

        if (AmongUsClient.Instance.AmHost)
        {
            discardedImp.RpcSetRole(newRole, true);
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
        if (Player.AmOwner)
        {
            // player is meant to be crewmate, and crewmates don't have a task header!
            TouRoleUtils.ClearTaskHeader(PlayerControl.LocalPlayer);
        }
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
        return base.IsModifierValidOn(role) && role.IsCrewmate() &&
               (role is not ILoyalCrewmate loyalCrew || loyalCrew.CanBeCrewpostor);
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return reason is GameOverReason.ImpostorsByKill || reason is GameOverReason.ImpostorsBySabotage ||
               reason is GameOverReason.ImpostorsByVote || reason is GameOverReason.CrewmateDisconnect ||
               reason is GameOverReason.HideAndSeek_ImpostorsByKills;
    }
}