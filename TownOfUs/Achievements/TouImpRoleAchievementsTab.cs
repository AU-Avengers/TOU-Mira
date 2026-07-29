using AchievementsAPI.API;
using UnityEngine;

namespace TownOfUs.Achievements;

public class TouImpRoleAchievementsTab : AchievementsTab
{
    /// <inheritdoc/>
    public override string Name => "Impostor Achievements";

    /// <inheritdoc/>
    public override Color GetTabColor() => Palette.ImpostorRoleHeaderRed;

    /// <inheritdoc/>
    public override Sprite GetIcon() => TouRoleIcons.Impostor.LoadAsset();

    public CountBundleAchievement EternalDarkness { get; set; } = new("Eternal Darkness", "Kill three blinded players in the same game.",
        TouImpAssets.BlindSprite, TouRoleIcons.Eclipsal, 0, 3, AchPersistence.ThroughoutRounds);

    /*public BaseBundleAchievement OhHenry { get; set; } = new("Oh Henry!",
        "Recall into a room where a player is already at.", TouImpAssets.RecallSprite, TouRoleIcons.Escapist);*/

    public BaseBundleAchievement BlindFoEva { get; set; } = new("Blind Fo'Eva",
        "Flashbang the same player 3 times in a single game.", TouImpAssets.FlashSprite, TouRoleIcons.Grenadier);

    public BaseBundleAchievement IdentityCrisis { get; set; } = new("Identity Crisis",
        "Kill the player you are morphed as.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling);

    /*public BaseBundleAchievement Framer { get; set; } = new("Framer",
        "Frame and morph as a player for a kill and get them voted out in the same round.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling);*/

    public BaseBundleAchievement SizeDoesntMatter { get; set; } = new("Size Doesn't Matter",
        "Morph as a Giant into a Mini player, or vice-versa.", TouImpAssets.MorphSprite, TouRoleIcons.Morphling);

    public CountBundleAchievement Untraceable { get; set; } =
        new("Untraceable", "Kill 15 players while Swooped.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper, 0, 15);

    /*public CountBundleAchievement Introvert { get; set; } =
        new("Introvert", "Swoop 3 times in a single round.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper, 0, 3, false);*/

    public BaseBundleAchievement Sneakman { get; set; } =
        new("Sneakman", "Kill and vent away during the same swoop.", TouImpAssets.SwoopSprite, TouRoleIcons.Swooper);
}