using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Options.Roles.Impostor;

public sealed class MinerOptions : AbstractRoleOptionGroup<MinerRole>
{
    public override string GroupName => MiraLocaleManager.Get("TownOfUsMira.Role.Miner", "Miner");

    [ModdedNumberOption("TouOptionMinerNumberOfVentsPerGame", 0f, 30f, 5f, MiraNumberSuffixes.None, "0", true)]
    public float MaxMines { get; set; } = 0f;

    [ModdedNumberOption("TouOptionMinerMineCooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float MineCooldown { get; set; } = 25f;

    [ModdedEnumOption("TouOptionMinerMineVisibility", typeof(MineVisiblityOptions), ["TouOptionMinerDelayEnumImmediate", "TouOptionMinerDelayEnumAfterUse"])]
    public MineVisiblityOptions MineVisibility { get; set; } = MineVisiblityOptions.Immediate;

    public ModdedNumberOption MineDelay { get; } = new("TouOptionMinerMineDelay", 3f, 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)
    {
        Visible = () => OptionGroupSingleton<MinerOptions>.Instance.MineVisibility is MineVisiblityOptions.Immediate
    };

    [ModdedToggleOption("TouOptionMinerCanKillWithTeammate")]
    public bool MinerKill { get; set; } = true;
}

public enum MineVisiblityOptions
{
    Immediate,
    AfterUse
}