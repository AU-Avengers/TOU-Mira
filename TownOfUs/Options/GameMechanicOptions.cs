using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace TownOfUs.Options;

public sealed class GameMechanicOptions : AbstractOptionGroup
{
    public override string GroupName => "Game Mechanics";
    public override uint GroupPriority => 1;

    /*[ModdedToggleOption("Hide Names Out Of Sight")]
    public bool HideNamesOutOfSight { get; set; } = true;*/

    [ModdedToggleOption("Powerful Crew Continue The Game")]
    public bool CrewKillersContinue { get; set; } = true;

    [ModdedToggleOption("All Shields Flash Grey on Trigger")]
    public bool AnonymousShields { get; set; } = false;

    public ModdedEnumOption CleanedBodiesAppearance { get; set; } = new("Cleaned/Dissolved Bodies Appear as", (int)BodyVitalsMode.Missing,
        typeof(BodyVitalsMode), ["Missing (MIS)", "Dead (DED)", "Disconnected (D/C)"]);

    public ModdedEnumOption KillAnimationBackgroundColor { get; set; } = new("Kill Animation Background Color", (int)KillColor.Red,
        typeof(KillColor), ["Red", "Faction", "Role Color"]);

    public ModdedNumberOption PlayerCountWhenVentsDisable { get; set; } = new("Max Players Alive When Vents Disable",
        2f, 1f, 15f, 1f, MiraNumberSuffixes.None, "0.#");

    public ModdedToggleOption GhostwalkerFixSabos { get; set; } = new("Ghostwalkers Can Fix Sabotages", false);

    [ModdedNumberOption("Temp Save Cooldown Reset", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds, "0.#")]
    public float TempSaveCdReset { get; set; } = 5f;
    public ModdedToggleOption RoleblockAffectsConsoles { get; set; } = new("Roleblock Affects Non-Role Actions", false);
    public ModdedEnumOption ShowPetsMode { get; set; } = new("Pet Visibility", (int)PetVisiblity.AlwaysVisible,
        typeof(PetVisiblity), ["Client Side", "When Alive", "Always Visible"]);

    public ModdedToggleOption HidePetsOnBodyRemove { get; set; } = new("Remove Pets Upon Janitor/Chef Clean", true)
    {
        Visible = () => (PetVisiblity)OptionGroupSingleton<GameMechanicOptions>.Instance.ShowPetsMode.Value is PetVisiblity.AlwaysVisible
    };
}

public enum BodyVitalsMode
{
    Missing,
    Dead,
    Disconnected
}

public enum KillColor
{
    Red,
    Faction,
    RoleColor
}