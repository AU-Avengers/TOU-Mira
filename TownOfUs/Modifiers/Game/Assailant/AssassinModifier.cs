using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Assailant;

public class AssassinModifier : TouGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(
        TownOfUsColors.Assassin,
        TmpSpriteUtils.CreateSpriteAsset(TouModifierIcons.Assassin.LoadAsset(),
            "TouMira.Modifier.Assailant.Assassin", 1.45f));
    public override Color FreeplayFileColor => TownOfUsColors.Overclocker;
    public int maxKills;
    public int defaultKills;
    private MeetingMenu meetingMenu;
    public override string LocaleKey => "Assassin";
    public static bool HasDoubleShot => PlayerControl.LocalPlayer.HasModifier<DoubleShotModifier>();
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");
    public override bool PreventsOtherModifiers => false;
    public override bool AppearsInSummary => false;
    public override bool AppearsInIntro => !PlayerControl.LocalPlayer.GetModifiers<TouGameModifier>().Any(x => x != this && x.AppearsInIntro);
    public override bool HideFromGuessing => true;
    public bool IsImpostorAssassin => Player.IsImpostorAligned();

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
    public string LastGuessedItem { get; set; }
    public uint LastGuessedItemId { get; set; }
    public bool LastGuessedIsRole { get; set; }
    public PlayerControl? LastAttemptedVictim { get; set; }

    public override bool HideOnUi => !LocalSettingsTabSingleton<TouLocalTabButtons>.Instance.ShowBasicAssassinOnHud.Value || HasDoubleShot;

    public static int ImpostorAssassinAttempts;
    public static int NeutralAssassinAttempts;
    public static System.Random Rng;
    public override void BeforeModifierSpawns()
    {
        Rng = new System.Random();
        ImpostorAssassinAttempts = 0;
        NeutralAssassinAttempts = 0;
    }

    public static bool ModifierValidCheck(RoleBehaviour role, bool runChecks)
    {
        var opts = OptionGroupSingleton<AssassinOptions>.Instance;
        var neutCount = (int)opts.NumberOfNeutralAssassins.Value;
        var neutChance = Math.Clamp((int)opts.NeutAssassinChance.Value, 0, 100);
        var impCount = (int)opts.NumberOfImpostorAssassins.Value;
        var impChance = Math.Clamp((int)opts.ImpAssassinChance.Value, 0, 100);
        if ((!runChecks || NeutralAssassinAttempts < neutCount) && neutChance != 0 && role is ITownOfUsRole { RoleAlignment: RoleAlignment.NeutralKilling })
        {
            if (runChecks)
            {
                NeutralAssassinAttempts++;
            }
            if (Rng.Next(100) >= neutChance)
            {
                return false;
            }

            return true;
        }

        if ((!runChecks || ImpostorAssassinAttempts < impCount) && impChance != 0 && role.TeamType == RoleTeamTypes.Impostor)
        {
            if (runChecks)
            {
                ImpostorAssassinAttempts++;
            }
            if (Rng.Next(100) >= impChance)
            {
                return false;
            }

            return true;
        }

        return false;
    }
    public override bool IsModifierValidOnPostCheck(RoleBehaviour role)
    {
        return ModifierValidCheck(role, false);
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return ModifierValidCheck(role, true);
    }

    public override int GetAssignmentChance() => 100;
    public override int GetAmountPerGame() => CustomAmount;

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

    public override void OnActivate()
    {
        base.OnActivate();

        var opts = OptionGroupSingleton<AssassinOptions>.Instance;
        maxKills = IsImpostorAssassin ? (int)opts.ImpAssassinKills.Value : (int)opts.NeutAssassinKills.Value;
        defaultKills = maxKills;

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


            var pickVictim = role.Role == realRole.Role;
            if (player.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) is ICachedRole cachedMod)
            {
                pickVictim = cachedMod.GuessMode switch
                {
                    // Checks for the role the player is at the moment
                    CacheRoleGuess.ActiveRole => role.Role == realRole.Role,
                    // Checks for the cached role itself (like Imitator or Traitor)
                    CacheRoleGuess.CachedRole => role.Role == cachedMod.CachedRole.Role,
                    // Checks if it's the cached or active role
                    _ => role.Role == cachedMod.CachedRole.Role || role.Role == realRole.Role,
                };
            }
            var victim = pickVictim ? player : Player;

            LastAttemptedVictim = player;
            LastGuessedItem = $"{role.TeamColor.ToTextColor()}{role.GetRoleName()}</color>";
            LastGuessedIsRole = true;
            LastGuessedItemId = (ushort)role.Role;

            if (ClickHandler(victim) && victim == Player)
            {
                GameHistory.RpcSetMisguessSummary(Player, player.PlayerId, LastGuessedItemId, LastGuessedIsRole);
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
                GameHistory.RpcSetMisguessSummary(Player, player.PlayerId, LastGuessedItemId, LastGuessedIsRole);
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
            Player.RpcMeetingMurder(victim, MeetingAnimation.PlayerNameplateAnimation, CustomTouMurderRpcs.GetRandomMeetingAnim(DeathAnimType.Nameplate),
                causeOfDeath: victim != Player ? "Guess" : "Misguess");

            if (victim != Player)
            {
                LastGuessedItem = string.Empty;
                LastAttemptedVictim = null;
                MeetingMenu.Instances.Do(x => x.HideSingle(victim.PlayerId));
            }

            maxKills--;

            var opts = OptionGroupSingleton<AssassinOptions>.Instance;
            if ((!IsImpostorAssassin && !opts.NeutAssassinMultiKill.Value) || (IsImpostorAssassin && !opts.ImpAssassinMultiKill.Value) || maxKills == 0 || victim == Player)
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

        if (role is IUnguessableBasic { IsGuessable: false })
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
        // This will remove modifiers that alter their chance/amount
        if (modifier is TouBaseGameModifier touMod && (touMod.CustomAmount <= 0 || touMod.CustomChance <= 0))
        {
            return false;
        }

        return IsModifierGuessable(modifier);
    }

    public static bool IsModifierGuessable(BaseModifier baseModifier)
    {
        if (baseModifier is not TouBaseGameModifier modifier)
        {
            return false;
        }

        if (baseModifier is IUnguessableBasic { IsGuessable: false })
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

            if (modifier is TouGameModifier crewMod && crewMod.FactionType.ToDisplayString().Contains("Crew") &&
                !crewMod.FactionType.ToDisplayString().Contains("Non"))
            {
                return true;
            }
        }

        if (modifier is TouGameModifier touMod4 && touMod4.FactionType.ToDisplayString().Contains("Imp") && !touMod4.FactionType.ToDisplayString().Contains("Non"))
        {
            return OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessImpostorModifiers.Value;
        }

        if (OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessNonCrewModifiers.Value && modifier is TouGameModifier)
        {
            return true;
        }

        return false;
    }
}