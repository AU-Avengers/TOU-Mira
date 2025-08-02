using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class MarshalOptions : AbstractOptionGroup<MarshalRole>
{
    public override string GroupName => "Marshal";

    [ModdedNumberOption("Amount Of Tribunals", 1, 5)]
    public float MaxTribunals { get; set; } = 2;
    
    [ModdedNumberOption("Max Ejections During A Tribunal", 2, 5)]
    public float MaxTribunalEjections { get; set; } = 2;
    
    [ModdedNumberOption("Round When Tribunal Is Available", 1, 5)]
    public float RoundWhenAvailable { get; set; } = 1;
    
    [ModdedNumberOption("Marshal Extra Votes During A Tribunal", 0, 3)]
    public float MarshalExtraVotes { get; set; } = 1;

    [ModdedToggleOption("Marshal Reveals When A Tribunal Is Called")]
    public bool RevealMarshal { get; set; } = true;
}