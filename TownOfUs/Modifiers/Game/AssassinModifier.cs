﻿using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using TownOfUs.Modifiers.Game.Impostor;
using MiraAPI.PluginLoading;
using Il2CppSystem.Linq;
using TownOfUs.Roles.Neutral;
using MiraAPI.Roles;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Modifiers.Game;

[MiraIgnore]
public abstract class AssassinModifier : ExcludedGameModifier
{
    public override string ModifierName => "Assassin";
    public string LastGuessedItem { get; set; }
    public PlayerControl? LastAttemptedVictim { get; set; }

    public override int GetAssignmentChance() => 100;

    public override int GetAmountPerGame() => 0;

    public override int Priority() => 0;

    public override bool HideOnUi => true;

    private int maxKills;
    private MeetingMenu meetingMenu;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return false;
    }

    public override void OnActivate()
    {
        base.OnActivate();

        maxKills = (int)OptionGroupSingleton<AssassinOptions>.Instance.AssassinKills;

        //Logger<TownOfUsPlugin>.Error($"AssassinModifier.OnActivate maxKills: {maxKills}");
        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                Player.Data.Role,
                ClickGuess,
                MeetingAbilityType.Click,
                TouAssets.Guess,
                null!,
                IsExempt);
        }
    }

    public override void OnMeetingStart()
    {
        //Logger<TownOfUsPlugin>.Error($"AssassinModifier.OnMeetingStart maxKills: {maxKills}");
        if (Player.AmOwner)
        {
            meetingMenu.GenButtons(MeetingHud.Instance, Player.AmOwner && !Player.HasDied() && maxKills > 0 && !Player.HasModifier<JailedModifier>());
        }
    }

    public void OnVotingComplete()
    {
        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            meetingMenu?.Dispose();
            meetingMenu = null!;
        }
    }

    public void ClickGuess(PlayerVoteArea voteArea, MeetingHud meetingHud)
    {
        if (meetingHud.state == MeetingHud.VoteStates.Discussion)
        {
            return;
        }

        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId).Object;

        var shapeMenu = GuesserMenu.Create();
        shapeMenu.Begin(IsRoleValid, ClickRoleHandle, IsModifierValid, ClickModifierHandle);

        void ClickRoleHandle(RoleBehaviour role)
        {
            var realRole = player.Data.Role;

            var cachedMod = player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) as ICachedRole;
            if (cachedMod != null)
            {
                realRole = cachedMod.CachedRole;
            }

            var pickVictim = role.Role == realRole.Role;
            var victim = pickVictim ? player : Player;

            ClickHandler(victim);
            LastAttemptedVictim = player;
            LastGuessedItem = $"{role.TeamColor.ToTextColor()}{role.NiceName}</color>";
        }

        void ClickModifierHandle(BaseModifier modifier)
        {
            var pickVictim = player.HasModifier(modifier.TypeId);
            var victim = pickVictim ? player : Player;

            ClickHandler(victim);
            LastAttemptedVictim = player;
            LastGuessedItem = $"{MiscUtils.GetRoleColour(modifier.ModifierName.Replace(" ", string.Empty)).ToTextColor()}{modifier.ModifierName}</color>";
        }

        void ClickHandler(PlayerControl victim)
        {
            if (victim == Player && Player.TryGetModifier<DoubleShotModifier>(out var modifier) && !modifier.Used)
            {
                modifier!.Used = true;

                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Impostor));

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}Your Double Shot has prevented you from dying this meeting!</color></b>", Color.white, spr: TouModifierIcons.DoubleShot.LoadAsset());

                notif1.Text.SetOutlineThickness(0.35f);
                notif1.transform.localPosition = new Vector3(0f, 1f, -20f);

                shapeMenu.Close();
                LastGuessedItem = string.Empty;
                LastAttemptedVictim = null;

                return;
            }

            Player.RpcCustomMurder(victim, createDeadBody: false, teleportMurderer: false, showKillAnim: false, playKillSound: false);

            if (victim != Player)
            {
                LastGuessedItem = string.Empty;
                LastAttemptedVictim = null;
                MeetingMenu.Instances.Do(x => x.HideSingle(victim.PlayerId));
            }

            maxKills--;

            if (!OptionGroupSingleton<AssassinOptions>.Instance.AssassinMultiKill || maxKills == 0 || victim == Player)
            {
                meetingMenu?.HideButtons();
            }

            shapeMenu.Close();
        }
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea?.TargetPlayerId == Player.PlayerId ||
            Player.Data.IsDead ||
            voteArea!.AmDead ||
            Player.IsImpostor() && voteArea.GetPlayer()?.IsImpostor() == true && !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode ||
            Player.Data.Role is VampireRole && voteArea.GetPlayer()?.Data.Role is VampireRole ||
            voteArea.GetPlayer()?.Data.Role is MayorRole mayor && mayor.Revealed ||
            voteArea.GetPlayer() != null && voteArea.GetPlayer()?.Data.Role is SnitchRole snitch && snitch.CompletedAllTasks && SnitchRole.SnitchVisibilityFlag(voteArea.GetPlayer()!, true) ||
            Player.IsLover() && voteArea.GetPlayer()?.IsLover() == true ||
            voteArea.GetPlayer()?.HasModifier<JailedModifier>() == true;
    }

    private bool IsRoleValid(RoleBehaviour role)
    {
        if (role.IsDead)
        {
            return false;
        }

        var options = OptionGroupSingleton<AssassinOptions>.Instance;
        var touRole = role as ITownOfUsRole;
        var assassinRole = Player.Data.Role as ITownOfUsRole;
        var unguessableRole = role as IUnguessable;

        if (touRole is IGhostRole)
        {
            return false;
        }

        if (unguessableRole != null && !unguessableRole.IsGuessable)
        {
            return false;
        }

        if (touRole?.RoleAlignment == RoleAlignment.CrewmateInvestigative)
        {
            return options.AssassinGuessInvest;
        }

        if (role.IsCrewmate() && role is ICustomRole)
        {
            return true;
        }

        if (role.IsCrewmate() && OptionGroupSingleton<AssassinOptions>.Instance.AssassinCrewmateGuess)
        {
            return true;
        }

        if (role.IsImpostor() && OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessImpostors && assassinRole?.RoleAlignment is RoleAlignment.NeutralKilling or RoleAlignment.NeutralEvil)
        {
            return true;
        }

        if (touRole?.RoleAlignment == RoleAlignment.NeutralBenign)
        {
            return options.AssassinGuessNeutralBenign;
        }

        if (touRole?.RoleAlignment == RoleAlignment.NeutralEvil)
        {
            return options.AssassinGuessNeutralEvil;
        }

        if (touRole?.RoleAlignment == RoleAlignment.NeutralKilling)
        {
            return options.AssassinGuessNeutralKilling;
        }

        return false;
    }

    private static bool IsModifierValid(BaseModifier modifier)
    {
        var isValid = true;
        // This will remove modifiers that alter their chance/amount
        if ((modifier is TouGameModifier touMod && (touMod.CustomAmount <= 0 || touMod.CustomChance <= 0)) ||
            (modifier is AllianceGameModifier allyMod && (allyMod.CustomAmount <= 0 || allyMod.CustomChance <= 0)) ||
            (modifier is UniversalGameModifier uniMod && (uniMod.CustomAmount <= 0 || uniMod.CustomChance <= 0)))
        {
            isValid = false;
        }

        if (!isValid)
        {
            return false;
        }

        if (OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessAlliances && modifier is AllianceGameModifier)
        {
            return true;
        }

        if (!OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessCrewModifiers)
        {
            return false;
        }

        if (!OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessInvModifier && modifier is InvestigatorModifier)
        {
            return false;
        }

        if (!OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessSpyModifier && modifier is SpyModifier)
        {
            return false;
        }

        var crewMod = modifier as TouGameModifier;
        if (crewMod != null && crewMod.FactionType.ToDisplayString().Contains("Crew") && !crewMod.FactionType.ToDisplayString().Contains("Non"))
        {
            return true;
        }

        return false;
    }
}
