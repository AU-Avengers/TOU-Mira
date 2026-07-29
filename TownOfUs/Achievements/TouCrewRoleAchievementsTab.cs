using AchievementsAPI.API;
using UnityEngine;

namespace TownOfUs.Achievements;

public class TouCrewRoleAchievementsTab : AchievementsTab
{
    /// <inheritdoc/>
    public override string Name => "Crew Achievements";

    /// <inheritdoc/>
    public override Color GetTabColor() => Palette.CrewmateRoleHeaderBlue;

    /// <inheritdoc/>
    public override Sprite GetIcon() => TouRoleIcons.Crewmate.LoadAsset();

    public CountBundleAchievement ForceBeWithYou { get; set; } = new("The Force Be With You",
        "Sense 5 abilities being used in your radius throughout a game.", TouRoleIcons.Aurial, TouRoleIcons.Aurial, 0, 5, AchPersistence.ThroughoutRounds);

    public BaseBundleAchievement StrangerDanger { get; set; } = new("Stranger Danger",
        "Sense a kill happen within your uncolored radius.", TouRoleIcons.Aurial, TouRoleIcons.Aurial);

    public BaseBundleAchievement BloodyHands { get; set; } = new("Bloody Hands",
        "Find a suspect on the first Examine attempt.", TouAchAssets.BloodyHands, TouRoleIcons.Forensic);

    public BaseBundleAchievement RightOnTime { get; set; } = new("Right on Time",
        "Report a body quick enough to get the body's role / faction.", TouAchAssets.RightOnTime, TouRoleIcons.Forensic);

    public BaseBundleAchievement SearchParty { get; set; } =
        new("Search Party", "Watch 5 players interact with someone.", TouCrewAssets.WatchSprite, TouRoleIcons.Lookout);

    public BaseBundleAchievement ParanormalActivity { get; set; } = new("Paranormal Activity",
        "Watch someone die without interactions.", TouCrewAssets.WatchSprite, TouRoleIcons.Lookout);

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
        new("To Kill a God", "Attempt to execute a Horseman or invulnerable player.", TouAssets.ExecuteCleanSprite, TouRoleIcons.Jailor);

    public BaseBundleAchievement NoChances { get; set; } =
        new("No Chances", "Execute an Evil without talking to them.", TouCrewAssets.JailSprite, TouRoleIcons.Jailor);

    /*public BaseBundleAchievement FallenKnights { get; set; } =
        new("Fallen Knights", "Win and survive after all Knights die.", TouCrewAssets.KnightSprite, TouRoleIcons.Monarch);

    public BaseBundleAchievement BostonTeaParty { get; set; } =
        new("Boston Tea Party", "Get voted out by a Knight.", TouCrewAssets.KnightSprite, TouRoleIcons.Monarch);

    public BaseBundleAchievement CircusPerformer { get; set; } =
        new("Circus Performer", "Knight a Jester that wins.", TouCrewAssets.KnightSprite, TouRoleIcons.Monarch, 2, true);*/

    public BaseBundleAchievement RiggedVotes { get; set; } =
        new("Rigged Votes", "Get knighted as a revealed Mayor.", TouCrewAssets.KnightSprite, TouRoleIcons.Mayor, 1);

    public BaseBundleAchievement CrewmatesUnite { get; set; } =
        new("Crewmates Unite", "Only campaign Crewmates, and then reveal.", TouCrewAssets.CampaignButtonSprite, TouRoleIcons.Politician, 2, true);

    public BaseBundleAchievement DeathByDemocracy { get; set; } =
        new("Death by Democracy", "Reveal in a 1v1.", TouAchAssets.DeathByDemocracy, TouRoleIcons.Mayor, 3, true);

    /*public BaseBundleAchievement AAAAAAAAAAAAAAAAAAA { get; set; } =
        new("AAAAAAAAAAAAAAAAAAA", "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB", TouCrewAssets.RememberButtonSprite, TouRoleIcons.CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC);*/
}