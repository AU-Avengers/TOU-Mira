using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;
using UnityEngine;

namespace TownOfUs.Modules.Components;

public sealed class GuesserMenu : CustomPaginableMenu
{
    protected override string Name => "Guesser";
    protected override TextBoxTMP? PrefabTextbox => TouAssets.WikiPrefab.LoadAsset().GetComponentInChildren<TextBoxTMP>(true);

    public static GuesserMenu Create()
    {
        return Create<GuesserMenu>();
    }

    [HideFromIl2Cpp]
    public void Begin(Func<RoleBehaviour, bool> roleMatch, Action<RoleBehaviour> onRoleClick,
        Func<BaseModifier, bool>? modifierMatch = null, Action<BaseModifier>? onModifierClick = null)
    {
        Begin(() =>
        {
            var roles = MiscUtils.GetPotentialRoles().Where(roleMatch).ToList();

            var guessableRoles = MiscUtils.AllRoles.Where(roleMatch)
                                .Where(x => x is IGuessable guessable && !roles.Contains(x) && guessable.CanBeGuessed).ToList();

            var newRoleList = roles.Concat(guessableRoles).OrderBy(x =>
            LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.SortGuessingByAlignmentToggle.Value
                ? MiscUtils.GetParsedRoleAlignment(x) + x.GetRoleName()
                : x.GetRoleName()).ToList();

            RegisterPanels(
                newRoleList,
                onRoleClick!,
                Utilities.Extensions.SetRole,
                (shapeshifterPanel, role) => new MenuEntry(shapeshifterPanel, role.GetRoleName())
            );

            foreach (var shapeshifterPanel in EntryPanels)
            {
                var highlight = shapeshifterPanel.gameObject.transform.FindChild("Nameplate").FindChild("Highlight");
                highlight.FindChild("ShapeshifterIcon").gameObject.SetActive(false);
                highlight.GetComponent<SpriteRenderer>().transform.GetChild(0).gameObject.SetActive(false);
            }

            if (modifierMatch == null || onModifierClick == null)
                return;

            var modifiers = MiscUtils.AllModifiers.Where(modifierMatch).OrderBy(x => x.ModifierName).ToList();

            RegisterPanels(
                modifiers,
                onModifierClick!,
                Utilities.Extensions.SetModifier,
                (shapeshifterPanel, modifier) => new MenuEntry(shapeshifterPanel, modifier.ModifierName)
            );

            foreach (var shapeshifterPanel in EntryPanels)
            {
                var highlight = shapeshifterPanel.gameObject.transform.FindChild("Nameplate").FindChild("Highlight");
                highlight.FindChild("ShapeshifterIcon").gameObject.SetActive(false);
                highlight.GetComponent<SpriteRenderer>().transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            }
        });
    }
}
