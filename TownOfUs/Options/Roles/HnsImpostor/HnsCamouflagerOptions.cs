using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.HnsImpostor;

namespace TownOfUs.Options.Roles.HnsImpostor;

public sealed class HnsCamouflagerOptions : AbstractOptionGroup<HnsCamouflagerRole>
{
    public override string GroupName => TouLocale.Get("HnsRoleCamouflager", "Camouflager");

    [ModdedNumberOption("HnsOptionCamouflagerCamoUses", 1f, 10f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxCamoUses { get; set; } = 3f;

    [ModdedNumberOption("HnsOptionCamouflagerCamoCooldown", 10f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CamoCooldown { get; set; } = 25f;

    [ModdedNumberOption("HnsOptionCamouflagerCamoDuration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CamoDuration { get; set; } = 15f;

    public ModdedToggleOption CamoDisablesProxBar { get; set; } = new("HnsOptionCamouflagerCamoDisablesProxBar", true);
}