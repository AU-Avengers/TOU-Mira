using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Crewmate;

/// <summary>
/// Clueless: removes all task guidance (task list, task arrows/markers, and map task locations).
/// Tasks still function normally and contribute to the task bar.
/// </summary>
public sealed class CluelessModifier : TouGameModifier, IWikiDiscoverable
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
    public override Color FreeplayFileColor => new Color32(140, 200, 255, 255);

    public override ModifierFaction FactionType => ModifierFaction.CrewmatePassive;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.CluelessChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.CluelessAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        // Crewmates only (no neutral/impostor), and keep Spectator excluded via base.
        return base.IsModifierValidOn(role) && role.IsCrewmate();
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


