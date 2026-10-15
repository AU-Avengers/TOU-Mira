using AchievementsAPI.API;
using UnityEngine;

namespace TownOfUs.Achievements;

public class TouNeutRoleAchievementsTab : TouAchievementsTab
{
    /// <inheritdoc/>
    public override StringNames Name => MiraLocaleManager.GetOrCreateLocaleString("TownOfUsMira.Achievements.Tab.Neutral");

    /// <inheritdoc/>
    public override string CategoryId => AchievementCategories.Neutral;

    /// <inheritdoc/>
    public override Color GetTabColor() => TownOfUsColors.Neutral;

    /// <inheritdoc/>
    public override Sprite GetIcon() => TouRoleIcons.Neutral.LoadAsset();

    public BaseBundleAchievement AmnesiaSquared { get; set; } = new("Amnesia²",
        "Attempt to remember a player who cannot be remembered.", TouNeutAssets.RememberButtonSprite, TouRoleIcons.Amnesiac);

    public BaseBundleAchievement FairlyBadParent { get; set; } = new("Fairly Bad Parent",
        "Have your target die in the first round.", TouAchAssets.FairlyBadParent, TouRoleIcons.Fairy);

    public BaseBundleAchievement Hangman { get; set; } = new("Hangman",
        "Get your target voted within 3 rounds.", TouNeutAssets.ExeTormentSprite, TouRoleIcons.Executioner, 1);

    public BaseBundleAchievement TrickyClown { get; set; } = new("Tricky Clown",
        "Get ejected within 3 rounds.", TouNeutAssets.JesterHauntSprite, TouRoleIcons.Jester, 1);

    public BaseBundleAchievement FinalAct { get; set; } = new("The Final Act",
        "Get ejected when three or less players remain.", TouNeutAssets.JesterHauntSprite, TouRoleIcons.Jester, 2);

    public BaseBundleAchievement DontPokeTheBear { get; set; } = new("Don't Poke the Bear",
        "Poke an alerting Veteran, gazing Medusa, or a Pestilence, and die to them.", TouNeutAssets.JesterPokeSprite, TouRoleIcons.Jester);

    public BaseBundleAchievement HeatOfTheBattle { get; set; } = new("Heat of the Battle",
        "Ignite 5 or more players at once.", TouNeutAssets.IgniteButtonSprite, TouRoleIcons.Arsonist, 2);

    /*public BaseBundleAchievement Inflammable { get; set; } = new("Inflammable",
        "Attempt to ignite on a protected player.", TouNeutAssets.DouseButtonSprite, TouRoleIcons.Arsonist);*/

    public BaseBundleAchievement Anomaly { get; set; } = new("Anomaly",
        "Get hacked as the Glitch.", TouNeutAssets.HackSprite, TouRoleIcons.Glitch, 1);

    public BaseBundleAchievement Ballistic { get; set; } = new("Ballistic",
        "Get a zero-second cooldown.", TouNeutAssets.JuggKillSprite, TouRoleIcons.Juggernaut, 2);

    public BaseBundleAchievement CommonCold { get; set; } = new("Common Cold",
        "Win the game without transforming into Pestilence", TouNeutAssets.InfectSprite, TouRoleIcons.Plaguebearer, 2);

    public CountBundleAchievement SnakeEyes { get; set; } = new("Snake Eyes",
        "Petrify 15 players with Stone Gaze.", TouNeutAssets.StoneGazeSprite, TouRoleIcons.Medusa, 0, 15, AchPersistence.ThroughoutSessions, 2);

    /*public BaseBundleAchievement ChangeOfHeart { get; set; } = new("Change of Heart",
        "Convert someone for the first time in a game when 5 or less players remain.", TouNeutAssets.BiteSprite, TouRoleIcons.Vampire, 1);*/

    public BaseBundleAchievement FullMoon { get; set; } = new("Full Moon",
        "End the game by killing all remaining players with a single Rampage.", TouNeutAssets.RampageSprite, TouRoleIcons.Werewolf, 2);

    public BaseBundleAchievement FullCourseMeal { get; set; } = new("Full-Course Meal",
        "Feed a Mini body and a Giant body in the same game.", TouAchAssets.FullCourseMeal, TouRoleIcons.Chef, 1);

    public BaseBundleAchievement SpanishInquisition { get; set; } = new("The Spanish Inquisition",
        "Vanquish all of your heretics by yourself.", TouNeutAssets.InquireSprite, TouRoleIcons.Inquisitor, 3);

    public BaseBundleAchievement Nostradamus { get; set; } = new("Nostradamus",
        "Guess 3 players correctly in a single meeting.", TouAssets.Guess, TouRoleIcons.Doomsayer, 2);

    public BaseBundleAchievement Hitman { get; set; } = new("Hitman",
        "Correctly guess 2 players in a single meeting as Assassin.", TouAssets.ShootMeetingSprite, TouRoleIcons.RandomAny, 1);

    public BaseBundleAchievement PatientZero { get; set; } = new("Patient Zero",
        "Infect 5 players in a single round as Plaguebearer.", TouNeutAssets.InfectSprite, TouRoleIcons.Plaguebearer, 1);

    public CountBundleAchievement Nosferatu { get; set; } = new("Nosferatu",
        "Bite 3 players in a single game.", TouNeutAssets.BiteSprite, TouRoleIcons.Vampire,
        0, 3, AchPersistence.ThroughoutRounds, 1);
}