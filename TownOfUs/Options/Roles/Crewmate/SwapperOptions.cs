﻿using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using TownOfUs.Roles.Crewmate;

namespace TownOfUs.Options.Roles.Crewmate;

public sealed class SwapperOptions : AbstractOptionGroup<SwapperRole>
{
    public override string GroupName => "Swapper";

    [ModdedToggleOption("Swapper Can Call Button")]
    public bool CanButton { get; set; } = true;
}
