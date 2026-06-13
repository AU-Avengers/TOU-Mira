using System.Text;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.HnsGame;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Networking;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class VigilanteRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITouCrewRole, IWikiDiscoverable, IDoomable
{
    private MeetingMenu meetingMenu;

    public int MaxKills { get; set; }
    public int SafeShotsLeft { get; set; }
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string LocaleKey => "Vigilante";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public Color RoleColor => TownOfUsColors.Vigilante;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateKilling;
    public bool IsPowerCrew => MaxKills > 0; // Always disable end game checks with a vigilante running around

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Vigilante,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.ImpostorIntroSound
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        if (PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{TouLocale.GetParsed("TouRoleVigilanteEvilTabInfo")}");
        }

        if ((int)OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots > 0)
        {
            var newText = SafeShotsLeft == 0
                ? TouLocale.GetParsed("TouRoleVigilanteNoSafeShots")
                : TouLocale.GetParsed("TouRoleVigilanteSafeShotsLeft").Replace("<count>", SafeShotsLeft.ToString(TownOfUsPlugin.Culture));
            stringB.AppendLine(TownOfUsPlugin.Culture, $"{newText}");
        }

        return stringB;
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        MaxKills = (int)OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteKills;
        SafeShotsLeft = (int)OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots;
        if (Player.HasModifier<ImitatorCacheModifier>())
        {
            SafeShotsLeft = 0;
        }

        if (Player.AmOwner)
        {
            meetingMenu = new MeetingMenu(
                this,
                ClickGuess,
                MeetingAbilityType.Click,
                TouAssets.Guess,
                null!,
                IsExempt);
        }
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);

        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null)
        {
            meetingMenu.GenButtons(meeting,
                Player.AmOwner && !Player.HasDied() && MaxKills > 0 && !Player.HasModifier<JailedModifier>());
        }
    }

    public override void OnVotingComplete()
    {
        RoleBehaviourStubs.OnVotingComplete(this);

        if (Player.AmOwner)
        {
            meetingMenu.HideButtons();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (Player.AmOwner)
        {
            meetingMenu.Dispose();
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

            if (ClickHandler(victim) && victim == Player)
            {
                DeathHandlerModifier.RpcSetMisguessSummary(Player, player.PlayerId, (ushort)role.Role, true);
            }
        }

        void ClickModifierHandle(BaseModifier modifier)
        {
            var pickVictim = player.HasModifier(modifier.TypeId);
            var victim = pickVictim ? player : Player;

            if (ClickHandler(victim) && victim == Player)
            {
                DeathHandlerModifier.RpcSetMisguessSummary(Player, player.PlayerId, modifier.TypeId, false);
            }
        }

        bool ClickHandler(PlayerControl victim)
        {
            if (!OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteMultiKill || MaxKills == 0 ||
                victim == Player)
            {
                meetingMenu.HideButtons();
            }

            if (victim != Player && victim.TryGetModifier<OracleBlessedModifier>(out var oracleMod))
            {
                OracleRole.RpcOracleBlessNotify(PlayerControl.LocalPlayer, oracleMod.Oracle, victim);

                MeetingMenu.Instances.Do(x => x.HideSingle(victim.PlayerId));

                shapeMenu.Close();

                return false;
            }

            if (victim == Player && SafeShotsLeft != 0)
            {
                SafeShotsLeft--;
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Impostor));

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Vigilante.ToTextColor()}{TouLocale.GetParsed("TouRoleVigilanteMultiShotFeedback").Replace("<count>", SafeShotsLeft.ToString(TownOfUsPlugin.Culture))}</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Vigilante.LoadAsset());

                notif1.AdjustNotification();

                shapeMenu.Close();

                return false;
            }
            Player.RpcSpecialMurder(victim, MeetingCheck.ForMeeting, true, true, createDeadBody: false, teleportMurderer: false,
                showKillAnim: false,
                playKillSound: false,
                causeOfDeath: victim != Player ? "Guess" : "Misguess");

            if (victim != Player)
            {
                meetingMenu.HideSingle(victim.PlayerId);
            }

            shapeMenu.Close();
            return true;
        }
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea?.TargetPlayerId == Player.PlayerId || Player.Data.IsDead || voteArea!.AmDead ||
               voteArea.GetPlayer()?.HasModifier<JailedModifier>() == true ||
               (voteArea.GetPlayer()?.Data.Role is MayorRole mayor && mayor.Revealed) ||
               (Player.IsLover() && voteArea.GetPlayer()?.IsLover() == true);
    }

    private static bool IsRoleValid(RoleBehaviour role)
    {
        if (role.IsDead)
        {
            return false;
        }

        var options = OptionGroupSingleton<VigilanteOptions>.Instance;

        if (role is IUnguessable { IsGuessable: false })
        {
            return false;
        }

        if (role.IsCrewmate() && !(PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) &&
                                   !allyMod.GetsPunished))
        {
            return false;
        }

        var alignment = role.GetRoleAlignment();

        // If Vigilante is Egotist, then guessing investigative roles is based off assassin settings
        if (!OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessInvest.Value &&
            alignment == RoleAlignment.CrewmateInvestigative)
        {
            return false;
        }

        if (role.IsCrewmate())
        {
            return true;
        }

        if (role.IsImpostor())
        {
            return true;
        }

        if (alignment == RoleAlignment.NeutralBenign)
        {
            return options.VigilanteGuessNeutralBenign.Value;
        }

        if (alignment == RoleAlignment.NeutralEvil)
        {
            return options.VigilanteGuessNeutralEvil.Value;
        }

        if (alignment == RoleAlignment.NeutralKilling)
        {
            return options.VigilanteGuessNeutralKilling.Value;
        }

        if (alignment == RoleAlignment.NeutralOutlier)
        {
            return options.VigilanteGuessNeutralOutlier.Value;
        }

        return false;
    }

    private static bool IsModifierValid(BaseModifier modifier)
    {
        // This will remove modifiers that alter their chance/amount
        var isValid =
            !((modifier is TouGameModifier touMod && (touMod.CustomAmount <= 0 || touMod.CustomChance <= 0)) ||
              (modifier is AllianceGameModifier allyMod && (allyMod.CustomAmount <= 0 || allyMod.CustomChance <= 0)) ||
              (modifier is UniversalGameModifier uniMod && (uniMod.CustomAmount <= 0 || uniMod.CustomChance <= 0)
               || modifier is HnsGameModifier));

        if (!isValid)
        {
            return false;
        }

        if (modifier is TouGameModifier touMod3 && touMod3.HideFromGuessing)
        {
            return false;
        }

        if (OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteGuessAlliances &&
            modifier is AllianceGameModifier)
        {
            return true;
        }

        if (modifier is TouGameModifier impMod &&
            (impMod.FactionType.ToDisplayString().Contains("Imp") ||
             impMod.FactionType.ToDisplayString().Contains("Killer")) &&
            !impMod.FactionType.ToDisplayString().Contains("Non"))
        {
            return OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteGuessKillerMods;
        }

        return false;
    }
}