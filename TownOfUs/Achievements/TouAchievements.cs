using System.Reflection;
using System.Runtime.CompilerServices;
using MiraAPI.Utilities;
using TownOfUs.Assets;
using UnityEngine;

namespace TownOfUs.Achievements;

/// <summary>
/// Town of Us's achievement tabs. These are plain data; the concrete SoftAchievementsTab
/// objects are built by the gated bridge when AchievementsAPI is present.
/// </summary>
internal static class TouAchievementGroups
{
    private static readonly Assembly TouAssembly = typeof(TownOfUsPlugin).Assembly;

    private static readonly AchievementArt TouArt = new(
        MenuBg: TouAchAssets.AchievementBox,
        ToastBg: TouAchAssets.AchievementToast,
        MenuIconOffset: new Vector3(10f, -10f),
        MenuTitleOffset: new Vector3(10f, -10f),
        MenuDescOffset: new Vector3(10f, -10f),
        MenuSubIconOffset: new Vector3(-40f, -70f),
        MenuSubIconScale: new Vector3(0.6f, 0.6f, 1f),
        ToastIconOffset: new Vector3(10f, -10f));

    public static readonly AchievementTabSpec General = new(
        TouAssembly, "Town of Us Mira: General", "Town of Us", "general",
        TabLocaleKey: "TownOfUsMira.Achievements.Tab.General",
        LocalePrefix: "TownOfUsMira.Achievement",
        TabIcon: TouRoleIcons.Mayor, ModIcon: TouAssets.TouMiraIcon,
        TabColor: TownOfUsColors.Politician, Art: TouArt);

    public static readonly AchievementTabSpec Crew = new(
        TouAssembly, "Town of Us Mira: Crewmate Roles", "Town of Us", "crewmate",
        TabLocaleKey: "TownOfUsMira.Achievements.Tab.Crewmate",
        LocalePrefix: "TownOfUsMira.Achievement",
        TabIcon: TouRoleIcons.Crewmate, ModIcon: TouAssets.TouMiraIcon,
        TabColor: Palette.CrewmateRoleHeaderBlue, Art: TouArt);

    public static readonly AchievementTabSpec Imp = new(
        TouAssembly, "Town of Us Mira: Impostor Roles", "Town of Us", "impostor",
        TabLocaleKey: "TownOfUsMira.Achievements.Tab.Impostor",
        LocalePrefix: "TownOfUsMira.Achievement",
        TabIcon: TouRoleIcons.Impostor, ModIcon: TouAssets.TouMiraIcon,
        TabColor: Palette.ImpostorRoleHeaderRed, Art: TouArt);

    public static readonly AchievementTabSpec Neut = new(
        TouAssembly, "Town of Us Mira: Neutral Roles", "Town of Us", "neutral",
        TabLocaleKey: "TownOfUsMira.Achievements.Tab.Neutral",
        LocalePrefix: "TownOfUsMira.Achievement",
        TabIcon: TouRoleIcons.Neutral, ModIcon: TouAssets.TouMiraIcon,
        TabColor: TownOfUsColors.Neutral, Art: TouArt);
}

/// <summary>
/// All Town of Us achievements as data. The <see cref="Key"/> matches both the old tab property
/// names (so locale keys are unchanged) and the static member names via
/// <see cref="CallerMemberNameAttribute"/>.
/// </summary>
public static class TouAchievements
{
    private static AchievementSpec Define(string name, string description, LoadableAsset<Sprite> icon,
        LoadableAsset<Sprite>? subIcon = null, string? subIconTooltip = null, int rarity = 0,
        bool hidden = false, int progressGoal = 0,
        AchievementPersistence persistence = AchievementPersistence.ThroughoutSessions,
        bool toastOnGameEnd = false, string? categoryOverride = null, string? idOverride = null,
        bool hideProgress = false, string[]? tags = null, [CallerMemberName] string key = "")
        => new(key, name, description, icon, subIcon, rarity, hidden, progressGoal, persistence,
            toastOnGameEnd, categoryOverride, idOverride, HideProgress: hideProgress, Tags: tags,
            SubIconTooltip: subIconTooltip);

    // General
    public static readonly AchievementSpec MiraMonday = Define("Mira Monday",
        "Play on Mira HQ on a Monday!", TouAssets.IconMira, tags: new[] { "Map", "MiraHQ" });
    public static readonly AchievementSpec FungleFriday = Define("Fungle Friday",
        "Play on Fungle on a Friday!", TouAssets.IconFungle, tags: new[] { "Map", "Fungle" });
    public static readonly AchievementSpec SubmergedSaturday = Define("Submerged Saturday",
        "Play on Submerged on a Saturday!", TouAssets.IconSubmerged, tags: new[] { "Map", "Submerged" });
    public static readonly AchievementSpec SkeldSunday = Define("Skeld Sunday",
        "Play on Skeld on a Sunday!", TouAssets.IconSkeld, tags: new[] { "Map", "Skeld" });
    public static readonly AchievementSpec Cheater = Define("Cheater",
        "Use /up to give yourself a role in the lobby.", TouRoleIcons.Impostor, hidden: true, tags: new[] { "Hidden", "Host", "Cheat" });
    public static readonly AchievementSpec Silence = Define("Silence",
        "Force end a meeting as host.", TouRoleIcons.Prosecutor, hidden: true, tags: new[] { "Hidden", "Host", "Meeting" });
    public static readonly AchievementSpec Oops = Define("Oops",
        "Abort a game as host.", TouRoleIcons.Mayor, hidden: true, tags: new[] { "Hidden", "Host", "Abort" });
    public static readonly AchievementSpec IveSeenThePlays = Define("I've seen the plays",
        "End your own life before they can.",
        TouRoleIcons.Jester, rarity: 1, hidden: true, tags: new[] { "Hidden", "Jester", "Meme" });
    public static readonly AchievementSpec GoBuyALotteryTicket = Define("Go buy a lottery ticket",
        "Win a 1 in 292,000,000 roll. Seriously, go buy a lottery ticket.", TouAchAssets.LuckyDice,
        rarity: 3, hidden: true, tags: new[] { "Hidden", "Luck", "Meme" });
    public static readonly AchievementSpec NoYou = Define("No You",
        "Serve a drink to the player who served you one in the same round.", TouCrewAssets.RoleblockSprite,
        rarity: 1, tags: new[] { "Barkeeper", "Bootlegger", "Roleblock", "Drink" });
    public static readonly AchievementSpec Endgame = Define("Endgame",
        "Witness a 1v1 between a Juggernaut and Pestilence.", TouNeutAssets.JuggKillSprite,
        rarity: 2, tags: new[] { "Juggernaut", "Pestilence", "Horseman", "1v1" });
    public static readonly AchievementSpec ButItRefused = Define("But it refused.",
        "Get revived alongside your lover.", TouCrewAssets.ReviveSprite, TouModifierIcons.Lover,
        subIconTooltip: "TownOfUsMira.Modifier.Lover", rarity: 2, tags: new[] { "Lover", "Revive", "Modifier" });
    public static readonly AchievementSpec YoureSafeWithMe = Define("You're Safe with Me",
        "Protect your lover from an attack.", TouCrewAssets.MedicSprite, TouModifierIcons.Lover,
        subIconTooltip: "TownOfUsMira.Modifier.Lover", rarity: 1, tags: new[] { "Lover", "Protect", "Modifier" });

    public static readonly AchievementSpec[] General =
    [
        MiraMonday, FungleFriday, SubmergedSaturday, SkeldSunday, Cheater, Silence, Oops, IveSeenThePlays,
        GoBuyALotteryTicket, NoYou, Endgame, ButItRefused, YoureSafeWithMe
    ];

    // Crewmate roles
    public static readonly AchievementSpec ForceBeWithYou = Define("The Force Be With You",
        "Sense 5 abilities being used in your radius throughout a game.", TouRoleIcons.Aurial, TouRoleIcons.Aurial, subIconTooltip: "TownOfUsMira.Role.Aurial",
        rarity: 1, progressGoal: 5, persistence: AchievementPersistence.ThroughoutRounds, tags: new[] { "Aurial", "Ability", "Sense" });
    public static readonly AchievementSpec StrangerDanger = Define("Stranger Danger",
        "Sense a kill happen within your uncolored radius.", TouRoleIcons.Aurial, TouRoleIcons.Aurial, subIconTooltip: "TownOfUsMira.Role.Aurial", toastOnGameEnd: true, tags: new[] { "Aurial", "Kill", "Sense" });
    public static readonly AchievementSpec BloodyHands = Define("Bloody Hands",
        "Find a suspect on the first Examine attempt.", TouAchAssets.BloodyHands, TouRoleIcons.Forensic, subIconTooltip: "TownOfUsMira.Role.Forensic", rarity: 1, tags: new[] { "Forensic", "Investigate", "Body" });
    public static readonly AchievementSpec RightOnTime = Define("Right on Time",
        "Report a body quick enough to get the body's role / faction.", TouAchAssets.RightOnTime, TouRoleIcons.Forensic, subIconTooltip: "TownOfUsMira.Role.Forensic", tags: new[] { "Forensic", "Report", "Body" });
    public static readonly AchievementSpec SearchParty = Define("Search Party",
        "Watch 5 players interact with someone.", TouCrewAssets.WatchSprite, TouRoleIcons.Lookout, subIconTooltip: "TownOfUsMira.Role.Lookout", rarity: 1, tags: new[] { "Lookout", "Watch", "Interact" });
    public static readonly AchievementSpec ParanormalActivity = Define("Paranormal Activity",
        "Watch someone die without interactions.", TouCrewAssets.WatchSprite, TouRoleIcons.Lookout, subIconTooltip: "TownOfUsMira.Role.Lookout", rarity: 1, tags: new[] { "Lookout", "Watch", "Death" });
    public static readonly AchievementSpec Purgatory = Define("Purgatory",
        "Die while Mediating.", TouCrewAssets.MediateSprite, TouRoleIcons.Medium, subIconTooltip: "TownOfUsMira.Role.Medium", tags: new[] { "Medium", "Death", "Mediate" });
    public static readonly AchievementSpec ToKillAGod = Define("To Kill a God",
        "Attempt to execute a Horseman or invulnerable player.", TouAssets.ExecuteCleanSprite, TouRoleIcons.Jailor, subIconTooltip: "TownOfUsMira.Role.Jailor",
        rarity: 2, hidden: true, toastOnGameEnd: true, tags: new[] { "Jailor", "Execute", "Hidden", "Horseman" });
    public static readonly AchievementSpec NoChances = Define("No Chances",
        "Execute an Evil without talking to them.", TouCrewAssets.JailSprite, TouRoleIcons.Jailor, subIconTooltip: "TownOfUsMira.Role.Jailor", rarity: 1, tags: new[] { "Jailor", "Execute", "Evil" });
    public static readonly AchievementSpec RiggedVotes = Define("Rigged Votes",
        "Get knighted as a revealed Mayor.", TouCrewAssets.KnightSprite, TouRoleIcons.Mayor, subIconTooltip: "TownOfUsMira.Role.Mayor", rarity: 2, tags: new[] { "Mayor", "Knight", "Reveal", "Voting" });
    public static readonly AchievementSpec CrewmatesUnite = Define("Crewmates Unite",
        "Only campaign Crewmates, and then reveal.", TouCrewAssets.CampaignButtonSprite, TouRoleIcons.Politician, subIconTooltip: "TownOfUsMira.Role.Politician",
        rarity: 2, hidden: true, toastOnGameEnd: true, tags: new[] { "Politician", "Campaign", "Reveal", "Hidden", "Voting" });
    public static readonly AchievementSpec DeathByDemocracy = Define("Death by Democracy",
        "Reveal in a 1v1.", TouAchAssets.DeathByDemocracy, TouRoleIcons.Mayor, subIconTooltip: "TownOfUsMira.Role.Mayor", rarity: 3, hidden: true, tags: new[] { "Mayor", "Reveal", "Hidden", "1v1" });
    public static readonly AchievementSpec SwitchedFate = Define("Switched Fate",
        "Swap the votes so an Impostor is ejected instead of a Crewmate.", TouAssets.SwapActive, TouRoleIcons.Swapper, subIconTooltip: "TownOfUsMira.Role.Swapper",
        rarity: 2, toastOnGameEnd: true, tags: new[] { "Swapper", "Swap", "Voting", "Eject" });
    public static readonly AchievementSpec KnightInShiningArmor = Define("Knight in Shining Armor",
        "Knight a Crewmate who then kills an Impostor or Neutral Killer.", TouCrewAssets.KnightSprite,
        TouRoleIcons.Monarch, subIconTooltip: "TownOfUsMira.Role.Monarch", rarity: 2, toastOnGameEnd: true, tags: new[] { "Monarch", "Knight", "Kill", "Protect" });
    public static readonly AchievementSpec Spotlight = Define("Spotlight",
        "Reveal the Impostors as Snitch and have one ejected in the very next meeting.", TouAssets.RevealButtonSprite,
        TouRoleIcons.Snitch, subIconTooltip: "TownOfUsMira.Role.Snitch", rarity: 1, tags: new[] { "Snitch", "Reveal", "Meeting", "Eject" });
    public static readonly AchievementSpec JusticeServed = Define("Justice Served",
        "Shoot the killer of a body you reported.", TouCrewAssets.SheriffShootSprite, TouRoleIcons.Sheriff, subIconTooltip: "TownOfUsMira.Role.Sheriff", rarity: 1, toastOnGameEnd: true, tags: new[] { "Sheriff", "Shoot", "Kill", "Body" });
    public static readonly AchievementSpec Exorcist = Define("Exorcist",
        "Cleanse a Spellslinger's hex off a player.", TouCrewAssets.CleanseSprite, TouRoleIcons.Cleric, subIconTooltip: "TownOfUsMira.Role.Cleric", rarity: 1,
        toastOnGameEnd: true, tags: new[] { "Cleric", "Cleanse", "Hex", "Protect" });
    public static readonly AchievementSpec OpenAndShut = Define("Open and Shut",
        "Prosecute an Impostor.", TouAssets.ProsecuteMeetingSprite, TouRoleIcons.Prosecutor, subIconTooltip: "TownOfUsMira.Role.Prosecutor", rarity: 1,
        toastOnGameEnd: true, tags: new[] { "Prosecutor", "Prosecute", "Meeting", "Eject" });
    public static readonly AchievementSpec SixthSense = Define("Sixth Sense",
        "Report a body within 15 seconds of sensing its death as Mystic.", TouCrewAssets.InspectSprite,
        TouRoleIcons.Mystic, subIconTooltip: "TownOfUsMira.Role.Mystic", rarity: 1, tags: new[] { "Mystic", "Sense", "Report", "Body", "Death" });
    public static readonly AchievementSpec TooCloseForComfort = Define("Too Close for Comfort",
        "Kill 2 players during a single Alert.", TouCrewAssets.AlertSprite, TouRoleIcons.Veteran, subIconTooltip: "TownOfUsMira.Role.Veteran", rarity: 1, tags: new[] { "Veteran", "Kill", "Alert", "Protect" });
    public static readonly AchievementSpec DoubleTap = Define("Double Tap",
        "Correctly guess 2 players in a single meeting as Vigilante.", TouAssets.Guess, TouRoleIcons.Vigilante, subIconTooltip: "TownOfUsMira.Role.Vigilante", rarity: 1, tags: new[] { "Vigilante", "Guess", "Kill", "Meeting" });
    public static readonly AchievementSpec HeroHour = Define("Hero Hour",
        "Fix a critical sabotage with less than 5 seconds remaining.", TouCrewAssets.FixButtonSprite,
        TouRoleIcons.Engineer, subIconTooltip: "TownOfUsMira.Role.Engineer", rarity: 2, tags: new[] { "Engineer", "Fix", "Sabotage" });
    public static readonly AchievementSpec FlushedOut = Define("Flushed Out",
        "Flush a player out of a vent as Plumber.", TouCrewAssets.FlushSprite, TouRoleIcons.Plumber, subIconTooltip: "TownOfUsMira.Role.Plumber", rarity: 1,
        toastOnGameEnd: true, tags: new[] { "Plumber", "Flush", "Vent" });
    public static readonly AchievementSpec MethodActor = Define("Method Actor",
        "Imitate 3 different roles in a single game.", TouAssets.ImitateSelectSprite, TouRoleIcons.Imitator, subIconTooltip: "TownOfUsMira.Role.Imitator", rarity: 1, tags: new[] { "Imitator", "Imitate", "Role" });
    public static readonly AchievementSpec IronWall = Define("Iron Wall",
        "Have your shield block an attack in three rounds as Medic.", TouCrewAssets.BarrierSprite, TouRoleIcons.Medic, subIconTooltip: "TownOfUsMira.Role.Medic",
        rarity: 1, progressGoal: 3, persistence: AchievementPersistence.ThroughoutRounds, toastOnGameEnd: true, tags: new[] { "Medic", "Shield", "Protect", "Block" });
    public static readonly AchievementSpec MassResurrection = Define("Mass Resurrection",
        "Revive 2 or more players with a single Revive.", TouCrewAssets.ReviveSprite, TouRoleIcons.Altruist, subIconTooltip: "TownOfUsMira.Role.Altruist", rarity: 2, tags: new[] { "Altruist", "Revive", "Multi" });
    public static readonly AchievementSpec Undo = Define("Undo",
        "Bring back 2 or more players with a single Rewind.", TouCrewAssets.RewindSprite, TouRoleIcons.TimeLord, subIconTooltip: "TownOfUsMira.Role.TimeLord", rarity: 1, toastOnGameEnd: true, tags: new[] { "TimeLord", "Rewind", "Multi", "Time" });
    public static readonly AchievementSpec Poltergeist = Define("Poltergeist",
        "Reveal the evils as Haunter.", TouAssets.RevealCleanSprite, TouRoleIcons.Haunter, subIconTooltip: "TownOfUsMira.Role.Haunter", tags: new[] { "Haunter", "Reveal", "Death" });
    public static readonly AchievementSpec Injustice = Define("Injustice",
        "Shoot a non-killing Crewmate as an Egotist Officer or Sheriff.", TouCrewAssets.SheriffShootSprite,
        TouModifierIcons.Egotist, subIconTooltip: "TownOfUsMira.Modifier.Egotist", rarity: 1,
        toastOnGameEnd: true, tags: new[] { "Egotist", "Sheriff", "Officer", "Shoot", "Modifier" });
    public static readonly AchievementSpec CaughtGhostlyHanded = Define("Caught Ghostly-Handed",
        "Watch a player die within your vision while Mediating.", TouCrewAssets.MediateSprite,
        TouRoleIcons.Medium, subIconTooltip: "TownOfUsMira.Role.Medium", rarity: 1, tags: new[] { "Medium", "Mediate", "Death" });
    public static readonly AchievementSpec LastCrewStanding = Define("Last Crew Standing",
        "Have the game end right after sensing a death.", TouCrewAssets.InspectSprite,
        TouRoleIcons.Mystic, subIconTooltip: "TownOfUsMira.Role.Mystic", rarity: 1, tags: new[] { "Mystic", "Sense", "Endgame" });
    public static readonly AchievementSpec TownOfHatred = Define("Town of Hatred",
        "Find three unique pairs of enemies in a single game.", TouCrewAssets.SeerSprite,
        TouRoleIcons.Seer, subIconTooltip: "TownOfUsMira.Role.Seer", rarity: 2, progressGoal: 3,
        persistence: AchievementPersistence.ThroughoutRounds, tags: new[] { "Seer", "Compare", "Multi" });
    public static readonly AchievementSpec RoleTwins = Define("Role Twins",
        "Compare two non-Crewmates who share a role or alignment and find them to be friends.", TouCrewAssets.SeerSprite,
        TouRoleIcons.Seer, subIconTooltip: "TownOfUsMira.Role.Seer", rarity: 2, toastOnGameEnd: true,
        tags: new[] { "Seer", "Compare", "Role" });
    public static readonly AchievementSpec TooSlow = Define("Too Slow",
        "Die after being revealed to the evils.", TouAssets.RevealButtonSprite,
        TouRoleIcons.Snitch, subIconTooltip: "TownOfUsMira.Role.Snitch", tags: new[] { "Snitch", "Death", "Reveal" });
    public static readonly AchievementSpec DeadTellNoTales = Define("Dead Tell No Tales",
        "Finish your tasks only to reveal dead players.", TouAssets.RevealCleanSprite,
        TouRoleIcons.Snitch, subIconTooltip: "TownOfUsMira.Role.Snitch", rarity: 2, hidden: true,
        tags: new[] { "Snitch", "Reveal", "Death", "Hidden" });
    public static readonly AchievementSpec SeeSomethingSayNothing = Define("See Something, Say Nothing",
        "Get blackmailed in the meeting right after revealing the Impostors.", TouAssets.BlackmailLetterSprite,
        TouRoleIcons.Snitch, subIconTooltip: "TownOfUsMira.Role.Snitch", rarity: 2, hidden: true,
        tags: new[] { "Snitch", "Blackmail", "Meeting", "Hidden" });
    public static readonly AchievementSpec Hyperfocused = Define("Hyperfocused",
        "Keep track of more than 5 players at once.", TouCrewAssets.TrackSprite,
        TouRoleIcons.Sonar, subIconTooltip: "TownOfUsMira.Role.Sonar", rarity: 2, tags: new[] { "Sonar", "Track", "Multi" });
    public static readonly AchievementSpec OffTheGlock = Define("Off-The-Glock",
        "Die while using the Admin Table.", TouAssets.AdminSprite,
        TouRoleIcons.Spy, subIconTooltip: "TownOfUsMira.Role.Spy", hidden: true,
        tags: new[] { "Spy", "Death", "Admin", "Hidden" });
    public static readonly AchievementSpec RoleCollector = Define("Role Collector",
        "Identify 5 or more roles from your traps in a single round.", TouCrewAssets.TrapSprite,
        TouRoleIcons.Trapper, subIconTooltip: "TownOfUsMira.Role.Trapper", rarity: 2, tags: new[] { "Trapper", "Trap", "Role", "Multi" });
    public static readonly AchievementSpec Sharpshooter = Define("Sharpshooter",
        "Blast a player within 30 seconds of a meeting.", TouCrewAssets.OfficerShootSprite,
        TouRoleIcons.Deputy, subIconTooltip: "TownOfUsMira.Role.Deputy", rarity: 1, tags: new[] { "Deputy", "Shoot", "Meeting" });
    public static readonly AchievementSpec ThisTownAintBigEnough = Define("This Town Ain't Big Enough",
        "Blast an Assassin.", TouAssets.ShootMeetingSprite,
        TouRoleIcons.Deputy, subIconTooltip: "TownOfUsMira.Role.Deputy", rarity: 1, toastOnGameEnd: true,
        tags: new[] { "Deputy", "Shoot", "Assassin", "Modifier" });
    public static readonly AchievementSpec BecomeTheHunted = Define("Become the Hunted",
        "Get killed by the player you were stalking.", TouCrewAssets.StalkButtonSprite,
        TouRoleIcons.Hunter, subIconTooltip: "TownOfUsMira.Role.Hunter", rarity: 1, tags: new[] { "Hunter", "Stalk", "Death" });
    public static readonly AchievementSpec Punisher = Define("Punisher",
        "Kill an evil with Retribution.", TouCrewAssets.HunterKillSprite,
        TouRoleIcons.Hunter, subIconTooltip: "TownOfUsMira.Role.Hunter", rarity: 1, toastOnGameEnd: true,
        tags: new[] { "Hunter", "Retribution", "Kill", "Evil" });
    public static readonly AchievementSpec HuntingSeason = Define("Hunting Season",
        "Kill 3 stalked players in a single game.", TouCrewAssets.HunterKillSprite,
        TouRoleIcons.Hunter, subIconTooltip: "TownOfUsMira.Role.Hunter", rarity: 2, progressGoal: 3,
        persistence: AchievementPersistence.ThroughoutRounds, tags: new[] { "Hunter", "Stalk", "Kill", "Multi" });
    public static readonly AchievementSpec TimeCop = Define("Time Cop",
        "Shoot your killer after being brought back by the Time Lord.", TouCrewAssets.OfficerShootSprite,
        TouRoleIcons.Officer, subIconTooltip: "TownOfUsMira.Role.Officer", rarity: 2,
        tags: new[] { "Officer", "Shoot", "TimeLord", "Revive" });
    public static readonly AchievementSpec Avenger = Define("Avenger",
        "Shoot the killer of a player who died near you.", TouCrewAssets.SheriffShootSprite,
        TouRoleIcons.Sheriff, subIconTooltip: "TownOfUsMira.Role.Sheriff", rarity: 1, toastOnGameEnd: true,
        tags: new[] { "Sheriff", "Shoot", "Kill", "Nearby" });
    public static readonly AchievementSpec Bloodshed = Define("Bloodshed",
        "Kill 4 or more evils in a single game while on Alert.", TouCrewAssets.AlertSprite,
        TouRoleIcons.Veteran, subIconTooltip: "TownOfUsMira.Role.Veteran", rarity: 2, progressGoal: 4,
        persistence: AchievementPersistence.ThroughoutRounds, toastOnGameEnd: true,
        tags: new[] { "Veteran", "Alert", "Kill", "Evil", "Multi" });
    public static readonly AchievementSpec GoFetch = Define("Go Fetch",
        "Kill a Werewolf while on Alert.", TouCrewAssets.AlertSprite,
        TouRoleIcons.Veteran, subIconTooltip: "TownOfUsMira.Role.Veteran", rarity: 1,
        tags: new[] { "Veteran", "Alert", "Kill", "Werewolf" });
    public static readonly AchievementSpec ShotInTheDark = Define("Shot in the Dark",
        "Correctly guess a player after losing all of your safe shots.", TouAssets.Guess,
        TouRoleIcons.Vigilante, subIconTooltip: "TownOfUsMira.Role.Vigilante", rarity: 2, tags: new[] { "Vigilante", "Guess", "Kill" });
    public static readonly AchievementSpec TheDarkKnight = Define("The Dark Knight",
        "Correctly guess 3 players in a single game.", TouAssets.Guess,
        TouRoleIcons.Vigilante, subIconTooltip: "TownOfUsMira.Role.Vigilante", rarity: 2, progressGoal: 3,
        persistence: AchievementPersistence.ThroughoutRounds, tags: new[] { "Vigilante", "Guess", "Kill", "Multi" });
    public static readonly AchievementSpec FallenKnights = Define("Fallen Knights",
        "Win and survive after all of your Knights have died.", TouCrewAssets.KnightSprite,
        TouRoleIcons.Monarch, subIconTooltip: "TownOfUsMira.Role.Monarch", rarity: 2,
        tags: new[] { "Monarch", "Knight", "Win", "Survive" });
    public static readonly AchievementSpec BostonTeaParty = Define("Boston Tea Party",
        "Get voted out by your own Knight.", TouCrewAssets.KnightSprite,
        TouRoleIcons.Monarch, subIconTooltip: "TownOfUsMira.Role.Monarch", rarity: 1, hidden: true,
        tags: new[] { "Monarch", "Knight", "Eject", "Hidden" });
    public static readonly AchievementSpec CircusPerformer = Define("Circus Performer",
        "Knight a Jester who goes on to win.", TouNeutAssets.JesterHauntSprite,
        TouRoleIcons.Monarch, subIconTooltip: "TownOfUsMira.Role.Monarch", rarity: 2, hidden: true,
        tags: new[] { "Monarch", "Knight", "Jester", "Hidden" });
    public static readonly AchievementSpec YourMajesty = Define("Your Majesty",
        "Get knighted by another Monarch.", TouCrewAssets.KnightSprite,
        TouRoleIcons.Monarch, subIconTooltip: "TownOfUsMira.Role.Monarch", rarity: 2, hidden: true,
        tags: new[] { "Monarch", "Knight", "Hidden" });
    public static readonly AchievementSpec JudgementDay = Define("Judgement Day",
        "End the game with a Prosecution.", TouAssets.ProsecuteMeetingSprite,
        TouRoleIcons.Prosecutor, subIconTooltip: "TownOfUsMira.Role.Prosecutor", rarity: 2,
        tags: new[] { "Prosecutor", "Prosecute", "Endgame", "Eject" });
    public static readonly AchievementSpec TheFinalSay = Define("The Final Say",
        "Prosecute two players in a single game.", TouAssets.ProsecutorToggleSprite,
        TouRoleIcons.Prosecutor, subIconTooltip: "TownOfUsMira.Role.Prosecutor", rarity: 1,
        tags: new[] { "Prosecutor", "Prosecute", "Multi" });
    public static readonly AchievementSpec Sike = Define("Sike",
        "Get someone ejected by swapping the votes on yourself.", TouAssets.SwapActive,
        TouRoleIcons.Swapper, subIconTooltip: "TownOfUsMira.Role.Swapper", rarity: 2,
        tags: new[] { "Swapper", "Swap", "Voting", "Eject" });
    public static readonly AchievementSpec LostCause = Define("Lost Cause",
        "Revive a player, only for them to die again.", TouCrewAssets.ReviveSprite,
        TouRoleIcons.Altruist, subIconTooltip: "TownOfUsMira.Role.Altruist", tags: new[] { "Altruist", "Revive", "Death" });
    public static readonly AchievementSpec HowDidWeGetHere = Define("How did we get here?",
        "Cleanse a player with 3 or more status effects.", TouCrewAssets.CleanseSprite,
        TouRoleIcons.Cleric, subIconTooltip: "TownOfUsMira.Role.Cleric", rarity: 2, tags: new[] { "Cleric", "Cleanse", "Multi" });
    public static readonly AchievementSpec Intact = Define("Intact",
        "Shield a player until the end of the game without them being attacked once.", TouCrewAssets.MedicSprite,
        TouRoleIcons.Medic, subIconTooltip: "TownOfUsMira.Role.Medic", rarity: 1,
        tags: new[] { "Medic", "Shield", "Protect", "Survive" });
    public static readonly AchievementSpec ReturnToSender = Define("Return to Sender",
        "Unleash an attack back onto the player it originated from.", TouCrewAssets.UnleashSprite,
        TouRoleIcons.Mirrorcaster, subIconTooltip: "TownOfUsMira.Role.Mirrorcaster", rarity: 2, toastOnGameEnd: true,
        tags: new[] { "Mirrorcaster", "Unleash", "Reflect" });
    public static readonly AchievementSpec HolyBlessing = Define("Holy Blessing",
        "Bless a player and save them from ejection.", TouCrewAssets.BlessSprite,
        TouRoleIcons.Oracle, subIconTooltip: "TownOfUsMira.Role.Oracle", rarity: 1,
        tags: new[] { "Oracle", "Bless", "Eject", "Protect" });
    public static readonly AchievementSpec NoConfessions = Define("No Confessions",
        "Get no results from a confession because too few players remain.", TouCrewAssets.ConfessSprite,
        TouRoleIcons.Oracle, subIconTooltip: "TownOfUsMira.Role.Oracle", rarity: 1, hidden: true,
        tags: new[] { "Oracle", "Confess", "Hidden" });
    public static readonly AchievementSpec NullifiedProtection = Define("Nullified Protection",
        "Have your fortified player die anyway.", TouCrewAssets.FortifySprite,
        TouRoleIcons.Warden, subIconTooltip: "TownOfUsMira.Role.Warden", tags: new[] { "Warden", "Fortify", "Death" });
    public static readonly AchievementSpec Mechanic = Define("Mechanic",
        "Fix a sabotage remotely within 5 seconds of it being called.", TouCrewAssets.FixButtonSprite,
        TouRoleIcons.Engineer, subIconTooltip: "TownOfUsMira.Role.Engineer", rarity: 1,
        tags: new[] { "Engineer", "Fix", "Sabotage" });
    public static readonly AchievementSpec Powerless = Define("Powerless",
        "Imitate a Crewmate Power role.", TouAssets.ImitateSelectSprite,
        TouRoleIcons.Imitator, subIconTooltip: "TownOfUsMira.Role.Imitator", tags: new[] { "Imitator", "Imitate", "Role" });
    public static readonly AchievementSpec Mimicry = Define("Mimicry",
        "Imitate only non-Crewmates in a game, imitating at least twice.", TouAssets.ImitateDeselectSprite,
        TouRoleIcons.Imitator, subIconTooltip: "TownOfUsMira.Role.Imitator", rarity: 2, tags: new[] { "Imitator", "Imitate", "Role" });
    public static readonly AchievementSpec ExtraSecure = Define("Extra Secure",
        "Place a camera in Security.", TouCrewAssets.DeployCamSprite,
        TouRoleIcons.Sentry, subIconTooltip: "TownOfUsMira.Role.Sentry", tags: new[] { "Sentry", "Camera", "Security" });
    public static readonly AchievementSpec PotatoTomato = Define("Potato Tomato",
        "Transport two dead bodies with one another.", TouCrewAssets.Transport,
        TouRoleIcons.Transporter, subIconTooltip: "TownOfUsMira.Role.Transporter", rarity: 1,
        tags: new[] { "Transporter", "Transport", "Body" });
    public static readonly AchievementSpec StolenValor = Define("Stolen Valor",
        "Talk with a Jailed player whilst imitating a Jailor.", TouAssets.JailCellSprite,
        TouRoleIcons.Imitator, subIconTooltip: "TownOfUsMira.Role.Imitator", rarity: 2,
        tags: new[] { "Imitator", "Jailor", "Jail", "Chat" });

    public static readonly AchievementSpec[] Crew =
    [
        ForceBeWithYou, StrangerDanger, BloodyHands, RightOnTime, SearchParty, ParanormalActivity, Purgatory,
        ToKillAGod, NoChances, RiggedVotes, CrewmatesUnite, DeathByDemocracy, SwitchedFate, KnightInShiningArmor,
        Spotlight, JusticeServed, Exorcist, OpenAndShut, SixthSense, TooCloseForComfort, DoubleTap, HeroHour,
        FlushedOut, MethodActor, IronWall, MassResurrection, Undo, Poltergeist, Injustice, CaughtGhostlyHanded,
        LastCrewStanding, TownOfHatred, RoleTwins, TooSlow, DeadTellNoTales, SeeSomethingSayNothing,
        Hyperfocused, OffTheGlock, RoleCollector, Sharpshooter, ThisTownAintBigEnough, BecomeTheHunted,
        Punisher, HuntingSeason, TimeCop, Avenger, Bloodshed, GoFetch, ShotInTheDark, TheDarkKnight,
        FallenKnights, BostonTeaParty, CircusPerformer, YourMajesty, JudgementDay, TheFinalSay, Sike,
        LostCause, HowDidWeGetHere, Intact, ReturnToSender, HolyBlessing, NoConfessions, NullifiedProtection,
        Mechanic, Powerless, Mimicry, ExtraSecure, PotatoTomato, StolenValor
    ];

    // Impostor roles
    public static readonly AchievementSpec EternalDarkness = Define("Eternal Darkness",
        "Kill three blinded players in the same game.", TouImpAssets.BlindSprite, TouRoleIcons.Eclipsal, subIconTooltip: "TownOfUsMira.Role.Eclipsal", rarity: 1,
        progressGoal: 3, persistence: AchievementPersistence.ThroughoutRounds, tags: new[] { "Eclipsal", "Kill", "Blind", "Multi" });
    public static readonly AchievementSpec BlindFoEva = Define("Blind Fo'Eva",
        "Flashbang the same player 3 times in a single game.", TouImpAssets.FlashSprite, TouRoleIcons.Grenadier, subIconTooltip: "TownOfUsMira.Role.Grenadier", rarity: 1, tags: new[] { "Grenadier", "Flashbang", "Multi" });
    public static readonly AchievementSpec IdentityCrisis = Define("Identity Crisis",
        "Kill the player you are morphed as.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling, subIconTooltip: "TownOfUsMira.Role.Morphling", rarity: 1, tags: new[] { "Morphling", "Morph", "Kill" });
    public static readonly AchievementSpec SizeDoesntMatter = Define("Size Doesn't Matter",
        "Morph as a Giant into a Mini player, or vice-versa.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling, subIconTooltip: "TownOfUsMira.Role.Morphling", rarity: 1, tags: new[] { "Morphling", "Morph", "Size" });
    public static readonly AchievementSpec Untraceable = Define("Untraceable",
        "Kill 15 players while Swooped.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper, subIconTooltip: "TownOfUsMira.Role.Swooper", rarity: 2, progressGoal: 15, tags: new[] { "Swooper", "Kill", "Swoop", "Multi" });
    public static readonly AchievementSpec Sneakman = Define("Sneakman",
        "Kill and vent away during the same swoop.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper, subIconTooltip: "TownOfUsMira.Role.Swooper", tags: new[] { "Swooper", "Kill", "Swoop", "Vent" });
    public static readonly AchievementSpec Fireworks = Define("Fireworks",
        "Kill 3 or more players with a single bomb.", TouImpAssets.DetonatingSprite, TouRoleIcons.Bomber, subIconTooltip: "TownOfUsMira.Role.Bomber", rarity: 2, toastOnGameEnd: true, tags: new[] { "Bomber", "Kill", "Bomb", "Multi" });
    public static readonly AchievementSpec BurstFire = Define("Burst Fire",
        "Kill 2 players in a single Warlock charge.", TouAssets.KillSprite, TouRoleIcons.Warlock, subIconTooltip: "TownOfUsMira.Role.Warlock", rarity: 1, tags: new[] { "Warlock", "Kill", "Multi" });
    public static readonly AchievementSpec HushMoney = Define("Hush Money",
        "Blackmail the player who reported the body.", TouImpAssets.BlackmailSprite, TouRoleIcons.Blackmailer, subIconTooltip: "TownOfUsMira.Role.Blackmailer", rarity: 1, tags: new[] { "Blackmailer", "Blackmail", "Body" });
    public static readonly AchievementSpec MassHysteria = Define("Mass Hysteria",
        "Trigger Hysteria with 4 or more hypnotized players.", TouAssets.HysteriaSprite, TouRoleIcons.Hypnotist, subIconTooltip: "TownOfUsMira.Role.Hypnotist", rarity: 1, tags: new[] { "Hypnotist", "Hysteria", "Multi" });
    public static readonly AchievementSpec Spotless = Define("Spotless",
        "Clean 3 bodies in a single game.", TouImpAssets.CleanButtonSprite, TouRoleIcons.Janitor, subIconTooltip: "TownOfUsMira.Role.Janitor", rarity: 1,
        progressGoal: 3, persistence: AchievementPersistence.ThroughoutRounds, tags: new[] { "Janitor", "Clean", "Body", "Multi" });
    public static readonly AchievementSpec SubwaySystem = Define("Subway System",
        "Place 5 vents in a single game as Miner.", TouImpAssets.MineSprite, TouRoleIcons.Miner, subIconTooltip: "TownOfUsMira.Role.Miner", rarity: 1,
        progressGoal: 5, persistence: AchievementPersistence.ThroughoutRounds, tags: new[] { "Miner", "Vent", "Multi" });
    public static readonly AchievementSpec Detonation = Define("Detonation",
        "Complete a Hex Bomb.", TouImpAssets.HexBombSprite, TouRoleIcons.Spellslinger, subIconTooltip: "TownOfUsMira.Role.Spellslinger", rarity: 2, tags: new[] { "Spellslinger", "Hex", "Bomb" });
    public static readonly AchievementSpec AreYouStupid = Define("Are you stupid?",
        "Kill the player you were pursuing.", TouImpAssets.AmbushSprite, TouRoleIcons.Ambusher, subIconTooltip: "TownOfUsMira.Role.Ambusher", tags: new[] { "Ambusher", "Kill", "Ambush" });
    public static readonly AchievementSpec FailedAmbush = Define("Failed Ambush",
        "Get killed while mid-ambush.", TouImpAssets.AmbushSprite, TouRoleIcons.Ambusher, subIconTooltip: "TownOfUsMira.Role.Ambusher",
        rarity: 2, hidden: true, tags: new[] { "Ambusher", "Death", "Ambush", "Hidden" });

    public static readonly AchievementSpec Undercover = Define("Undercover",
        "Win as a living Crewpostor without becoming the Traitor.", TouModifierIcons.Crewpostor,
        TouModifierIcons.Crewpostor, subIconTooltip: "TownOfUsMira.Modifier.Crewpostor", rarity: 2,
        tags: new[] { "Crewpostor", "Win", "Modifier" });
    public static readonly AchievementSpec TrueDarkness = Define("True Darkness",
        "Trigger an eclipse while the lights are out.", TouImpAssets.BlindSprite,
        TouRoleIcons.Eclipsal, subIconTooltip: "TownOfUsMira.Role.Eclipsal", rarity: 1,
        tags: new[] { "Eclipsal", "Blind", "Sabotage" });
    public static readonly AchievementSpec OhHenry = Define("Oh, Henry!",
        "Recall into a room where a player is already standing.", TouImpAssets.RecallSprite,
        TouRoleIcons.Escapist, subIconTooltip: "TownOfUsMira.Role.Escapist", rarity: 1, hidden: true,
        tags: new[] { "Escapist", "Recall", "Hidden" });
    public static readonly AchievementSpec Framer = Define("Framer",
        "Kill while morphed as a player and have them voted out in the same round.", TouImpAssets.MorphSprite,
        TouRoleIcons.Morphling, subIconTooltip: "TownOfUsMira.Role.Morphling", rarity: 2,
        tags: new[] { "Morphling", "Morph", "Kill", "Eject" });
    public static readonly AchievementSpec UnderTheRadar = Define("Under the Radar",
        "Pass by 3 different non-Impostors during a single swoop.", TouImpAssets.SwoopSprite,
        TouRoleIcons.Swooper, subIconTooltip: "TownOfUsMira.Role.Swooper", rarity: 1,
        tags: new[] { "Swooper", "Swoop", "Stealth" });
    public static readonly AchievementSpec Introvert = Define("Introvert",
        "Swoop 3 times in a single round.", TouImpAssets.SwoopSprite,
        TouRoleIcons.Swooper, subIconTooltip: "TownOfUsMira.Role.Swooper", rarity: 1,
        tags: new[] { "Swooper", "Swoop", "Multi" });
    public static readonly AchievementSpec Evolution = Define("Evolution",
        "Unlock all 3 abilities in a single game.", TouImpAssets.FreezeSprite,
        TouRoleIcons.Venerer, subIconTooltip: "TownOfUsMira.Role.Venerer", rarity: 1,
        tags: new[] { "Venerer", "Ability", "Multi" });
    public static readonly AchievementSpec Boogeyman = Define("Boogeyman",
        "Ambush 3 players in a single game.", TouImpAssets.AmbushSprite,
        TouRoleIcons.Ambusher, subIconTooltip: "TownOfUsMira.Role.Ambusher", rarity: 1, progressGoal: 3,
        persistence: AchievementPersistence.ThroughoutRounds, tags: new[] { "Ambusher", "Ambush", "Kill", "Multi" });
    public static readonly AchievementSpec UavInbound = Define("UAV Inbound",
        "Kill 3 scavenge targets in a row.", TouAssets.KillSprite,
        TouRoleIcons.Scavenger, subIconTooltip: "TownOfUsMira.Role.Scavenger", rarity: 2,
        tags: new[] { "Scavenger", "Kill", "Streak" });
    public static readonly AchievementSpec DoubleTrouble = Define("Double Trouble",
        "Retrain yourself and another Impostor in the same game.", TouAssets.RetrainSprite,
        TouRoleIcons.Ambassador, subIconTooltip: "TownOfUsMira.Role.Ambassador", rarity: 2,
        tags: new[] { "Ambassador", "Retrain", "Multi" });
    public static readonly AchievementSpec Strategist = Define("Strategist",
        "Use 6 or more herbs in a single game.", TouAssets.AbilityCounterHerbsSprite,
        TouRoleIcons.Herbalist, subIconTooltip: "TownOfUsMira.Role.Herbalist", rarity: 1, progressGoal: 6,
        persistence: AchievementPersistence.ThroughoutRounds, tags: new[] { "Herbalist", "Herb", "Multi" });
    public static readonly AchievementSpec ThreatEliminated = Define("Threat Eliminated",
        "Kill a killer in the same round you exposed them.", TouImpAssets.HerbExposeSprite,
        TouRoleIcons.Herbalist, subIconTooltip: "TownOfUsMira.Role.Herbalist", rarity: 2, toastOnGameEnd: true,
        tags: new[] { "Herbalist", "Expose", "Kill" });
    public static readonly AchievementSpec AllKnowing = Define("All-Knowing",
        "Have the roles of every living player revealed to you.", TouImpAssets.HerbExposeSprite,
        TouRoleIcons.Herbalist, subIconTooltip: "TownOfUsMira.Role.Herbalist", rarity: 3,
        tags: new[] { "Herbalist", "Expose", "Role", "All" });
    public static readonly AchievementSpec DoubleDipping = Define("Double-Dipping",
        "Control the same player twice in one game.", TouImpAssets.ControlSprite,
        TouRoleIcons.Puppeteer, subIconTooltip: "TownOfUsMira.Role.Puppeteer", rarity: 1,
        tags: new[] { "Puppeteer", "Control", "Multi" });
    public static readonly AchievementSpec Homewrecker = Define("Homewrecker",
        "Kill your puppet's lover, taking both out at once.", TouImpAssets.ControlSprite,
        TouRoleIcons.Puppeteer, subIconTooltip: "TownOfUsMira.Role.Puppeteer", rarity: 2,
        tags: new[] { "Puppeteer", "Control", "Kill", "Lover" });
    public static readonly AchievementSpec WorldEnder = Define("World-Ender",
        "Win the game with a Hex Bomb.", TouImpAssets.HexBombSprite,
        TouRoleIcons.Spellslinger, subIconTooltip: "TownOfUsMira.Role.Spellslinger", rarity: 3,
        tags: new[] { "Spellslinger", "Hex", "Bomb", "Win" });
    public static readonly AchievementSpec CulturalDischarge = Define("Cultural Discharge",
        "Die while your Hex Bomb is active.", TouImpAssets.HexBombSprite,
        TouRoleIcons.Spellslinger, subIconTooltip: "TownOfUsMira.Role.Spellslinger", rarity: 1, hidden: true,
        tags: new[] { "Spellslinger", "Hex", "Bomb", "Death", "Hidden" });
    public static readonly AchievementSpec DeadlyDelusion = Define("Deadly Delusion",
        "Become the Traitor and win within two rounds.", TouImpAssets.TraitorSelect,
        TouRoleIcons.Traitor, subIconTooltip: "TownOfUsMira.Role.Traitor", rarity: 2,
        tags: new[] { "Traitor", "Win" });
    public static readonly AchievementSpec UselessJail = Define("Useless Jail",
        "Blackmail a player who is also Jailed.", TouImpAssets.BlackmailSprite,
        TouRoleIcons.Blackmailer, subIconTooltip: "TownOfUsMira.Role.Blackmailer", rarity: 1, hidden: true,
        tags: new[] { "Blackmailer", "Blackmail", "Jail", "Hidden" });
    public static readonly AchievementSpec Poisoner = Define("Poisoner",
        "Kill a player with poison.", TouImpAssets.PoisonSprite,
        TouRoleIcons.Bootlegger, subIconTooltip: "TownOfUsMira.Role.Bootlegger", tags: new[] { "Bootlegger", "Poison", "Kill" });
    public static readonly AchievementSpec Blackout = Define("Blackout",
        "Serve the same player three drinks in a row in one round.", TouImpAssets.DrinkPoisonSprite,
        TouRoleIcons.Bootlegger, subIconTooltip: "TownOfUsMira.Role.Bootlegger", rarity: 2,
        tags: new[] { "Bootlegger", "Drink", "Poison", "Multi" });
    public static readonly AchievementSpec HorsingAround = Define("Horsing Around",
        "Trigger Hysteria with every living non-Impostor hypnotized.", TouAssets.HysteriaSprite,
        TouRoleIcons.Hypnotist, subIconTooltip: "TownOfUsMira.Role.Hypnotist", rarity: 2,
        tags: new[] { "Hypnotist", "Hysteria", "All" });
    public static readonly AchievementSpec Delusional = Define("Delusional",
        "Keep Hysteria active for three rounds.", TouAssets.HysteriaCleanSprite,
        TouRoleIcons.Hypnotist, subIconTooltip: "TownOfUsMira.Role.Hypnotist", rarity: 2,
        tags: new[] { "Hypnotist", "Hysteria", "Multi" });
    public static readonly AchievementSpec FalseEvidence = Define("False Evidence",
        "Have someone report a body you dragged elsewhere.", TouImpAssets.DragSprite,
        TouRoleIcons.Undertaker, subIconTooltip: "TownOfUsMira.Role.Undertaker", rarity: 1,
        tags: new[] { "Undertaker", "Drag", "Body", "Report" });
    public static readonly AchievementSpec PublicExecution = Define("Public Execution",
        "Kill your Overtaken target in front of another player.", TouImpAssets.OvertakeSprite,
        TouRoleIcons.Parasite, subIconTooltip: "TownOfUsMira.Role.Parasite", rarity: 1,
        tags: new[] { "Parasite", "Overtake", "Kill", "Witness" });
    public static readonly AchievementSpec Fratricide = Define("Fratricide",
        "Control one of your fellow Impostors.", TouImpAssets.ControlSprite,
        TouRoleIcons.Puppeteer, subIconTooltip: "TownOfUsMira.Role.Puppeteer", rarity: 1, hidden: true,
        tags: new[] { "Puppeteer", "Control", "Impostor", "Hidden" });

    public static readonly AchievementSpec[] Imp =
    [
        EternalDarkness, BlindFoEva, IdentityCrisis, SizeDoesntMatter, Untraceable, Sneakman, Fireworks, BurstFire,
        HushMoney, MassHysteria, Spotless, SubwaySystem, Detonation, AreYouStupid, FailedAmbush, Undercover,
        TrueDarkness, OhHenry, Framer, UnderTheRadar, Introvert, Evolution, Boogeyman, UavInbound, DoubleTrouble,
        Strategist, ThreatEliminated, AllKnowing, DoubleDipping, Homewrecker, WorldEnder, CulturalDischarge,
        DeadlyDelusion, UselessJail, Poisoner, Blackout, HorsingAround, Delusional, FalseEvidence,
        PublicExecution, Fratricide
    ];

    // Neutral roles
    public static readonly AchievementSpec AmnesiaSquared = Define("Amnesia²",
        "Attempt to remember a player who cannot be remembered.", TouNeutAssets.RememberButtonSprite, TouRoleIcons.Amnesiac, subIconTooltip: "TownOfUsMira.Role.Amnesiac", tags: new[] { "Amnesiac", "Remember", "Role" });
    public static readonly AchievementSpec FairlyBadParent = Define("Fairly Bad Parent",
        "Have your target die in the first round.", TouAchAssets.FairlyBadParent, TouRoleIcons.Fairy, subIconTooltip: "TownOfUsMira.Role.Fairy", tags: new[] { "Fairy", "Target", "Death" });
    public static readonly AchievementSpec Hangman = Define("Hangman",
        "Get your target voted within 3 rounds.", TouNeutAssets.ExeTormentSprite, TouRoleIcons.Executioner, subIconTooltip: "TownOfUsMira.Role.Executioner", rarity: 1, tags: new[] { "Executioner", "Target", "Vote", "Eject" });
    public static readonly AchievementSpec TrickyClown = Define("Tricky Clown",
        "Get ejected within 3 rounds.", TouNeutAssets.JesterHauntSprite, TouRoleIcons.Jester, subIconTooltip: "TownOfUsMira.Role.Jester", rarity: 1, tags: new[] { "Jester", "Eject", "Meeting" });
    public static readonly AchievementSpec FinalAct = Define("The Final Act",
        "Get ejected when three or less players remain.", TouNeutAssets.JesterHauntSprite, TouRoleIcons.Jester, subIconTooltip: "TownOfUsMira.Role.Jester", rarity: 2, tags: new[] { "Jester", "Eject", "Endgame" });
    public static readonly AchievementSpec DontPokeTheBear = Define("Don't Poke the Bear",
        "Poke an alerting Veteran, gazing Medusa, or a Pestilence, and die to them.", TouNeutAssets.JesterPokeSprite,
        TouRoleIcons.Jester, subIconTooltip: "TownOfUsMira.Role.Jester", tags: new[] { "Jester", "Poke", "Death", "Horseman" });
    public static readonly AchievementSpec HeatOfTheBattle = Define("Heat of the Battle",
        "Ignite 5 or more players at once.", TouNeutAssets.IgniteButtonSprite, TouRoleIcons.Arsonist, subIconTooltip: "TownOfUsMira.Role.Arsonist", rarity: 2, tags: new[] { "Arsonist", "Kill", "Multi", "Ignite" });
    public static readonly AchievementSpec Ballistic = Define("Ballistic",
        "Get a zero-second cooldown.", TouNeutAssets.JuggKillSprite, TouRoleIcons.Juggernaut, subIconTooltip: "TownOfUsMira.Role.Juggernaut", rarity: 2, tags: new[] { "Juggernaut", "Cooldown", "Kill" });
    public static readonly AchievementSpec SpanishInquisition = Define("Spanish Inquisition",
        "Vanquish all of your heretics by yourself.", TouNeutAssets.InquireSprite, TouRoleIcons.Inquisitor, subIconTooltip: "TownOfUsMira.Role.Inquisitor", rarity: 3, tags: new[] { "Inquisitor", "Kill", "Solo", "All" });
    public static readonly AchievementSpec SnakeEyes = Define("Snake Eyes",
        "Kill 15 players across sessions as Pestilence.", TouNeutAssets.PestKillSprite, TouRoleIcons.Pestilence, subIconTooltip: "TownOfUsMira.Role.Pestilence",
        rarity: 2, progressGoal: 15, tags: new[] { "Pestilence", "Kill", "Multi", "Horseman" });
    public static readonly AchievementSpec PatientZero = Define("Patient Zero",
        "Infect 10 players across sessions as Plaguebearer.", TouNeutAssets.InfectSprite, TouRoleIcons.Plaguebearer, subIconTooltip: "TownOfUsMira.Role.Plaguebearer",
        rarity: 2, progressGoal: 10, tags: new[] { "Plaguebearer", "Infect", "Multi" });
    public static readonly AchievementSpec Nosferatu = Define("Nosferatu",
        "Bite 15 players across sessions as Vampire.", TouNeutAssets.BiteSprite, TouRoleIcons.Vampire, subIconTooltip: "TownOfUsMira.Role.Vampire", rarity: 2,
        progressGoal: 15, tags: new[] { "Vampire", "Bite", "Multi" });
    public static readonly AchievementSpec CommonCold = Define("Common Cold",
        "Win as Pestilence with 5 or more dead players.", TouNeutAssets.PestKillSprite, TouRoleIcons.Pestilence, subIconTooltip: "TownOfUsMira.Role.Pestilence", rarity: 1, tags: new[] { "Pestilence", "Win", "Horseman" });
    public static readonly AchievementSpec Nostradamus = Define("Nostradamus",
        "Guess 3 players correctly in a single meeting.", TouAssets.Guess, TouRoleIcons.Doomsayer, subIconTooltip: "TownOfUsMira.Role.Doomsayer", rarity: 2, tags: new[] { "Doomsayer", "Guess", "Multi", "Meeting" });
    public static readonly AchievementSpec Hitman = Define("Hitman",
        "Correctly guess 2 players in a single meeting as Assassin.", TouAssets.ShootMeetingSprite, TouModifierIcons.Assassin, subIconTooltip: "TownOfUsMira.Modifier.Assassin", rarity: 1, tags: new[] { "Assassin", "Guess", "Multi", "Meeting", "Modifier" });
    public static readonly AchievementSpec Anomaly = Define("Anomaly",
        "Get hacked as the Glitch.", TouNeutAssets.HackSprite, TouRoleIcons.Glitch, subIconTooltip: "TownOfUsMira.Role.Glitch", rarity: 1, tags: new[] { "Glitch", "Hack", "Horseman" });
    public static readonly AchievementSpec FullCourseMeal = Define("Full Course Meal",
        "Feed a Mini body and a Giant body in the same game.", TouAchAssets.FullCourseMeal, TouRoleIcons.Chef, subIconTooltip: "TownOfUsMira.Role.Chef", rarity: 1, tags: new[] { "Chef", "Feed", "Body", "Size" });
    public static readonly AchievementSpec FullMoon = Define("Full Moon",
        "Kill 15 players across sessions as Werewolf.", TouNeutAssets.PestKillSprite, TouRoleIcons.Werewolf, subIconTooltip: "TownOfUsMira.Role.Werewolf", rarity: 2,
        progressGoal: 15, tags: new[] { "Werewolf", "Kill", "Multi" });

    public static readonly AchievementSpec DivineProtector = Define("Divine Protector",
        "Fend off an attacker with your protection.", TouNeutAssets.ProtectSprite,
        TouRoleIcons.Fairy, subIconTooltip: "TownOfUsMira.Role.Fairy", rarity: 1,
        tags: new[] { "Fairy", "Protect", "Block" });
    public static readonly AchievementSpec DoABarrelroll = Define("Do a Barrelroll!",
        "Bribe 2 or more players in a single game.", TouNeutAssets.BribeSprite,
        TouRoleIcons.Mercenary, subIconTooltip: "TownOfUsMira.Role.Mercenary", rarity: 1,
        tags: new[] { "Mercenary", "Bribe", "Multi" });
    public static readonly AchievementSpec WinWinSituation = Define("Win-Win Situation",
        "Win with at least two-thirds of the living players bribed.", TouNeutAssets.BribeSprite,
        TouRoleIcons.Mercenary, subIconTooltip: "TownOfUsMira.Role.Mercenary", rarity: 2,
        tags: new[] { "Mercenary", "Bribe", "Win" });
    public static readonly AchievementSpec SurvivalOfTheFittest = Define("Survival of the Fittest",
        "Be the sole winner as Survivor.", TouNeutAssets.VestSprite,
        TouRoleIcons.Survivor, subIconTooltip: "TownOfUsMira.Role.Survivor", rarity: 3, hidden: true,
        tags: new[] { "Survivor", "Win", "Solo", "Hidden" });
    public static readonly AchievementSpec ThirdWheel = Define("Third-Wheel",
        "Win alongside the Lovers.", TouNeutAssets.VestSprite,
        TouRoleIcons.Survivor, subIconTooltip: "TownOfUsMira.Role.Survivor", rarity: 1,
        tags: new[] { "Survivor", "Win", "Lover" });
    public static readonly AchievementSpec RoleEater = Define("Role-Eater",
        "Guess every role you need within 3 rounds.", TouNeutAssets.Observe,
        TouRoleIcons.Doomsayer, subIconTooltip: "TownOfUsMira.Role.Doomsayer", rarity: 2,
        tags: new[] { "Doomsayer", "Guess", "Win" });
    public static readonly AchievementSpec Fireproof = Define("Fireproof",
        "Try to ignite a protected player.", TouNeutAssets.IgniteButtonSprite,
        TouRoleIcons.Arsonist, subIconTooltip: "TownOfUsMira.Role.Arsonist", hidden: true,
        tags: new[] { "Arsonist", "Ignite", "Protect", "Hidden" });
    public static readonly AchievementSpec Carrier = Define("Carrier",
        "Win as Plaguebearer without ever transforming into Pestilence.", TouNeutAssets.InfectSprite,
        TouRoleIcons.Plaguebearer, subIconTooltip: "TownOfUsMira.Role.Plaguebearer", rarity: 2,
        tags: new[] { "Plaguebearer", "Win" });
    public static readonly AchievementSpec BadStaringContest = Define("Bad Staring Contest",
        "Get killed through Stone Gaze.", TouNeutAssets.StoneGazeSprite,
        TouRoleIcons.Medusa, subIconTooltip: "TownOfUsMira.Role.Medusa", rarity: 1, hidden: true,
        tags: new[] { "Medusa", "Gaze", "Death", "Hidden" });
    public static readonly AchievementSpec ChangeOfHeart = Define("Change of Heart",
        "Convert a player when fewer than 5 players remain.", TouNeutAssets.BiteSprite,
        TouRoleIcons.Vampire, subIconTooltip: "TownOfUsMira.Role.Vampire", rarity: 2,
        tags: new[] { "Vampire", "Bite", "Convert", "Endgame" });
    public static readonly AchievementSpec BloodMoon = Define("Blood Moon",
        "End the game by killing all remaining players in a single Rampage.", TouNeutAssets.RampageSprite,
        TouRoleIcons.Werewolf, subIconTooltip: "TownOfUsMira.Role.Werewolf", rarity: 3,
        tags: new[] { "Werewolf", "Rampage", "Kill", "Endgame" });
    public static readonly AchievementSpec HappyCustomers = Define("Happy Customers",
        "Leave in victory after completing your goal as Chef.", TouNeutAssets.ChefServeCakeSprite,
        TouRoleIcons.Chef, subIconTooltip: "TownOfUsMira.Role.Chef", rarity: 1,
        tags: new[] { "Chef", "Win" });
    public static readonly AchievementSpec Lazybones = Define("Lazybones",
        "Win as Inquisitor without using any abilities.", TouNeutAssets.InquireSprite,
        TouRoleIcons.Inquisitor, subIconTooltip: "TownOfUsMira.Role.Inquisitor", rarity: 2, hidden: true,
        tags: new[] { "Inquisitor", "Win", "Hidden" });
    public static readonly AchievementSpec AllBarkNoBite = Define("All Bark No Bite",
        "End a Rampage without killing anyone.", TouAchAssets.AllBarkNoBite,
        TouRoleIcons.Werewolf, subIconTooltip: "TownOfUsMira.Role.Werewolf",
        tags: new[] { "Werewolf", "Rampage" });
    public static readonly AchievementSpec BlindFaith = Define("Blind Faith",
        "Observe a player, then guess them as a role that wasn't even listed.", TouNeutAssets.Observe,
        TouRoleIcons.Doomsayer, subIconTooltip: "TownOfUsMira.Role.Doomsayer", rarity: 1, hidden: true,
        tags: new[] { "Doomsayer", "Observe", "Guess", "Hidden" });

    public static readonly AchievementSpec[] Neut =
    [
        AmnesiaSquared, FairlyBadParent, Hangman, TrickyClown, FinalAct, DontPokeTheBear, HeatOfTheBattle,
        Ballistic, SpanishInquisition, SnakeEyes, PatientZero, Nosferatu, CommonCold, Nostradamus, Hitman,
        Anomaly, FullCourseMeal, FullMoon, DivineProtector, DoABarrelroll, WinWinSituation,
        SurvivalOfTheFittest, ThirdWheel, RoleEater, Fireproof, Carrier, BadStaringContest, ChangeOfHeart,
        BloodMoon, HappyCustomers, Lazybones, AllBarkNoBite, BlindFaith
    ];
}