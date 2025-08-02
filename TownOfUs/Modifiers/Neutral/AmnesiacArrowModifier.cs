using UnityEngine;

namespace TownOfUs.Modifiers.Neutral;

public sealed class AmnesiacArrowModifier(DeadBody deadBody, Color color) : ArrowDeadBodyModifier(deadBody, color, 0)
{
    public override string ModifierName => $"{TouLocale.Get(TouNames.Amnesiac, "Amnesiac")}  Arrow";
}
