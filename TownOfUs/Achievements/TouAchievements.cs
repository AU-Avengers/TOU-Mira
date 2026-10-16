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
        LoadableAsset<Sprite>? subIcon = null, int rarity = 0, bool hidden = false, int progressGoal = 0,
        AchievementPersistence persistence = AchievementPersistence.ThroughoutSessions,
        bool toastOnGameEnd = false, [CallerMemberName] string key = "")
        => new(key, name, description, icon, subIcon, rarity, hidden, progressGoal, persistence, toastOnGameEnd);

    // General
    public static readonly AchievementSpec MiraMonday = Define("Mira Monday",
        "Play on Mira HQ on a Monday!", TouAssets.IconMira);
    public static readonly AchievementSpec FungleFriday = Define("Fungle Friday",
        "Play on Fungle on a Friday!", TouAssets.IconFungle);
    public static readonly AchievementSpec SubmergedSaturday = Define("Submerged Saturday",
        "Play on Submerged on a Saturday!", TouAssets.IconSubmerged);
    public static readonly AchievementSpec SkeldSunday = Define("Skeld Sunday",
        "Play on Skeld on a Sunday!", TouAssets.IconSkeld);
    public static readonly AchievementSpec Cheater = Define("Cheater",
        "Use /up to give yourself a role in the lobby.", TouRoleIcons.Impostor, hidden: true);
    public static readonly AchievementSpec Silence = Define("Silence",
        "Force end a meeting as host.", TouRoleIcons.Prosecutor, hidden: true);
    public static readonly AchievementSpec Oops = Define("Oops",
        "Abort a game as host.", TouRoleIcons.Mayor, hidden: true);
    public static readonly AchievementSpec IveSeenThePlays = Define("I've seen the plays",
        "No I'm actually inclined to believe 5up you know because I've seen the plays where dk hops out of a vent and then kills himself",
        TouRoleIcons.Jester, rarity: 1, hidden: true);

    public static readonly AchievementSpec[] General =
    [
        MiraMonday, FungleFriday, SubmergedSaturday, SkeldSunday, Cheater, Silence, Oops, IveSeenThePlays
    ];

    // Crewmate roles
    public static readonly AchievementSpec ForceBeWithYou = Define("The Force Be With You",
        "Sense 5 abilities being used in your radius throughout a game.", TouRoleIcons.Aurial, TouRoleIcons.Aurial,
        rarity: 1, progressGoal: 5, persistence: AchievementPersistence.ThroughoutRounds);
    public static readonly AchievementSpec StrangerDanger = Define("Stranger Danger",
        "Sense a kill happen within your uncolored radius.", TouRoleIcons.Aurial, TouRoleIcons.Aurial);
    public static readonly AchievementSpec BloodyHands = Define("Bloody Hands",
        "Find a suspect on the first Examine attempt.", TouAchAssets.BloodyHands, TouRoleIcons.Forensic, rarity: 1);
    public static readonly AchievementSpec RightOnTime = Define("Right on Time",
        "Report a body quick enough to get the body's role / faction.", TouAchAssets.RightOnTime, TouRoleIcons.Forensic);
    public static readonly AchievementSpec SearchParty = Define("Search Party",
        "Watch 5 players interact with someone.", TouCrewAssets.WatchSprite, TouRoleIcons.Lookout, rarity: 1);
    public static readonly AchievementSpec ParanormalActivity = Define("Paranormal Activity",
        "Watch someone die without interactions.", TouCrewAssets.WatchSprite, TouRoleIcons.Lookout, rarity: 1);
    public static readonly AchievementSpec Purgatory = Define("Purgatory",
        "Die while Mediating.", TouCrewAssets.MediateSprite, TouRoleIcons.Medium);
    public static readonly AchievementSpec ToKillAGod = Define("To Kill a God",
        "Attempt to execute a Horseman or invulnerable player.", TouAssets.ExecuteCleanSprite, TouRoleIcons.Jailor,
        rarity: 2, hidden: true, toastOnGameEnd: true);
    public static readonly AchievementSpec NoChances = Define("No Chances",
        "Execute an Evil without talking to them.", TouCrewAssets.JailSprite, TouRoleIcons.Jailor, rarity: 1);
    public static readonly AchievementSpec RiggedVotes = Define("Rigged Votes",
        "Get knighted as a revealed Mayor.", TouCrewAssets.KnightSprite, TouRoleIcons.Mayor, rarity: 2);
    public static readonly AchievementSpec CrewmatesUnite = Define("Crewmates Unite",
        "Only campaign Crewmates, and then reveal.", TouCrewAssets.CampaignButtonSprite, TouRoleIcons.Politician,
        rarity: 2, hidden: true, toastOnGameEnd: true);
    public static readonly AchievementSpec DeathByDemocracy = Define("Death by Democracy",
        "Reveal in a 1v1.", TouAchAssets.DeathByDemocracy, TouRoleIcons.Mayor, rarity: 3, hidden: true);
    public static readonly AchievementSpec SwitchedFate = Define("Switched Fate",
        "Swap the votes so an Impostor is ejected instead of a Crewmate.", TouAssets.SwapActive, TouRoleIcons.Swapper,
        rarity: 2, toastOnGameEnd: true);
    public static readonly AchievementSpec KnightInShiningArmor = Define("Knight in Shining Armor",
        "Knight a Crewmate who then kills an Impostor or Neutral Killer.", TouCrewAssets.KnightSprite,
        TouRoleIcons.Monarch, rarity: 2, toastOnGameEnd: true);
    public static readonly AchievementSpec Spotlight = Define("Spotlight",
        "Reveal the Impostors as Snitch and have one ejected in the very next meeting.", TouAssets.RevealButtonSprite,
        TouRoleIcons.Snitch, rarity: 1);
    public static readonly AchievementSpec JusticeServed = Define("Justice Served",
        "Shoot the killer of a body you reported.", TouCrewAssets.SheriffShootSprite, TouRoleIcons.Sheriff, rarity: 1);
    public static readonly AchievementSpec Exorcist = Define("Exorcist",
        "Cleanse a Spellslinger's hex off a player.", TouCrewAssets.CleanseSprite, TouRoleIcons.Cleric, rarity: 1,
        toastOnGameEnd: true);
    public static readonly AchievementSpec OpenAndShut = Define("Open and Shut",
        "Prosecute an Impostor.", TouAssets.ProsecuteMeetingSprite, TouRoleIcons.Prosecutor, rarity: 1,
        toastOnGameEnd: true);
    public static readonly AchievementSpec SixthSense = Define("Sixth Sense",
        "Report a body within 15 seconds of sensing its death as Mystic.", TouCrewAssets.InspectSprite,
        TouRoleIcons.Mystic, rarity: 1);
    public static readonly AchievementSpec TooCloseForComfort = Define("Too Close for Comfort",
        "Kill 2 players during a single Alert.", TouCrewAssets.AlertSprite, TouRoleIcons.Veteran, rarity: 1);
    public static readonly AchievementSpec DoubleTap = Define("Double Tap",
        "Correctly guess 2 players in a single meeting as Vigilante.", TouAssets.Guess, TouRoleIcons.Vigilante, rarity: 1);
    public static readonly AchievementSpec HeroHour = Define("Hero Hour",
        "Fix a critical sabotage with less than 5 seconds remaining.", TouCrewAssets.FixButtonSprite,
        TouRoleIcons.Engineer, rarity: 2);
    public static readonly AchievementSpec FlushedOut = Define("Flushed Out",
        "Flush a player out of a vent as Plumber.", TouCrewAssets.FlushSprite, TouRoleIcons.Plumber, rarity: 1,
        toastOnGameEnd: true);
    public static readonly AchievementSpec MethodActor = Define("Method Actor",
        "Imitate 3 different roles in a single game.", TouAssets.ImitateSelectSprite, TouRoleIcons.Imitator, rarity: 1);
    public static readonly AchievementSpec IronWall = Define("Iron Wall",
        "Have your shield block 3 attacks in a single game as Medic.", TouCrewAssets.BarrierSprite, TouRoleIcons.Medic,
        rarity: 1, progressGoal: 3, persistence: AchievementPersistence.ThroughoutRounds);
    public static readonly AchievementSpec MassResurrection = Define("Mass Resurrection",
        "Revive 2 or more players with a single Revive.", TouCrewAssets.ReviveSprite, TouRoleIcons.Altruist, rarity: 2);
    public static readonly AchievementSpec Undo = Define("Undo",
        "Bring back 2 or more players with a single Rewind.", TouCrewAssets.RewindSprite, TouRoleIcons.TimeLord, rarity: 1);
    public static readonly AchievementSpec Poltergeist = Define("Poltergeist",
        "Reveal the evils as Haunter.", TouAssets.RevealCleanSprite, TouRoleIcons.Haunter);

    public static readonly AchievementSpec[] Crew =
    [
        ForceBeWithYou, StrangerDanger, BloodyHands, RightOnTime, SearchParty, ParanormalActivity, Purgatory,
        ToKillAGod, NoChances, RiggedVotes, CrewmatesUnite, DeathByDemocracy, SwitchedFate, KnightInShiningArmor,
        Spotlight, JusticeServed, Exorcist, OpenAndShut, SixthSense, TooCloseForComfort, DoubleTap, HeroHour,
        FlushedOut, MethodActor, IronWall, MassResurrection, Undo, Poltergeist
    ];

    // Impostor roles
    public static readonly AchievementSpec EternalDarkness = Define("Eternal Darkness",
        "Kill three blinded players in the same game.", TouImpAssets.BlindSprite, TouRoleIcons.Eclipsal, rarity: 1,
        progressGoal: 3, persistence: AchievementPersistence.ThroughoutRounds);
    public static readonly AchievementSpec BlindFoEva = Define("Blind Fo'Eva",
        "Flashbang the same player 3 times in a single game.", TouImpAssets.FlashSprite, TouRoleIcons.Grenadier, rarity: 1);
    public static readonly AchievementSpec IdentityCrisis = Define("Identity Crisis",
        "Kill the player you are morphed as.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling, rarity: 1);
    public static readonly AchievementSpec SizeDoesntMatter = Define("Size Doesn't Matter",
        "Morph as a Giant into a Mini player, or vice-versa.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling, rarity: 1);
    public static readonly AchievementSpec Untraceable = Define("Untraceable",
        "Kill 15 players while Swooped.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper, rarity: 2, progressGoal: 15);
    public static readonly AchievementSpec Sneakman = Define("Sneakman",
        "Kill and vent away during the same swoop.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper);
    public static readonly AchievementSpec Fireworks = Define("Fireworks",
        "Kill 3 or more players with a single bomb.", TouImpAssets.DetonatingSprite, TouRoleIcons.Bomber, rarity: 2);
    public static readonly AchievementSpec BurstFire = Define("Burst Fire",
        "Kill 2 players in a single Warlock charge.", TouAssets.KillSprite, TouRoleIcons.Warlock, rarity: 1);
    public static readonly AchievementSpec HushMoney = Define("Hush Money",
        "Blackmail the player who reported the body.", TouImpAssets.BlackmailSprite, TouRoleIcons.Blackmailer, rarity: 1);
    public static readonly AchievementSpec MassHysteria = Define("Mass Hysteria",
        "Trigger Hysteria with 4 or more hypnotized players.", TouAssets.MassHysteriaSprite, TouRoleIcons.Hypnotist, rarity: 1);
    public static readonly AchievementSpec Spotless = Define("Spotless",
        "Clean 3 bodies in a single game.", TouImpAssets.CleanButtonSprite, TouRoleIcons.Janitor, rarity: 1,
        progressGoal: 3, persistence: AchievementPersistence.ThroughoutRounds);
    public static readonly AchievementSpec SubwaySystem = Define("Subway System",
        "Place 5 vents in a single game as Miner.", TouImpAssets.MineSprite, TouRoleIcons.Miner, rarity: 1,
        progressGoal: 5, persistence: AchievementPersistence.ThroughoutRounds);
    public static readonly AchievementSpec Detonation = Define("Detonation",
        "Complete a Hex Bomb.", TouImpAssets.HexBombSprite, TouRoleIcons.Spellslinger, rarity: 2);
    public static readonly AchievementSpec PerfectAmbush = Define("Perfect Ambush",
        "Kill the player you were pursuing.", TouImpAssets.AmbushSprite, TouRoleIcons.Ambusher);

    public static readonly AchievementSpec[] Imp =
    [
        EternalDarkness, BlindFoEva, IdentityCrisis, SizeDoesntMatter, Untraceable, Sneakman, Fireworks, BurstFire,
        HushMoney, MassHysteria, Spotless, SubwaySystem, Detonation, PerfectAmbush
    ];

    // Neutral roles
    public static readonly AchievementSpec AmnesiaSquared = Define("Amnesia²",
        "Attempt to remember a player who cannot be remembered.", TouNeutAssets.RememberButtonSprite, TouRoleIcons.Amnesiac);
    public static readonly AchievementSpec FairlyBadParent = Define("Fairly Bad Parent",
        "Have your target die in the first round.", TouAchAssets.FairlyBadParent, TouRoleIcons.Fairy);
    public static readonly AchievementSpec Hangman = Define("Hangman",
        "Get your target voted within 3 rounds.", TouNeutAssets.ExeTormentSprite, TouRoleIcons.Executioner, rarity: 1);
    public static readonly AchievementSpec TrickyClown = Define("Tricky Clown",
        "Get ejected within 3 rounds.", TouNeutAssets.JesterHauntSprite, TouRoleIcons.Jester, rarity: 1);
    public static readonly AchievementSpec FinalAct = Define("The Final Act",
        "Get ejected when three or less players remain.", TouNeutAssets.JesterHauntSprite, TouRoleIcons.Jester, rarity: 2);
    public static readonly AchievementSpec DontPokeTheBear = Define("Don't Poke the Bear",
        "Poke an alerting Veteran, gazing Medusa, or a Pestilence, and die to them.", TouNeutAssets.JesterPokeSprite,
        TouRoleIcons.Jester);
    public static readonly AchievementSpec HeatOfTheBattle = Define("Heat of the Battle",
        "Ignite 5 or more players at once.", TouNeutAssets.IgniteButtonSprite, TouRoleIcons.Arsonist, rarity: 2);
    public static readonly AchievementSpec Ballistic = Define("Ballistic",
        "Get a zero-second cooldown.", TouNeutAssets.JuggKillSprite, TouRoleIcons.Juggernaut, rarity: 2);
    public static readonly AchievementSpec SpanishInquisition = Define("Spanish Inquisition",
        "Vanquish all of your heretics by yourself.", TouNeutAssets.InquireSprite, TouRoleIcons.Inquisitor, rarity: 3);
    public static readonly AchievementSpec SnakeEyes = Define("Snake Eyes",
        "Kill 15 players across sessions as Pestilence.", TouNeutAssets.PestKillSprite, TouRoleIcons.Pestilence,
        rarity: 2, progressGoal: 15);
    public static readonly AchievementSpec PatientZero = Define("Patient Zero",
        "Infect 10 players across sessions as Plaguebearer.", TouNeutAssets.InfectSprite, TouRoleIcons.Plaguebearer,
        rarity: 2, progressGoal: 10);
    public static readonly AchievementSpec Nosferatu = Define("Nosferatu",
        "Bite 15 players across sessions as Vampire.", TouNeutAssets.BiteSprite, TouRoleIcons.Vampire, rarity: 2,
        progressGoal: 15);
    public static readonly AchievementSpec CommonCold = Define("Common Cold",
        "Win as Pestilence with 5 or more dead players.", TouNeutAssets.PestKillSprite, TouRoleIcons.Pestilence, rarity: 1);
    public static readonly AchievementSpec Nostradamus = Define("Nostradamus",
        "Guess 3 players correctly in a single meeting.", TouAssets.Guess, TouRoleIcons.Doomsayer, rarity: 2);
    public static readonly AchievementSpec Hitman = Define("Hitman",
        "Correctly guess 2 players in a single meeting as Assassin.", TouAssets.ShootMeetingSprite, TouRoleIcons.RandomAny, rarity: 1);
    public static readonly AchievementSpec Anomaly = Define("Anomaly",
        "Get hacked as the Glitch.", TouNeutAssets.HackSprite, TouRoleIcons.Glitch, rarity: 1);
    public static readonly AchievementSpec FullCourseMeal = Define("Full Course Meal",
        "Feed a Mini body and a Giant body in the same game.", TouAchAssets.FullCourseMeal, TouRoleIcons.Chef, rarity: 1);
    public static readonly AchievementSpec FullMoon = Define("Full Moon",
        "Kill 15 players across sessions as Werewolf.", TouNeutAssets.PestKillSprite, TouRoleIcons.Werewolf, rarity: 2,
        progressGoal: 15);

    public static readonly AchievementSpec[] Neut =
    [
        AmnesiaSquared, FairlyBadParent, Hangman, TrickyClown, FinalAct, DontPokeTheBear, HeatOfTheBattle,
        Ballistic, SpanishInquisition, SnakeEyes, PatientZero, Nosferatu, CommonCold, Nostradamus, Hitman,
        Anomaly, FullCourseMeal, FullMoon
    ];
}
