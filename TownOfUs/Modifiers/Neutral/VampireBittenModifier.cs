﻿using MiraAPI.Modifiers;

namespace TownOfUs.Modifiers.Neutral;

public sealed class VampireBittenModifier : BaseModifier
{
    public override string ModifierName => "Bitten";
    public override bool HideOnUi => true;
}