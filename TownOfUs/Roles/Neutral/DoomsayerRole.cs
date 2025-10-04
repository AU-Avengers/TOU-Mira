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
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Neutral;

public sealed class DoomsayerRole(IntPtr cppPtr)
    : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    private MeetingMenu meetingMenu;

    public int NumberOfGuesses { get; set; }
    public int IncorrectGuesses { get; set; }
    public bool AllGuessesCorrect { get; set; }

    [HideFromIl2Cpp] public List<PlayerControl> AllVictims { get; } = [];

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<VigilanteRole>());
    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Doomsayer";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");

    public string RoleLongDescription =>
        TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription").Replace("<guessCount>",
            $"{(int)OptionGroupSingleton<DoomsayerOptions>.Instance.DoomsayerGuessesToWin}");

    public Color RoleColor => TownOfUsColors.Doomsayer;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.QuestionSound,
        Icon = TouRoleIcons.Doomsayer,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool MetWinCon => AllGuessesCorrect;

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        if (OptionGroupSingleton<DoomsayerOptions>.Instance.DoomWin is not DoomWinOptions.EndsGame)
        {
            return false;
        }

        return AllGuessesCorrect;
    }

    public string GetAdvancedDescription()
    {
        var opts = OptionGroupSingleton<DoomsayerOptions>.Instance;
        var shownDesc = TouLocale.GetParsed(opts.CantObserve
            ? "TouRoleDoomsayerWikiDescription"
            : "TouRoleDoomsayerWikiDescriptionIfCanObserve");
        return
            shownDesc.Replace("<guessCount>", $"{(int)opts.DoomsayerGuessesToWin}") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Observe", "Observe"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}ObserveWikiDescription"),
                    TouNeutAssets.Observe)
            };
        }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

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
                Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());

            if (OptionGroupSingleton<DoomsayerOptions>.Instance.DoomsayerGuessAllAtOnce)
            {
                NumberOfGuesses = 0;
            }

            IncorrectGuesses = 0;
            AllVictims.Clear();
            AllGuessesCorrect = false;
        }

        GenerateReport();
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

        if (!Player.HasModifier<BasicGhostModifier>() && AllGuessesCorrect)
        {
            Player.AddModifier<BasicGhostModifier>();
        }
    }

    private void GenerateReport()
    {
        Logger<TownOfUsPlugin>.Info($"Generating Doomsayer report");

        var reportBuilder = new StringBuilder();

        if (Player == null)
        {
            return;
        }

        if (!Player.AmOwner)
        {
            return;
        }

        foreach (var player in GameData.Instance.AllPlayers.ToArray()
                     .Where(x => !x.Object.HasDied() && x.Object.HasModifier<DoomsayerObservedModifier>()))
        {
            var role = player.Object.Data.Role;
            var doomableRole = role as IDoomable;
            var undoomableRole = role as IUnguessable;
            var hintType = DoomableType.Default;
            var cachedMod =
                player.Object.GetModifiers<BaseModifier>().FirstOrDefault(x => x is ICachedRole) as ICachedRole;
            if (cachedMod != null)
            {
                role = cachedMod.CachedRole;
                doomableRole = role as IDoomable;
            }

            if (undoomableRole != null)
            {
                role = undoomableRole.AppearAs;
                doomableRole = role as IDoomable;
            }

            if (doomableRole != null)
            {
                hintType = doomableRole.DoomHintType;
            }

            var fallback = TouLocale.GetParsed("TouRoleDoomsayerRoleHintDefault");
            var hint = TouLocale.GetParsed($"TouRoleDoomsayerRoleHint{hintType}");

            if (hint.Contains("STRMISS"))
            {
                reportBuilder.AppendLine(TownOfUsPlugin.Culture,
                    $"{fallback.Replace("<player>", player.PlayerName)}\n");
            }
            else
            {
                reportBuilder.AppendLine(TownOfUsPlugin.Culture, $"{hint.Replace("<player>", player.PlayerName)}\n");
            }

            var roles = RoleManager.Instance.AllRoles.ToArray()
                .Where(x => (x is IDoomable doomRole && doomRole.DoomHintType == DoomableType.Default &&
                    x is not IUnguessable || x is not IDoomable) && !x.IsDead).ToList();
            roles = roles.OrderBy(x => x.GetRoleName()).ToList();
            var lastRole = roles[roles.Count - 1];

            if (hintType != DoomableType.Default)
            {
                roles = MiscUtils.AllRoles
                    .Where(x => x is IDoomable doomRole && doomRole.DoomHintType == hintType && x is not IUnguessable)
                    .OrderBy(x => x.GetRoleName()).ToList();
                lastRole = roles[roles.Count - 1];
            }

            if (roles.Count != 0)
            {
                reportBuilder.Append(TownOfUsPlugin.Culture, $"(");
                foreach (var role2 in roles)
                {
                    if (role2 == lastRole)
                    {
                        reportBuilder.Append(TownOfUsPlugin.Culture,
                            $"#{lastRole.GetRoleName().ToLowerInvariant().Replace(" ", "-")})");
                    }
                    else
                    {
                        reportBuilder.Append(TownOfUsPlugin.Culture,
                            $"#{role2.GetRoleName().ToLowerInvariant().Replace(" ", "-")}, ");
                    }
                }
            }

            player.Object.RemoveModifier<DoomsayerObservedModifier>();
        }

        var report = reportBuilder.ToString();

        if (HudManager.Instance && report.Length > 0)
        {
            var title =
                $"<color=#{TownOfUsColors.Doomsayer.ToHtmlStringRGBA()}>{TouLocale.Get("TouRoleDoomsayerMessageTitle")}</color>";
            MiscUtils.AddFakeChat(Player.Data, title, report, false, true);
        }
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return AllGuessesCorrect;
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
        shapeMenu.Begin(IsRoleValid, ClickRoleHandle);

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

            ClickHandler(victim, voteArea.TargetPlayerId);
        }

        void ClickHandler(PlayerControl victim, byte targetId)
        {
            var opts = OptionGroupSingleton<DoomsayerOptions>.Instance;

            if (opts.DoomsayerGuessAllAtOnce)
            {
                NumberOfGuesses++;
            }

            meetingMenu?.HideSingle(targetId);

            var playersAlive = PlayerControl.AllPlayerControls.ToArray()
                .Count(x => !x.HasDied() && !x.IsJailed() && x != Player);

            if (victim == Player)
            {
                IncorrectGuesses++;
                if (!opts.DoomsayerGuessAllAtOnce)
                {
                    Coroutines.Start(MiscUtils.CoFlash(Color.red));
                    meetingMenu?.HideButtons();
                    shapeMenu.Close();
                    return;
                }
            }
            else if (!opts.DoomsayerGuessAllAtOnce)
            {
                Coroutines.Start(MiscUtils.CoFlash(Color.green));
                NumberOfGuesses++;
            }
            else
            {
                AllVictims.Add(victim);
            }

            if (((NumberOfGuesses < 2 && playersAlive < 3) ||
                 (NumberOfGuesses < (int)opts.DoomsayerGuessesToWin && playersAlive > 2)) &&
                opts.DoomsayerGuessAllAtOnce)
            {
                shapeMenu.Close();
                return;
            }

            if (IncorrectGuesses > 0 && opts.DoomsayerGuessAllAtOnce)
            {
                var text = NumberOfGuesses - AllVictims.Count == 1
                    ? $"<b>{TouLocale.GetParsed("TouRoleDoomsayerMisguessOne")}</b>"
                    : $"<b>{TouLocale.GetParsed("TouRoleDoomsayerMisguessMultiple").Replace("<misguessCount>", $"{NumberOfGuesses - AllVictims.Count}")}</b>";
                var notif1 = Helpers.CreateAndShowNotification(
                    text, Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Doomsayer.LoadAsset());

                notif1.AdjustNotification();

                Coroutines.Start(MiscUtils.CoFlash(Color.red));
            }
            else if (opts.DoomsayerGuessAllAtOnce)
            {
                if (opts.DoomsayerKillOnlyLast)
                {
                    if (victim != Player && victim.TryGetModifier<OracleBlessedModifier>(out var oracleMod))
                    {
                        OracleRole.RpcOracleBlessNotify(oracleMod.Oracle, PlayerControl.LocalPlayer, victim);
                    }
                    else
                    {
                        Player.RpcCustomMurder(victim, createDeadBody: false, teleportMurderer: false,
                            showKillAnim: false,
                            playKillSound: false);
                    }
                }
                else
                {
                    foreach (var victim2 in AllVictims)
                    {
                        if (victim2.TryGetModifier<OracleBlessedModifier>(out var oracleMod))
                        {
                            OracleRole.RpcOracleBlessNotify(oracleMod.Oracle, PlayerControl.LocalPlayer, victim2);
                        }
                        else
                        {
                            Player.RpcCustomMurder(victim2, createDeadBody: false, teleportMurderer: false,
                                showKillAnim: false,
                                playKillSound: false);
                        }
                    }
                }
            }
            else
            {
                if (victim != Player && victim.TryGetModifier<OracleBlessedModifier>(out var oracleMod))
                {
                    OracleRole.RpcOracleBlessNotify(oracleMod.Oracle, PlayerControl.LocalPlayer, victim);

                    MeetingMenu.Instances.Do(x => x.HideSingle(victim.PlayerId));

                    shapeMenu.Close();

                    return;
                }

                // no incorrect guesses so this should be the target not the Doomsayer
                Player.RpcCustomMurder(victim, createDeadBody: false, teleportMurderer: false, showKillAnim: false,
                    playKillSound: false);
            }

            if (opts.DoomsayerGuessAllAtOnce || NumberOfGuesses == (int)opts.DoomsayerGuessesToWin)
            {
                meetingMenu?.HideButtons();
            }

            shapeMenu.Close();
        }
    }

    public bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea.TargetPlayerId == Player.PlayerId ||
               Player.Data.IsDead || voteArea.AmDead ||
               voteArea.GetPlayer()?.HasModifier<JailedModifier>() == true ||
               (voteArea.GetPlayer()?.Data.Role is MayorRole mayor && mayor.Revealed) ||
               (Player.IsLover() && voteArea.GetPlayer()?.IsLover() == true);
    }

    private static bool IsRoleValid(RoleBehaviour role)
    {
        var unguessableRole = role as IUnguessable;
        if (role.IsDead || role is IGhostRole || (unguessableRole != null && !unguessableRole.IsGuessable))
        {
            return false;
        }

        if (role.GetRoleAlignment() == RoleAlignment.CrewmateInvestigative)
        {
            return OptionGroupSingleton<DoomsayerOptions>.Instance.DoomGuessInvest;
        }

        return true;
    }

    [MethodRpc((uint)TownOfUsRpc.DoomsayerWin)]
    public static void RpcDoomsayerWin(PlayerControl player)
    {
        if (player.Data.Role is not DoomsayerRole)
        {
            Logger<TownOfUsPlugin>.Error("RpcDoomsayerWin - Invalid Doomsayer");
            return;
        }

        var doom = player.GetRole<DoomsayerRole>();
        doom!.AllGuessesCorrect = true;

        if (GameHistory.PlayerStats.TryGetValue(player.PlayerId, out var stats))
        {
            stats.CorrectAssassinKills++;
        }
    }
}