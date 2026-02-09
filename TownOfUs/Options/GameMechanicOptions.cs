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

    public ModdedToggleOption GhostwalkerFixSabos { get; set; } = new("Ghostwalkers Can Fix Sabotages", false);

    public ModdedEnumOption ShowPetsMode { get; set; } = new("Pet Visibility", (int)PetVisiblity.AlwaysVisible,
        typeof(PetVisiblity), ["Client Side", "When Alive", "Always Visible"]);

    public ModdedToggleOption HidePetsOnBodyRemove { get; set; } = new("Remove Pets Upon Janitor/Chef Clean", true)
    {
        Visible = () => (PetVisiblity)OptionGroupSingleton<GameMechanicOptions>.Instance.ShowPetsMode.Value is PetVisiblity.AlwaysVisible
    };

    [ModdedNumberOption("Temp Save Cooldown Reset", 0f, 15f, 0.5f, MiraNumberSuffixes.Seconds, "0.#")]
    public float TempSaveCdReset { get; set; } = 5f;
}

public enum PetVisiblity
{
    ClientSide,
    WhenAlive,
    AlwaysVisible
}