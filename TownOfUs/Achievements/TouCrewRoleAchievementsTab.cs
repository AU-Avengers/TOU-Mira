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
        "Sense 5 abilities being used in your radius throughout a game.", TouRoleIcons.Aurial, 0, 5, false);

    public BaseBundleAchievement StrangerDanger { get; set; } = new("Stranger Danger",
        "Sense a kill happen within your uncolored radius.", TouRoleIcons.Aurial);

    public BaseBundleAchievement BloodyHands { get; set; } = new("Bloody Hands",
        "Find a suspect on the first Examine attempt.", TouRoleIcons.Forensic);

    public BaseBundleAchievement RightOnTime { get; set; } = new("Right on Time",
        "Report a body quick enough to get the body's role / faction.", TouRoleIcons.Forensic);

    public BaseBundleAchievement SearchParty { get; set; } =
        new("Search Party", "Watch 5 players interact with someone.", TouRoleIcons.Lookout);

    public BaseBundleAchievement ParanormalActivity { get; set; } = new("Paranormal Activity",
        "Watch someone die without interactions.", TouRoleIcons.Lookout);

    public BaseBundleAchievement Purgatory { get; set; } =
        new("Purgatory", "Die while Mediating.", TouRoleIcons.Medium);
}