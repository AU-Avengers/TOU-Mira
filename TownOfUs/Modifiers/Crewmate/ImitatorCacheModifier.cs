using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Events;
using TownOfUs.Interfaces;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ImitatorCacheModifier : BaseModifier, ICachedRole, IContinuesGame
{
    public bool ContinuesGame =>
        !Player.HasDied() && Player.IsCrewmate() && (MiscUtils.NKillersAliveCount > 0 || MiscUtils.ImpAliveCount > 0) && MiscUtils.CrewKillersAliveCount == 0 && PlayerControl.AllPlayerControls.ToArray().Any(x =>
            x.Data.IsDead && x.GetRoleWhenAlive() is ITouCrewRole crewRole && crewRole.IsPowerCrew) &&
        Helpers.GetAlivePlayers().Count > 1;
    private MeetingMenu? _meetingMenu;
    private NetworkedPlayerInfo? _selectedPlr;
    public override string ModifierName => "Imitator";
    public override bool HideOnUi => true;
    public bool ShowCurrentRoleFirst => true;

    public bool Visible => Player.AmOwner || PlayerControl.LocalPlayer.HasDied() ||
                           GuardianAngelTouRole.GASeesRoleVisibilityFlag(Player);

    public CacheRoleGuess GuessMode => (CacheRoleGuess)OptionGroupSingleton<ImitatorOptions>.Instance.ImitatorGuess.Value;

    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ImitatorRole>());

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player.AmOwner)
        {
            _meetingMenu = new MeetingMenu(
                Player.Data.Role,
                Click,
                MeetingAbilityType.Toggle,
                TouAssets.ImitateSelectSprite,
                TouAssets.ImitateDeselectSprite,
                IsExempt,
                Color.white)
            {
                Position = new Vector3(-0.40f, 0f, -3f)
            };
        }
    }

    public override void OnMeetingStart()
    {
        if (!Player.IsCrewmate())
        {
            var text = "Removed Imitator Cache Modifier On Meeting Start";
            MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, text);

            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (Player.AmOwner)
        {
            // _selectedPlr = null;
            _meetingMenu!.GenButtons(MeetingHud.Instance,
                Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());
            if (_selectedPlr != null)
            {
                _meetingMenu!.Actives[_selectedPlr.PlayerId] = true;
            }
        }
    }

    public void OnVotingComplete()
    {
        if (Player.AmOwner)
        {
            _meetingMenu!.HideButtons();
        }
    }

    public override void OnDeactivate()
    {
        _selectedPlr = null;

        if (Player.AmOwner && _meetingMenu != null)
        {
            _meetingMenu?.Dispose();
            _meetingMenu = null!;
        }
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud __)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId);

        if (_selectedPlr == player)
        {
            _selectedPlr = null;
            _meetingMenu!.Actives[voteArea.TargetPlayerId] = false;
            return;
        }

        if (_selectedPlr != null)
        {
            _meetingMenu!.Actives[_selectedPlr.PlayerId] = false;
            _selectedPlr = null;
        }

        _meetingMenu!.Actives[voteArea.TargetPlayerId] = true;
        _selectedPlr = player;
    }

    private bool IsExempt(PlayerVoteArea voteArea)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId);
        var opts = OptionGroupSingleton<ImitatorOptions>.Instance;
        if (Player.Data.IsDead || player == null || player.Object == null || voteArea.TargetPlayerId == Player.PlayerId || player.Object.Data.Disconnected || !voteArea.AmDead)
        {
            return true;
        }
        var playerRole = player.Object.GetRoleWhenAlive();
        var playerRoleId = playerRole.Role;
        var crewRole = (playerRole is ICrewVariant crewVariant) ? crewVariant.CrewVariant : playerRole;
        var otherPlayersWithPowerRole = PlayerControl.AllPlayerControls.ToArray().Count(x => x.Data.Role.Role == playerRoleId && x != player.Object && !x.AmOwner) > 1;
        var otherImitatorsExist = Helpers.GetAlivePlayers()
            .Any(x => x.HasModifier<ImitatorCacheModifier>() && x.IsCrewmate() && !x.AmOwner);

        if (crewRole.IsCrewmate() &&
            player.Object.IsNeutral() &&
            MiscUtils.GetPotentialRoles().Contains(crewRole))
        {
            return !opts.ImitateNeutrals;
        }

        if (crewRole.IsCrewmate() &&
            player.Object.IsImpostor() &&
            MiscUtils.GetPotentialRoles().Contains(crewRole))
        {
            return !opts.ImitateImpostors;
        }

        if (!player.Object.IsCrewmate() || !playerRole.IsCrewmate())
        {
            return true;
        }
        
        if (playerRole is MayorRole || playerRole is PoliticianRole)
        {
            return true;
        }

        if (playerRole.GetRoleAlignment() is RoleAlignment.CrewmatePower && otherPlayersWithPowerRole && otherImitatorsExist)
        {
            return true;
        }

        if (playerRole is ImitatorRole || playerRole is SurvivorRole || playerRole.IsSimpleRole)
        {
            return !opts.ImitateBasicCrewmate;
        }

        return false;
    }

    public void UpdateRole()
    {
        if (!Player.IsCrewmate())
        {
            var text = "Removed Imitator Cache Modifier On Attempt To Update Role";
            MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, text);

            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (_selectedPlr == null || Player.HasDied() || !_selectedPlr.IsDead || _selectedPlr.Disconnected || _selectedPlr.Object == null)
        {
            _selectedPlr = null;
            if (Player == null || Player.IsRole<ImitatorRole>())
            {
                return;
            }

            Player.RpcChangeRole(RoleId.Get<ImitatorRole>(), false);
            if (Player.HasDied())
            {
                Player.RpcChangeRole((ushort)RoleTypes.CrewmateGhost, false);
            }
            return;
        }

        var roleWhenAlive = _selectedPlr.Object.GetRoleWhenAlive();
        if (roleWhenAlive is ICrewVariant crewType)
        {
            roleWhenAlive = crewType.CrewVariant;
        }

        // Only the imitator will see this!
        if (!_selectedPlr.Object.HasModifier<ImitatedRevealedModifier>())
        {
            _selectedPlr.Object.AddModifier<ImitatedRevealedModifier>(roleWhenAlive);
        }

        if (Player.Data.Role.GetType() != roleWhenAlive.GetType())
        {
            Player.RpcChangeRole((ushort)roleWhenAlive.Role, false);
        }
    }
}