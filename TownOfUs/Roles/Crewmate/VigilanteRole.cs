using System.Globalization;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
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
    public bool IsPowerCrew => MaxKills > 0; // Always disable end game checks with a vigi running around

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Vigilante,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Impostor)
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);
        if (PlayerControl.LocalPlayer.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
        {
            stringB.AppendLine(CultureInfo.InvariantCulture, $"You can also guess Crewmates.");
        }

        if ((int)OptionGroupSingleton<VigilanteOptions>.Instance.MultiShots > 0)
        {
            var newText = SafeShotsLeft == 0
                ? "You have no more safe shots left."
                : $"{SafeShotsLeft} safe shot(s) left.";
            stringB.AppendLine(CultureInfo.InvariantCulture, $"{newText}");
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

        if (Player.AmOwner)
        {
            meetingMenu.GenButtons(MeetingHud.Instance,
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

        if (Minigame.Instance != null)
        {
            return;
        }

        var player = GameData.Instance.GetPlayerById(voteArea.TargetPlayerId).Object;

        var shapeMenu = GuesserMenu.Create();
        shapeMenu.Begin(IsRoleValid, ClickRoleHandle, IsModifierValid, ClickModifierHandle);

        void ClickRoleHandle(RoleBehaviour role)
        {
            var pickVictim = role.Role == player.Data.Role.Role;

            if (player.IsImpostor())
            {
                if (role.Role == player.Data.Role.Role && !player.HasModifier<TraitorCacheModifier>())
                {
                    pickVictim = true;
                }
                else if (role is TraitorRole && player.HasModifier<TraitorCacheModifier>())
                {
                    pickVictim = true;
                }
                else
                {
                    pickVictim = false;
                }
            }

            var victim = pickVictim ? player : Player;

            ClickHandler(victim);
        }

        void ClickModifierHandle(BaseModifier modifier)
        {
            var pickVictim = player.HasModifier(modifier.TypeId);
            var victim = pickVictim ? player : Player;

            ClickHandler(victim);
        }

        void ClickHandler(PlayerControl victim)
        {
            if (!OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteMultiKill || MaxKills == 0 ||
                victim == Player)
            {
                meetingMenu?.HideButtons();
            }

            if (victim != Player && victim.TryGetModifier<OracleBlessedModifier>(out var oracleMod))
            {
                OracleRole.RpcOracleBlessNotify(oracleMod.Oracle, PlayerControl.LocalPlayer, victim);

                MeetingMenu.Instances.Do(x => x.HideSingle(victim.PlayerId));

                shapeMenu.Close();

                return;
            }

            if (victim == Player && SafeShotsLeft != 0)
            {
                SafeShotsLeft--;
                Coroutines.Start(MiscUtils.CoFlash(TownOfUsColors.Impostor));

                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{TownOfUsColors.Vigilante.ToTextColor()}Your Multi Shot has prevented you from dying this meeting! You have {SafeShotsLeft} safe shot(s) left!</color></b>",
                    Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Vigilante.LoadAsset());

                notif1.AdjustNotification();

                shapeMenu.Close();

                return;
            }

            Player.RpcCustomMurder(victim, createDeadBody: false, teleportMurderer: false, showKillAnim: false,
                playKillSound: false);

            if (victim != Player)
            {
                meetingMenu?.HideSingle(victim.PlayerId);
                DeathHandlerModifier.RpcUpdateDeathHandler(victim, TouLocale.Get("DiedToGuess"),
                    DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse,
                    TouLocale.GetParsed("DiedByStringBasic").Replace("<player>", Player.Data.PlayerName),
                    lockInfo: DeathHandlerOverride.SetTrue);
            }
            else
            {
                DeathHandlerModifier.RpcUpdateDeathHandler(victim, TouLocale.Get("DiedToMisguess"),
                    DeathEventHandlers.CurrentRound, DeathHandlerOverride.SetFalse,
                    lockInfo: DeathHandlerOverride.SetTrue);
            }

            shapeMenu.Close();
        }
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea?.TargetPlayerId == Player.PlayerId ||
               Player.Data.IsDead || voteArea!.AmDead ||
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
        if (!OptionGroupSingleton<AssassinOptions>.Instance.AssassinGuessInvest &&
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
            return options.VigilanteGuessNeutralBenign;
        }

        if (alignment == RoleAlignment.NeutralEvil)
        {
            return options.VigilanteGuessNeutralEvil;
        }

        if (alignment == RoleAlignment.NeutralKilling)
        {
            return options.VigilanteGuessNeutralKilling;
        }

        if (alignment == RoleAlignment.NeutralOutlier)
        {
            return options.VigilanteGuessNeutralOutlier;
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

        if (OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteGuessAlliances &&
            modifier is AllianceGameModifier)
        {
            return true;
        }

        var impMod = modifier as TouGameModifier;
        if (impMod != null &&
            (impMod.FactionType.ToDisplayString().Contains("Imp") ||
             impMod.FactionType.ToDisplayString().Contains("Killer")) &&
            !impMod.FactionType.ToDisplayString().Contains("Non"))
        {
            return OptionGroupSingleton<VigilanteOptions>.Instance.VigilanteGuessKillerMods;
        }

        return false;
    }
}