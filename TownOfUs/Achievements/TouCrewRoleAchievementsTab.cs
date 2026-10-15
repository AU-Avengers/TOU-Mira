using AchievementsAPI.API;
using UnityEngine;

namespace TownOfUs.Achievements;

public class TouCrewRoleAchievementsTab : TouAchievementsTab
{
    /// <inheritdoc/>
    public override StringNames Name => MiraLocaleManager.GetOrCreateLocaleString("TownOfUsMira.Achievements.Tab.Crewmate");

    /// <inheritdoc/>
    public override string CategoryId => AchievementCategories.Crewmate;

    /// <inheritdoc/>
    public override Color GetTabColor() => Palette.CrewmateRoleHeaderBlue;

    /// <inheritdoc/>
    public override Sprite GetIcon() => TouRoleIcons.Crewmate.LoadAsset();

    public CountBundleAchievement ForceBeWithYou { get; set; } = new("The Force Be With You",
        "Sense 5 abilities being used in your radius throughout a game.", TouRoleIcons.Aurial, TouRoleIcons.Aurial, 0, 5, AchPersistence.ThroughoutRounds, 1);

    public BaseBundleAchievement StrangerDanger { get; set; } = new("Stranger Danger",
        "Sense a kill happen within your uncolored radius.", TouRoleIcons.Aurial, TouRoleIcons.Aurial);

    public BaseBundleAchievement BloodyHands { get; set; } = new("Bloody Hands",
        "Find a suspect on the first Examine attempt.", TouAchAssets.BloodyHands, TouRoleIcons.Forensic, 1);

    public BaseBundleAchievement RightOnTime { get; set; } = new("Right on Time",
        "Report a body quick enough to get the body's role / faction.", TouAchAssets.RightOnTime, TouRoleIcons.Forensic);

    public BaseBundleAchievement SearchParty { get; set; } =
        new("Search Party", "Watch 5 players interact with someone.", TouCrewAssets.WatchSprite, TouRoleIcons.Lookout, 1);

    public BaseBundleAchievement ParanormalActivity { get; set; } = new("Paranormal Activity",
        "Watch someone die without interactions.", TouCrewAssets.WatchSprite, TouRoleIcons.Lookout, 1);

    public BaseBundleAchievement Purgatory { get; set; } =
        new("Purgatory", "Die while Mediating.", TouCrewAssets.MediateSprite, TouRoleIcons.Medium);

    /*public CountBundleAchievement TownOfHatred { get; set; } =
        new("Town of Hatred", "Find three unique pairs of enemies in a single game.", TouCrewAssets.IntuitSprite, TouRoleIcons.Seer, 0, 3, false);

    public BaseBundleAchievement RoleTwins { get; set; } =
        new("Role Twins", "Find friends between two non-crewmates with the same role or alignment.", TouCrewAssets.GazeSprite, TouRoleIcons.Seer);

    public BaseBundleAchievement TooSlow { get; set; } =
        new("Too Slow", "Die when you are revealed to the killers, but before you reveal them.", TouAssets.KillSprite, TouRoleIcons.Snitch, 1);

    public CountBundleAchievement Hyperfocused { get; set; } =
        new("Hyperfocused", "Keep track of more than 5 players at once.", TouCrewAssets.TrackSprite, TouRoleIcons.Sonar, 0, 5, false);

    public BaseBundleAchievement OffTheGlock { get; set; } =
        new("Off-The-Glock", "Die while on Admin Table.", TouAssets.AdminSprite, TouRoleIcons.Spy);

    public BaseBundleAchievement RoleCollector { get; set; } =
        new("Role Collector", "Identify at least 5 roles within a round.", TouCrewAssets.TrapSprite, TouRoleIcons.Trapper);

    public BaseBundleAchievement Sharpshooter { get; set; } =
        new("Sharpshooter", "Blast a player within 30 seconds of the meeting.", TouCrewAssets.CampButtonSprite, TouRoleIcons.Deputy, 1);

    public BaseBundleAchievement ThisTownAintBigEnough { get; set; } =
        new("This Town Ain't Big Enough", "Blast an Assassin.", TouAssets.ShootMeetingSprite, TouRoleIcons.Deputy);

    public BaseBundleAchievement BecomeTheHunted { get; set; } =
        new("Become the Hunted", "Get killed by your stalked target.", TouCrewAssets.StalkButtonSprite, TouRoleIcons.Hunter);

    public BaseBundleAchievement Punisher { get; set; } =
        new("Punisher", "Kill an evil from Retribution.", TouCrewAssets.HunterKillSprite, TouRoleIcons.Hunter, 1);

    public CountBundleAchievement HuntingSeason { get; set; } =
        new("Hunting Season", "Successfully stalk and catch 3 players in a game.", TouCrewAssets.StalkButtonSprite, TouRoleIcons.Hunter, 0, 3, false);

    public BaseBundleAchievement TimeCop { get; set; } =
        new("Time Cop", "Shoot your killer after being brought back by the Time Lord.", TouCrewAssets.OfficerLoadSprite, TouRoleIcons.Officer, 2);

    public BaseBundleAchievement Avenger { get; set; } =
        new("Avenger", "Shoot the killer of anyone who died near you.", TouCrewAssets.SheriffShootSprite, TouRoleIcons.Sheriff);

    public CountBundleAchievement Bloodshed { get; set; } =
        new("Bloodshed", "Kill more than 3 evils throughout a game.", TouCrewAssets.AlertSprite, TouRoleIcons.Veteran, 0, 3, false, 1);

    public BaseBundleAchievement GoFetch { get; set; } =
        new("Go Fetch", "Kill a Werewolf while on alert.", TouCrewAssets.AlertSprite, TouRoleIcons.Veteran);

    public BaseBundleAchievement ShotInTheDark { get; set; } =
        new("Shot in the Dark", "Shoot successfully after losing all other safe shots.", TouAssets.Guess, TouRoleIcons.Vigilante);

    public CountBundleAchievement TheDarkKnight { get; set; } =
        new("The Dark Knight", "Successfully guess 3 players throughout a round.", TouAssets.Guess, TouRoleIcons.Vigilante, 0, 3, false);*/

    public BaseBundleAchievement ToKillAGod { get; set; } =
        new("To Kill a God", "Attempt to execute a Horseman or invulnerable player.", TouAssets.ExecuteCleanSprite, TouRoleIcons.Jailor, 2, true)
    {
        ToastTiming = AchToastTiming.OnGameEnd
    };

    public BaseBundleAchievement NoChances { get; set; } =
        new("No Chances", "Execute an Evil without talking to them.", TouCrewAssets.JailSprite, TouRoleIcons.Jailor, 1);

    /*public BaseBundleAchievement FallenKnights { get; set; } =
        new("Fallen Knights", "Win and survive after all Knights die.", TouCrewAssets.KnightSprite, TouRoleIcons.Monarch);

    public BaseBundleAchievement BostonTeaParty { get; set; } =
        new("Boston Tea Party", "Get voted out by a Knight.", TouCrewAssets.KnightSprite, TouRoleIcons.Monarch);

    public BaseBundleAchievement CircusPerformer { get; set; } =
        new("Circus Performer", "Knight a Jester that wins.", TouCrewAssets.KnightSprite, TouRoleIcons.Monarch, 2, true);*/

    public BaseBundleAchievement RiggedVotes { get; set; } =
        new("Rigged Votes", "Get knighted as a revealed Mayor.", TouCrewAssets.KnightSprite, TouRoleIcons.Mayor, 2);

    public BaseBundleAchievement CrewmatesUnite { get; set; } =
        new("Crewmates Unite", "Only campaign Crewmates, and then reveal.", TouCrewAssets.CampaignButtonSprite, TouRoleIcons.Politician, 2, true)
    {
        ToastTiming = AchToastTiming.OnGameEnd
    };

    public BaseBundleAchievement DeathByDemocracy { get; set; } =
        new("Death by Democracy", "Reveal in a 1v1.", TouAchAssets.DeathByDemocracy, TouRoleIcons.Mayor, 3, true);

    public BaseBundleAchievement SwitchedFate { get; set; } = new("Switched Fate",
        "Swap the votes so an Impostor is ejected instead of a Crewmate.", TouAssets.SwapActive, TouRoleIcons.Swapper, 2)
    {
        ToastTiming = AchToastTiming.OnGameEnd
    };

    public BaseBundleAchievement KnightInShiningArmor { get; set; } = new("Knight in Shining Armor",
        "Knight a Crewmate who then kills an Impostor or Neutral Killer.", TouCrewAssets.KnightSprite, TouRoleIcons.Monarch, 2)
    {
        ToastTiming = AchToastTiming.OnGameEnd
    };

    public BaseBundleAchievement Spotlight { get; set; } = new("Spotlight",
        "Reveal the Impostors as Snitch and have one ejected in the very next meeting.", TouAssets.RevealButtonSprite,
        TouRoleIcons.Snitch, 1);

    public BaseBundleAchievement JusticeServed { get; set; } = new("Justice Served",
        "Shoot the killer of a body you reported.", TouCrewAssets.SheriffShootSprite, TouRoleIcons.Sheriff, 1);

    public BaseBundleAchievement Exorcist { get; set; } = new("Exorcist",
        "Cleanse a Spellslinger's hex off a player.", TouCrewAssets.CleanseSprite, TouRoleIcons.Cleric, 1)
    {
        ToastTiming = AchToastTiming.OnGameEnd
    };

    public BaseBundleAchievement OpenAndShut { get; set; } = new("Open and Shut",
        "Prosecute an Impostor.", TouAssets.ProsecuteMeetingSprite, TouRoleIcons.Prosecutor, 1)
    {
        ToastTiming = AchToastTiming.OnGameEnd
    };

    public BaseBundleAchievement SixthSense { get; set; } = new("Sixth Sense",
        "Report a body within 15 seconds of sensing its death as Mystic.", TouCrewAssets.InspectSprite, TouRoleIcons.Mystic, 1);

    public BaseBundleAchievement TooCloseForComfort { get; set; } = new("Too Close for Comfort",
        "Kill 2 players during a single Alert.", TouCrewAssets.AlertSprite, TouRoleIcons.Veteran, 1);

    public BaseBundleAchievement DoubleTap { get; set; } = new("Double Tap",
        "Correctly guess 2 players in a single meeting as Vigilante.", TouAssets.Guess, TouRoleIcons.Vigilante, 1);

    public BaseBundleAchievement HeroHour { get; set; } = new("Hero Hour",
        "Fix a critical sabotage with less than 5 seconds remaining.", TouCrewAssets.FixButtonSprite, TouRoleIcons.Engineer, 2);

    public BaseBundleAchievement FlushedOut { get; set; } = new("Flushed Out",
        "Flush a player out of a vent as Plumber.", TouCrewAssets.FlushSprite, TouRoleIcons.Plumber, 1)
    {
        ToastTiming = AchToastTiming.OnGameEnd
    };

    public BaseBundleAchievement MethodActor { get; set; } = new("Method Actor",
        "Imitate 3 different roles in a single game.", TouAssets.ImitateSelectSprite, TouRoleIcons.Imitator, 1);

    public CountBundleAchievement IronWall { get; set; } = new("Iron Wall",
        "Have your shield block 3 attacks in a single game as Medic.", TouCrewAssets.BarrierSprite, TouRoleIcons.Medic,
        0, 3, AchPersistence.ThroughoutRounds, 1);

    public BaseBundleAchievement MassResurrection { get; set; } = new("Mass Resurrection",
        "Revive 2 or more players with a single Revive.", TouCrewAssets.ReviveSprite, TouRoleIcons.Altruist, 2);

    public BaseBundleAchievement Undo { get; set; } = new("Undo",
        "Bring back 2 or more players with a single Rewind.", TouCrewAssets.RewindSprite, TouRoleIcons.TimeLord, 1);

    public BaseBundleAchievement Poltergeist { get; set; } = new("Poltergeist",
        "Reveal the evils as Haunter.", TouAssets.RevealCleanSprite, TouRoleIcons.Haunter);

    /*public BaseBundleAchievement AAAAAAAAAAAAAAAAAAA { get; set; } =
        new("AAAAAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", TouCrewAssets.RememberButtonSprite, TouRoleIcons.CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC);*/
}