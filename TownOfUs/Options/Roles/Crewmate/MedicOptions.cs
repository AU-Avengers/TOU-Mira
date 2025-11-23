using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MedicOptions : AbstractOptionGroup<MedicRole>
{
    public override string GroupName => TouLocale.Get("TouRoleMedic", "Medic");

    [ModdedEnumOption("TouOptionMedicShowShieldedPlayer", typeof(MedicOption),
        ["TouOptionMedicShieldEnumMedic", "TouOptionMedicShieldEnumShielded", "TouOptionMedicShieldEnumShieldedAndMedic", "TouOptionMedicShieldEnumEveryone", "TouOptionMedicShieldEnumNobody"])]
    public MedicOption ShowShielded { get; set; } = MedicOption.ShieldedAndMedic;

    [ModdedEnumOption("TouOptionMedicWhoGetsMurderAttemptIndicator", typeof(MedicOption),
        ["TouOptionMedicShieldEnumMedic", "TouOptionMedicShieldEnumShielded", "TouOptionMedicShieldEnumShieldedAndMedic", "TouOptionMedicShieldEnumEveryone", "TouOptionMedicShieldEnumNobody"])]
    public MedicOption WhoGetsNotification { get; set; } = MedicOption.Medic;

    [ModdedToggleOption("TouOptionMedicCanGiveShieldAwayNextRound")]
    public bool ChangeTarget { get; set; } = true;

    public ModdedNumberOption MedicShieldUses { get; } = new("TouOptionMedicMaxAmountOfShieldUses", 3f, 0f, 15f,
        1f, MiraNumberSuffixes.None, "0", true)
    {
        Visible = () => OptionGroupSingleton<MedicOptions>.Instance.ChangeTarget
    };

    [ModdedToggleOption("TouOptionMedicShieldBreaksOnMurderAttempt")]
    public bool ShieldBreaks { get; set; } = false;

    [ModdedToggleOption("TouOptionMedicShowReportsInChat")]
    public bool ShowReports { get; set; } = true;

    public ModdedNumberOption MedicReportNameDuration { get; } = new("TouOptionMedicTimeWhereMedicWillHaveName", 0f, 0f, 60f,
        2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedicOptions>.Instance.ShowReports
    };

    public ModdedNumberOption MedicReportColorDuration { get; } = new("TouOptionMedicTimeWhereMedicWillHaveColorType", 15, 0f,
        60f, 2.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MedicOptions>.Instance.ShowReports
    };
}

public enum MedicOption
{
    Medic,
    Shielded,
    ShieldedAndMedic,
    Everyone,
    Nobody
}