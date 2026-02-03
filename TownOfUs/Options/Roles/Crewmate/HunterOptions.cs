using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class HunterOptions : AbstractOptionGroup<HunterRole>
{
    public override string GroupName => TouLocale.Get("TouRoleHunter", "Hunter");

    [ModdedNumberOption("TouOptionHunterKillCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HunterKillCooldown { get; set; } = 25f;

    [ModdedNumberOption("TouOptionHunterStalkCooldown", 1f, 30f, 1f, MiraNumberSuffixes.Seconds)]
    public float HunterStalkCooldown { get; set; } = 20f;

    [ModdedNumberOption("TouOptionHunterStalkDuration", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HunterStalkDuration { get; set; } = 25f;

    public ModdedNumberOption StalkUses { get; } = new("TouOptionHunterStalkUses", 5f, -1f, 30f, 1f, "0", "∞",
        MiraNumberSuffixes.None, "0");

    public ModdedNumberOption StalkPerTasks { get; } = new("TouOptionHunterStalksPerTasks", 1f, 0f, 15f, 1f, "Off", "#",
        MiraNumberSuffixes.None, "0")
    {
        Visible = () => OptionGroupSingleton<HunterOptions>.Instance.StalkUses != -1
    };

    public ModdedEnumOption StalkTriggeredBy { get; set; } = new("TouOptionHunterStalkTriggeredBy",
        (int)StalkTriggered.Always, typeof(StalkTriggered),
        [
            "TouOptionHunterStalkTriggerEnumAlways", "TouOptionHunterStalkTriggerEnumInteraction"
        ]);

    public ModdedToggleOption SeesTypeOfInteraction { get; } = new("TouOptionHunterSeesTypeOfInteraction", true)
    {
        Visible = () =>
            (StalkTriggered)OptionGroupSingleton<HunterOptions>.Instance.StalkTriggeredBy.Value is StalkTriggered.Always
    };

    [ModdedToggleOption("TouOptionHunterRetributionOnVote")]
    public bool RetributionOnVote { get; set; } = true;

    [ModdedToggleOption("TouOptionHunterHunterBodyReport")]
    public bool HunterBodyReport { get; set; } = false;
}

public enum StalkTriggered
{
    Always,
    Interactions
}