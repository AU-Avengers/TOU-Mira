using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Hud;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using Random = System.Random;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ToBecomeTraitorModifier : ExcludedGameModifier, IAssignableTargets, IContinuesGame
{
    public bool ContinuesGame => !Player.HasDied() && Helpers.GetAlivePlayers().Count >
                                 (int)OptionGroupSingleton<TraitorOptions>.Instance.LatestSpawn - 1;
    public override string ModifierName => "Possible Traitor";
    public override bool HideOnUi => true;

    public int Priority { get; set; } = 3;
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public void AssignTargets()
    {
        if (!OptionGroupSingleton<RoleOptions>.Instance.IsClassicRoleAssignment)
        {
            return;
        }

        if (GameOptionsManager.Instance.CurrentGameOptions.RoleOptions
                .GetNumPerGame((RoleTypes)RoleId.Get<TraitorRole>()) == 0 || ModifierUtils.GetActiveModifiers<CrewpostorModifier>().Any())
        {
            return;
        }

        Random rnd = new();
        var chance = rnd.Next(1, 101);

        if (chance <=
            GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(
                (RoleTypes)RoleId.Get<TraitorRole>()))
        {
            var filtered = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => x.IsCrewmate() &&
                            !x.HasDied() &&
                            !x.HasModifier<ExecutionerTargetModifier>() &&
                            !x.HasModifier<EgotistModifier>() &&
                            x.Data.Role is not MayorRole).ToList();

            if (filtered.Count == 0)
            {
                return;
            }

            var randomTarget = filtered[rnd.Next(0, filtered.Count)];

            randomTarget.RpcAddModifier<ToBecomeTraitorModifier>();
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

    public void Clear()
    {
        AssignTargets();
        ModifierComponent?.RemoveModifier(this);
    }

    [MethodRpc((uint)TownOfUsRpc.SetTraitor)]
    public static void RpcSetTraitor(PlayerControl player)
    {
        if (!player.HasModifier<ToBecomeTraitorModifier>() && !player.HasModifier<CrewpostorModifier>())
        {
            return;
        }

        player.ChangeRole(RoleId.Get<TraitorRole>());
        if (player.HasModifier<ToBecomeTraitorModifier>())
        {
            player.RemoveModifier<ToBecomeTraitorModifier>();
        }

        if (OptionGroupSingleton<AssassinOptions>.Instance.TraitorCanAssassin)
        {
            player.AddModifier<ImpostorAssassinModifier>();
        }

        CustomRoleUtils.GetActiveRolesOfType<SnitchRole>().ToList()
            .ForEach(snitch => snitch.AddSnitchTraitorArrows());

        if (player.AmOwner)
        {
            ButtonResetPatches.ResetCooldowns();
            player.SetKillTimer(player.GetKillCooldown());
        }
    }
}