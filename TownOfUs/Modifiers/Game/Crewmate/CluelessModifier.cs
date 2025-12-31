using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Universal;

/// <summary>
/// Clueless: removes all task guidance (task list, task arrows/markers, and map task locations).
/// Tasks still function normally and contribute to the task bar.
/// </summary>
public sealed class CluelessModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override string LocaleKey => "Clueless";
    public override string ModifierName => TouLocale.Get($"TouModifier{LocaleKey}");
    public override string IntroInfo => TouLocale.GetParsed($"TouModifier{LocaleKey}IntroBlurb");

    public override string GetDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}TabDescription");
    }

    public string GetAdvancedDescription()
    {
        return TouLocale.GetParsed($"TouModifier{LocaleKey}WikiDescription")
               + MiscUtils.AppendOptionsText(GetType());
    }

    // Intentionally left null to avoid requiring a new bundle sprite for compilation/runtime safety.
    public override LoadableAsset<Sprite>? ModifierIcon => null;
    public override Color FreeplayFileColor => new Color32(180, 180, 180, 255);

    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.CluelessChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.CluelessAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        // Any role can be Clueless (keeps Spectator excluded via base).
        return base.IsModifierValidOn(role);
    }

    public override void OnActivate()
    {
        base.OnActivate();

        // Ensure guidance is removed immediately if this modifier is applied mid-round (e.g. via Freeplay).
        if (!Player.AmOwner)
        {
            return;
        }

        try
        {
            // Clear task list text immediately.
            if (HudManager.Instance != null && HudManager.Instance.TaskPanel != null &&
                HudManager.Instance.TaskPanel.taskText != null)
            {
                HudManager.Instance.TaskPanel.taskText.text = string.Empty;
            }

            // If the map is currently open, hide task overlay.
            if (MapBehaviour.Instance != null)
            {
                MapBehaviour.Instance.taskOverlay?.Hide();
            }
        }
        catch
        {
            // ignored
        }
    }
}


