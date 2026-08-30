using MiraAPI.GameOptions.Attributes;
using TownOfUs.Modifiers.Game.Impostor;
using UnityEngine;

namespace TownOfUs.Options.Modifiers.Impostor;

public sealed class CircumventOptions : AbstractTouModifierOptionGroup<CircumventModifier>
{
    public override string GroupName => MiraLocaleManager.Get("TouModifierCircumvent", "Circumvent");
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;
    public override uint GroupPriority => 40;

    [ModdedNumberOption("TouOptionCircumventMinimumVents", 0f, 10f, 1f)]
    public float VentsMin { get; set; } = 3f;

    [ModdedNumberOption("TouOptionCircumventMaximumVents", 0f, 10f, 1f)]
    public float VentsMax { get; set; } = 10f;

    /// <summary>
    /// Picks the quota using Min/Max or falls back to Max if invalid
    /// </summary>
    public int GenerateUsesCount()
    {
        var min = Mathf.FloorToInt(VentsMin);
        var max = Mathf.FloorToInt(VentsMax);

        return UnityEngine.Random.Range(min, max + 1);
    }
}