using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class VanillaTweakOptions : AbstractOptionGroup
{
    public override string GroupName => "Vanilla Tweaks";
    public override uint GroupPriority => 1;

    /*[ModdedToggleOption("Hide Names Out Of Sight")]
    public bool HideNamesOutOfSight { get; set; } = true;*/

    public ModdedNumberOption PlayerCountWhenVentsDisable { get; set; } = new("Max Players Alive When Vents Disable",
        2f, 1f, 15f, 1f, MiraNumberSuffixes.None, "0.#");

    public ModdedToggleOption TickCooldownsInMinigame { get; set; } = new("Continue Cooldown In Tasks and Panels", true);

    public ModdedToggleOption ParallelMedbay { get; set; } = new("Parallel Medbay Scans", true);

    public ModdedToggleOption MedscanWalk { get; set; } = new("Walk to Medscan", true);

    public ModdedEnumOption SkipButtonDisable { get; set; } = new("Disable Meeting Skip Button", (int)SkipState.No,
        typeof(SkipState), ["Never", "Emergency", "Always"]);

    public ModdedToggleOption HideVentAnimationNotInVision { get; set; } =
        new("Hide Vent Animations Not In Vision", true);

    public ModdedEnumOption ShowPetsMode { get; set; } = new("Pet Visibility", (int)PetVisiblity.AlwaysVisible,
        typeof(PetVisiblity), ["Client Side", "When Alive", "Always Visible"]);

    public ModdedToggleOption HidePetsOnBodyRemove { get; set; } = new("Remove Pets Upon Janitor/Chef Clean", true)
    {
        Visible = () => (PetVisiblity)OptionGroupSingleton<VanillaTweakOptions>.Instance.ShowPetsMode.Value is PetVisiblity.AlwaysVisible
    };

    public bool CanPauseCooldown => !TickCooldownsInMinigame.Value &&
                                 (Minigame.Instance && Minigame.Instance is not IngameWikiMinigame);
}

public enum PetVisiblity
{
    ClientSide,
    WhenAlive,
    AlwaysVisible
}

public enum SkipState
{
    No,
    Emergency,
    Always
}