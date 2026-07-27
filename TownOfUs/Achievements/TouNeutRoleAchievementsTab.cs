using AchievementsAPI.API;
using UnityEngine;

namespace TownOfUs.Achievements;

public class TouNeutRoleAchievementsTab : AchievementsTab
{
    /// <inheritdoc/>
    public override string Name => "Neut Achievements";

    /// <inheritdoc/>
    public override Color GetTabColor() => TownOfUsColors.Neutral;

    /// <inheritdoc/>
    public override Sprite GetIcon() => TouRoleIcons.Neutral.LoadAsset();

    public BaseBundleAchievement AmnesiaSquared { get; set; } = new("Amnesia²",
        "Attempt to remember a player who cannot be remembered.", TouRoleIcons.Amnesiac);

    public BaseBundleAchievement FairlyBadParent { get; set; } = new("Fairly Bad Parent",
        "Have your target die in the first round.", TouRoleIcons.Fairy);

    public BaseBundleAchievement Hangman { get; set; } = new("Hangman",
        "Get your target voted within 3 rounds.", TouRoleIcons.Executioner);

    public BaseBundleAchievement TrickyClown { get; set; } = new("Tricky Clown",
        "Get ejected within 3 rounds.", TouRoleIcons.Jester);

    public BaseBundleAchievement FinalAct { get; set; } = new("The Final Act",
        "Get ejected when three or less players remain.", TouRoleIcons.Jester);

    public BaseBundleAchievement DontPokeTheBear { get; set; } = new("Don't Poke the Bear",
        "Poke an alerting Veteran, gazing Medusa, or a Pestilence, and die to them.", TouRoleIcons.Jester);

    public BaseBundleAchievement Inflammable { get; set; } = new("Inflammable",
        "Attempt to ignite on a protected player.", TouRoleIcons.Arsonist);

    public BaseBundleAchievement Anomaly { get; set; } = new("Anomaly",
        "Get hacked as the Glitch.", TouRoleIcons.Glitch);

    public BaseBundleAchievement Ballistic { get; set; } = new("Ballistic",
        "Get a zero-second cooldown.", TouRoleIcons.Juggernaut);

    public BaseBundleAchievement CommonCold { get; set; } = new("Common Cold",
        "Win the game without transforming into Pestilence", TouRoleIcons.Plaguebearer);

    public CountBundleAchievement SnakeEyes { get; set; } = new("Snake Eyes",
        "Petrify 10 players with Stone Gaze.", TouRoleIcons.Medusa, 0, 10);

    public BaseBundleAchievement ChangeOfHeart { get; set; } = new("Change of Heart",
        "Convert a player when less than 5 players remain.", TouRoleIcons.Vampire);

    public BaseBundleAchievement FullMoon { get; set; } = new("Full Moon",
        "End the game by killing all remaining players with a single Rampage.", TouRoleIcons.Werewolf);
}