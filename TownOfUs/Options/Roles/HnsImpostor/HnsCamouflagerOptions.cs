using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.HideAndSeek.Seeker;

namespace TownOfUs.Options.Roles.HnsImpostor;

public sealed class HnsCamouflagerOptions : AbstractRoleOptionGroup<HnsCamouflagerRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.HideAndSeek.Role.Camouflager", "Camouflager");

    [ModdedNumberOption("TownOfUsMira.HideAndSeek.Role.Option.CamouflagerCamoUses", 1f, 10f, 1f, MiraNumberSuffixes.None, "0", true)]
    public float MaxCamoUses { get; set; } = 3f;

    [ModdedNumberOption("TownOfUsMira.HideAndSeek.Role.Option.CamouflagerCamoCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CamoCooldown { get; set; } = 25f;

    [ModdedNumberOption("TownOfUsMira.HideAndSeek.Role.Option.CamouflagerCamoDuration", 5f, 15f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float CamoDuration { get; set; } = 15f;

    public ModdedToggleOption CamoDisablesProxBar { get; set; } = new("TownOfUsMira.HideAndSeek.Role.Option.CamouflagerCamoDisablesProxBar", true);
}