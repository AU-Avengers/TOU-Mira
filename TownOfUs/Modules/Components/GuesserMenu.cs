using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Interfaces;

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

            var allRoles = MiscUtils.AllRoles.Where(roleMatch).Where(x => x is IGuessable && !roles.Contains(x)).ToList();

            if (allRoles.Count > 0)
            {
                foreach (var addedRole in allRoles)
                {
                    if (addedRole is IGuessable guessable && guessable.CanBeGuessed)
                    {
                        roles.Add(addedRole);
                    }
                }
            }

            var newRoleList = roles.OrderBy(x =>
            LocalSettingsTabSingleton<TouLocalTabPreferences>.Instance.SortGuessingByAlignmentToggle.Value
                ? MiscUtils.GetParsedRoleAlignment(x) + x.GetRoleName()
                : x.GetRoleName()).ToList();

            RegisterPanels(
                newRoleList,
                onRoleClick!,
                Utilities.Extensions.SetRole,
                (shapeshifterPanel, role) => new MenuEntry(shapeshifterPanel, role.GetRoleName())
            );

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
                shapeshifterPanel.gameObject.transform.FindChild("Nameplate").FindChild("Highlight")
                    .FindChild("ShapeshifterIcon").gameObject.SetActive(false);
            }
        });
    }
}
