using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.HnsGame;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Roles.Other;
using UnityEngine;

namespace TownOfUs.Modifiers.Game;

public class AssassinModifier : TouGameModifier, IWikiDiscoverable
{
    public int maxKills;
    private MeetingMenu meetingMenu;
    public override string LocaleKey => "Assassin";
    public static bool HasDoubleShot => PlayerControl.LocalPlayer.HasModifier<DoubleShotModifier>();
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");
    public override bool PreventsOtherModifiers => false;
    public override bool AppearsInSummary => false;
    public override bool AppearsInIntro => !PlayerControl.LocalPlayer.GetModifiers<TouGameModifier>().Any(x => x != this && x.AppearsInIntro);
    public override bool HideFromGuessing => true;

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription") + MiscUtils.AppendOptionsText(GetType());
    }

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.Assassin;
    public override ModifierFaction FactionType => ModifierFaction.AssailantUtility;

    // YES this is scuffed, a better solution will be used at a later time
    public override bool ShowInFreeplay => false;
    public string LastGuessedItem { get; set; }
    public uint LastGuessedItemId { get; set; }
    public bool LastGuessedIsRole { get; set; }
    public PlayerControl? LastAttemptedVictim { get; set; }

    public override bool HideOnUi => !LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance.ShowBasicAssassinOnHud.Value || HasDoubleShot;

    public override int GetAssignmentChance()
    {
        return 0;
    }

    public override int GetAmountPerGame()
    {
        return 0;
    }

    public override int Priority()
    {
        return 0;
    }

    public override int CustomAmount =>
        (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins.Value +
        (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins.Value;

    public override int CustomChance
    {
        get
        {
            var opt = OptionGroupSingleton<AssassinOptions>.Instance;
            var impChance = (int)opt.ImpAssassinChance.Value;
            var neutChance = (int)opt.NeutAssassinChance.Value;
            if ((int)opt.NumberOfImpostorAssassins.Value > 0 && (int)opt.NumberOfNeutralAssassins.Value > 0)
            {
                return (impChance + neutChance) / 2;
            }

            if ((int)opt.NumberOfImpostorAssassins.Value > 0)
            {
                return impChance;
            }
            else if ((int)opt.NumberOfNeutralAssassins.Value > 0)
            {
                return neutChance;
            }

            return 0;
        }
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role is not SpectatorRole;
    }

    public override void OnActivate()
    {
        base.OnActivate();

        maxKills = (int)OptionGroupSingleton<AssassinOptions>.Instance.AssassinKills.Value;

        //Error($"AssassinModifier.OnActivate maxKills: {maxKills}");
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
        //Error($"AssassinModifier.OnMeetingStart maxKills: {maxKills}");
        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null)
        {
            meetingMenu.GenButtons(meeting,
                Player.AmOwner && !Player.HasDied() && maxKills > 0 && !Player.HasModifier<JailedModifier>());
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

        if (Minigame.Instance)
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

            var pickVictim = role.Role == realRole.Role;
            if (cachedMod != null)
            {
                switch (cachedMod.GuessMode)
                {
                    case CacheRoleGuess.ActiveRole:
                        // Checks for the role the player is at the moment
                        pickVictim = role.Role == realRole.Role;
                        break;
                    case CacheRoleGuess.CachedRole:
                        // Checks for the cached role itself (like Imitator or Traitor)
                        pickVictim = role.Role == cachedMod.CachedRole.Role;
                        break;
                    default:
                        // Checks if it's the cached or active role
                        pickVictim = role.Role == cachedMod.CachedRole.Role || role.Role == realRole.Role;
                        break;
                }
            }
            var victim = pickVictim ? player : Player;

            LastAttemptedVictim = player;
            LastGuessedItem = $"{role.TeamColor.ToTextColor()}{role.GetRoleName()}</color>";
            LastGuessedIsRole = true;
            LastGuessedItemId = (ushort)role.Role;

            if (ClickHandler(victim) && victim == Player)
            {
                DeathHandlerModifier.RpcSetMisguessSummary(Player, player.PlayerId, LastGuessedItemId, LastGuessedIsRole);
            }
        }

        void ClickModifierHandle(BaseModifier modifier)
        {
            var pickVictim = player.HasModifier(modifier.TypeId);
            var victim = pickVictim ? player : Player;

            LastAttemptedVictim = player;
            LastGuessedItem =
                $"{MiscUtils.GetRoleColour(modifier.ModifierName.Replace(" ", string.Empty)).ToTextColor()}{modifier.ModifierName}</color>";
            LastGuessedIsRole = false;
            LastGuessedItemId = modifier.TypeId;

            if (ClickHandler(victim) && victim == Player)
            {
                DeathHandlerModifier.RpcSetMisguessSummary(Player, player.PlayerId, LastGuessedItemId, LastGuessedIsRole);
            }
        }

        bool ClickHandler(PlayerControl victim)
        {
            if (victim.HasDied() || Player.HasDied())
            {
                return false;
            }

            if (victim != Player && victim.TryGetModifier<OracleBlessedModifier>(out var oracleMod))
            {
                OracleRole.RpcOracleBlessNotify(PlayerControl.LocalPlayer, oracleMod.Oracle, victim);

                MeetingMenu.Instances.Do(x => x.HideSingle(victim.PlayerId));

                shapeMenu.Close();
                LastGuessedItem = string.Empty;
                LastAttemptedVictim = null;

                return false;
            }

            if (victim == Player && Player.TryGetModifier<DoubleShotModifier>(out var modifier) && !modifier.Used)
            {
                modifier!.Used = true;

                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Impostor));

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.ImpSoft.ToTextColor()}Your Double Shot has prevented you from dying this meeting!</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouModifierIcons.DoubleShot.LoadAsset());

                notif1.AdjustNotification();

                shapeMenu.Close();
                LastGuessedItem = string.Empty;
                LastAttemptedVictim = null;

                return false;
            }
            Player.RpcSpecialMurder(victim, MeetingCheck.ForMeeting, true, true, createDeadBody: false, teleportMurderer: false,
                showKillAnim: false,
                playKillSound: false,
                causeOfDeath: victim != Player ? "Guess" : "Misguess");

            if (victim != Player)
            {
                LastGuessedItem = string.Empty;
                LastAttemptedVictim = null;
                MeetingMenu.Instances.Do(x => x.HideSingle(victim.PlayerId));
            }

            maxKills--;

            if (!OptionGroupSingleton<AssassinOptions>.Instance.AssassinMultiKill.Value || maxKills == 0 || victim == Player)
            {
                meetingMenu?.HideButtons();
            }

            shapeMenu.Close();
            return true;
        }
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        var votePlayer = voteArea.GetPlayer();
        return voteArea?.TargetPlayerId == Player.PlayerId ||
               Player.Data.IsDead ||
               voteArea!.AmDead ||
               (Player.IsImpostorAligned() && votePlayer?.IsImpostorAligned() == true &&
                !OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode) ||
               (Player.Data.Role is VampireRole && votePlayer?.Data.Role is VampireRole) ||
               (votePlayer?.Data.Role is MayorRole mayor && mayor.Revealed) ||
               votePlayer.IsRevealed() ||
               (Player.IsLover() && votePlayer?.IsLover() == true) ||
               votePlayer?.HasModifier<JailedModifier>() == true;
    }

    private bool IsRoleValid(RoleBehaviour role)
    {
        if (role.IsDead)
        {
            return false;
        }

        var options = OptionGroupSingleton<AssassinOptions>.Instance;

        if (role is IGhostRole)
        {
            return false;
        }

        if (role is IUnguessable { IsGuessable: false })
        {
            return false;
        }

        var alignment = role.GetRoleAlignment();

        if (alignment == RoleAlignment.GameOutlier)
        {
            return false;
        }

        if (alignment == RoleAlignment.CrewmateInvestigative)
        {
            return options.AssassinGuessInvest.Value;
        }

        if (role.IsCrewmate() && role is ICustomRole)
        {
            return true;
        }

        if (role.IsCrewmate() && OptionGroupSingleton<AssassinOptions>.Instance.AssassinCrewmateGuess.Value)
        {
            return true;
        }

        var assassinAlignment = Player.Data.Role.GetRoleAlignment();

        if (role.IsImpostor() && OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessImpostors.Value &&
            assassinAlignment is RoleAlignment.NeutralKilling or RoleAlignment.NeutralEvil)
        {
            return true;
        }

        if (alignment == RoleAlignment.NeutralBenign)
        {
            return options.AssassinGuessNeutralBenign.Value;
        }

        if (alignment == RoleAlignment.NeutralEvil)
        {
            return options.AssassinGuessNeutralEvil.Value;
        }

        if (alignment == RoleAlignment.NeutralKilling)
        {
            return options.AssassinGuessNeutralKilling.Value;
        }

        if (alignment == RoleAlignment.NeutralOutlier)
        {
            return options.AssassinGuessNeutralOutlier.Value;
        }

        return false;
    }

    private static bool IsModifierValid(BaseModifier modifier)
    {
        var isValid = true;
        // This will remove modifiers that alter their chance/amount
        if ((modifier is TouGameModifier touMod && (touMod.CustomAmount <= 0 || touMod.CustomChance <= 0)) ||
            (modifier is AllianceGameModifier allyMod && (allyMod.CustomAmount <= 0 || allyMod.CustomChance <= 0)) ||
            (modifier is UniversalGameModifier uniMod && (uniMod.CustomAmount <= 0 || uniMod.CustomChance <= 0))
            || modifier is HnsGameModifier)
        {
            isValid = false;
        }

        if (!isValid)
        {
            return false;
        }

        if (modifier is TouGameModifier touMod3 && touMod3.HideFromGuessing)
        {
            return false;
        }

        if (OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessAlliances.Value &&
            modifier is AllianceGameModifier)
        {
            return true;
        }

        if (OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessCrewModifiers.Value)
        {
            if (!OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessUtilityModifiers.Value &&
                modifier is TouGameModifier touMod2 && touMod2.FactionType == ModifierFaction.CrewmateUtility)
            {
                return false;
            }

            var crewMod = modifier as TouGameModifier;
            if (crewMod != null && crewMod.FactionType.ToDisplayString().Contains("Crew") &&
                !crewMod.FactionType.ToDisplayString().Contains("Non"))
            {
                return true;
            }
        }

        if (OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessNonCrewModifiers.Value && modifier is TouGameModifier)
        {
            return true;
        }

        return false;
    }
}