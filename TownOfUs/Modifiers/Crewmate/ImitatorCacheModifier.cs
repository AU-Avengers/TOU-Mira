using AmongUs.GameOptions;
using HarmonyLib;
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
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class ImitatorCacheModifier : BaseModifier, ICachedRole, IContinuesGame
{
    public bool CanDisplayForRole(RoleBehaviour role)
    {
        return !role.IsDead && role.Role != CachedRole.Role;
    }

    public bool ContinuesGame =>
        !Player.HasDied() && Player.IsCrewmate() && (MiscUtils.NKillersAliveCount > 0 || MiscUtils.ImpAliveCount > 0) &&
        MiscUtils.CrewKillersAliveCount == 0 && PlayerControl.AllPlayerControls.Any(x =>
            x.Data.IsDead && x.GetRoleWhenAlive() is ITouCrewRole { IsPowerCrew: true }) &&
        Helpers.GetAlivePlayers().Count > 1;
    private MeetingMenu? _meetingMenu;
    private NetworkedPlayerInfo? _selectedPlr;
    public override string ModifierName => "Imitator";
    public string CachedRoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.ImitatorShortName");
    public override bool HideOnUi => true;
    public bool ShowCurrentRoleFirst => true;

    public bool Visible => Player.AmOwner || PlayerControl.LocalPlayer.HasDied() ||
                           FairyRole.FairySeesRoleVisibilityFlag(Player);

    public CacheRoleGuess GuessMode => OptionGroupSingleton<ImitatorOptions>.Instance.ImitatorGuess;

    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ImitatorRole>());

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player.AmOwner)
        {
            var classic = LegacyAssets.IsLegacy;
            _meetingMenu = MeetingMenu.CreateMenu(
                Player.Data.Role,
                Click,
                MeetingAbilityType.Toggle,
                classic ? LegacyAssets.ImitateSelectSprite : TouAssets.ImitateSelectSprite,
                classic ? LegacyAssets.ImitateDeselectSprite : TouAssets.ImitateDeselectSprite,
                IsExempt,
                Color.white,
                position: new Vector3(-0.40f, 0f, -3f),
                startActive: StartSelected);
        }
    }

    public bool StartSelected(PlayerVoteArea voteArea)
    {
        return voteArea.PlayerId == _selectedPlr?.PlayerId;
    }

    public override void OnMeetingStart()
    {
        if (Player.HasDied() && Player.AmOwner)
        {
            ModifierUtils.GetActiveModifiers<ImitatedRevealedModifier>().Do(x => x.Player.RemoveModifier(x));
        }
        if (!Player.IsCrewmate())
        {
            var text = "Removed Imitator Cache Modifier On Meeting Start";
            MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, text);

            RemoveSelf();
            return;
        }

        if (Player.AmOwner)
        {
            // _selectedPlr = null;
            _meetingMenu!.GenButtons(MeetingHud.Instance,
                Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());
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
            _meetingMenu?.Destroy();
            _meetingMenu = null!;
        }
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud __)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.PlayerId);

        if (_selectedPlr == player)
        {
            _selectedPlr = null;
            _meetingMenu!.Actives[voteArea.PlayerId] = false;
            return;
        }

        if (_selectedPlr != null)
        {
            _meetingMenu!.Actives[_selectedPlr.PlayerId] = false;
            _selectedPlr = null;
        }

        _meetingMenu!.Actives[voteArea.PlayerId] = true;
        _selectedPlr = player;
    }

    private bool IsExempt(PlayerVoteArea voteArea)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.PlayerId);
        var opts = OptionGroupSingleton<ImitatorOptions>.Instance;
        if (Player.Data.IsDead || player == null || player.Object == null || voteArea.PlayerId == Player.PlayerId || player.Object.Data.Disconnected || !voteArea.AmDead)
        {
            return true;
        }
        var playerRole = player.Object.GetRoleWhenAlive();
        var playerRoleId = playerRole.Role;
        var crewRole = playerRole is ICrewVariant { CrewVariant: var crewVariant } ? crewVariant : playerRole;
        var otherPlayersWithPowerRole = PlayerControl.AllPlayerControls.Count(x => x.Data.Role.Role == playerRoleId && x != player.Object && !x.AmOwner) > 1;
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

        if (playerRole is MayorRole || playerRole is PoliticianRole || playerRole is MonarchRole || playerRole is TimeLordRole)
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

            RemoveSelf();
            return;
        }

        if (Player.HasDied())
        {
            return;
        }

        if (_selectedPlr == null || !_selectedPlr.IsDead || _selectedPlr.Disconnected || _selectedPlr.Object == null)
        {
            _selectedPlr = null;
            if (!Player || Player.IsRole<ImitatorRole>())
            {
                return;
            }

            Player.RpcChangeRole(RoleId.Get<ImitatorRole>(), false);
            return;
        }

        var roleWhenAlive = _selectedPlr.Object.GetRoleWhenAlive();
        if (roleWhenAlive is ICrewVariant { CrewVariant: var crewVariant })
        {
            roleWhenAlive = crewVariant;
        }

        if (roleWhenAlive is ImitatorRole || roleWhenAlive is SurvivorRole || roleWhenAlive.IsSimpleRole)
        {
            roleWhenAlive = RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<ImitatorRole>());
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