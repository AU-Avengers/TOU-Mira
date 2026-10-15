using AchievementsAPI.API;
using UnityEngine;

namespace TownOfUs.Achievements;

public class TouImpRoleAchievementsTab : TouAchievementsTab
{
    /// <inheritdoc/>
    public override StringNames Name => MiraLocaleManager.GetOrCreateLocaleString("TownOfUsMira.Achievements.Tab.Impostor");

    /// <inheritdoc/>
    public override string CategoryId => AchievementCategories.Impostor;

    /// <inheritdoc/>
    public override Color GetTabColor() => Palette.ImpostorRoleHeaderRed;

    /// <inheritdoc/>
    public override Sprite GetIcon() => TouRoleIcons.Impostor.LoadAsset();

    public CountBundleAchievement EternalDarkness { get; set; } = new("Eternal Darkness", "Kill three blinded players in the same game.",
        TouImpAssets.BlindSprite, TouRoleIcons.Eclipsal, 0, 3, AchPersistence.ThroughoutRounds, 1);

    /*public BaseBundleAchievement OhHenry { get; set; } = new("Oh Henry!",
        "Recall into a room where a player is already at.", TouImpAssets.RecallSprite, TouRoleIcons.Escapist);*/

    public BaseBundleAchievement BlindFoEva { get; set; } = new("Blind Fo'Eva",
        "Flashbang the same player 3 times in a single game.", TouImpAssets.FlashSprite, TouRoleIcons.Grenadier, 1);

    public BaseBundleAchievement IdentityCrisis { get; set; } = new("Identity Crisis",
        "Kill the player you are morphed as.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling, 1);

    /*public BaseBundleAchievement Framer { get; set; } = new("Framer",
        "Frame and morph as a player for a kill and get them voted out in the same round.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling);*/

    public BaseBundleAchievement SizeDoesntMatter { get; set; } = new("Size Doesn't Matter",
        "Morph as a Giant into a Mini player, or vice-versa.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling, 1);

    public CountBundleAchievement Untraceable { get; set; } =
        new("Untraceable", "Kill 15 players while Swooped.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper, 0, 15, AchPersistence.ThroughoutSessions, 2);

    /*public CountBundleAchievement Introvert { get; set; } =
        new("Introvert", "Swoop 3 times in a single round.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper, 0, 3, false);*/

    public BaseBundleAchievement Sneakman { get; set; } =
        new("Sneakman", "Kill and vent away during the same swoop.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper);

    public BaseBundleAchievement Fireworks { get; set; } = new("Fireworks",
        "Kill 3 or more players with a single bomb.", TouImpAssets.PlantSprite, TouRoleIcons.Bomber, 2);

    public BaseBundleAchievement BurstFire { get; set; } = new("Burst Fire",
        "Kill 2 players in a single Warlock charge.", TouAssets.KillSprite, TouRoleIcons.Warlock, 1);

    public BaseBundleAchievement HushMoney { get; set; } = new("Hush Money",
        "Blackmail the player who reported the body.", TouImpAssets.BlackmailSprite, TouRoleIcons.Blackmailer, 1);

    public BaseBundleAchievement MassHysteria { get; set; } = new("Mass Hysteria",
        "Trigger Hysteria with 4 or more hypnotized players.", TouAssets.MassHysteriaSprite, TouRoleIcons.Hypnotist, 1);

    public CountBundleAchievement Spotless { get; set; } = new("Spotless",
        "Clean 3 bodies in a single game.", TouImpAssets.CleanButtonSprite, TouRoleIcons.Janitor,
        0, 3, AchPersistence.ThroughoutRounds, 1);

    public CountBundleAchievement SubwaySystem { get; set; } = new("Subway System",
        "Place 5 vents in a single game as Miner.", TouImpAssets.MineSprite, TouRoleIcons.Miner,
        0, 5, AchPersistence.ThroughoutRounds, 1);

    public BaseBundleAchievement Detonation { get; set; } = new("Detonation",
        "Complete a Hex Bomb.", TouImpAssets.HexBombSprite, TouRoleIcons.Spellslinger, 2);

    public BaseBundleAchievement PerfectAmbush { get; set; } = new("Perfect Ambush",
        "Kill the player you were pursuing.", TouImpAssets.AmbushSprite, TouRoleIcons.Ambusher);
}