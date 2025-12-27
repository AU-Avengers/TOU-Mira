using System.Collections;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Interfaces;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TownOfUs.Modules.Components;

[RegisterInIl2Cpp]
public sealed class GuesserMenu(IntPtr cppPtr) : Minigame(cppPtr)
{
    private UiElement? backButton;
    private int currentPage;
    private UiElement? defaultButtonSelected;
    private Action<BaseModifier>? onModifierClick;
    private Action<RoleBehaviour>? onRoleClick;
    private ShapeshifterPanel? panelPrefab;
    private List<MenuEntry> allEntries = [];

    private TextBoxTMP? searchTextbox;
    private string searchText = string.Empty;
    private TextMeshPro? noResultsText;

    private const int ItemsPerPage = 15;

    private float xOffset = 1.95f;
    private float xStart = -0.8f;
    private float yOffset = -0.65f;
    private float yStart = 2.15f;

    public void OnDisable()
    {
        ControllerManager.Instance.CloseOverlayMenu(name);
    }

    public static GuesserMenu Create()
    {
        var shapeShifterRole = RoleManager.Instance.GetRole(RoleTypes.Shapeshifter);

        var ogMenu = shapeShifterRole.TryCast<ShapeshifterRole>()!.ShapeshifterMenu;
        var newMenu = Instantiate(ogMenu);
        var customMenu = newMenu.gameObject.AddComponent<GuesserMenu>();

        customMenu.panelPrefab = newMenu.PanelPrefab;
        customMenu.xStart = newMenu.XStart;
        customMenu.yStart = newMenu.YStart;
        customMenu.xOffset = newMenu.XOffset;
        customMenu.yOffset = newMenu.YOffset;
        customMenu.defaultButtonSelected = newMenu.DefaultButtonSelected;
        customMenu.backButton = newMenu.BackButton;

        var back = customMenu.backButton.GetComponent<PassiveButton>();
        back.OnClick.RemoveAllListeners();
        back.OnClick.AddListener((UnityAction)(Action)customMenu.Close);

        customMenu.CloseSound = newMenu.CloseSound;
        customMenu.logger = newMenu.logger;
        customMenu.OpenSound = newMenu.OpenSound;

        newMenu.DestroyImmediate();

        customMenu.transform.SetParent(Camera.main.transform, false);
        customMenu.transform.localPosition = new Vector3(0f, 0f, -60f);

        var nextButton = Instantiate(customMenu.backButton, customMenu.transform).gameObject;
        nextButton.transform.localPosition = new Vector3(1.85f, -2.185f, -60f);
        nextButton.transform.localScale = new Vector3(0.65f, 0.65f, 1);
        nextButton.name = "RightArrowButton";
        nextButton.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButton.LoadAsset();
        nextButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();

        var passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            customMenu.NextPage();
        }));

        var backButton = Instantiate(nextButton, customMenu.transform).gameObject;
        backButton.transform.localPosition = new Vector3(-1.85f, -2.185f, -60f);
        backButton.name = "LeftArrowButton";
        backButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        backButton.GetComponent<SpriteRenderer>().flipX = true;
        var prevPassive = backButton.gameObject.GetComponent<PassiveButton>();
        prevPassive.OnClick.AddListener((UnityAction)(() =>
        {
            customMenu.PreviousPage();
        }));
        customMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        customMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        return customMenu;
    }

    private sealed class MenuEntry(ShapeshifterPanel panel, string sortKey)
    {
        public ShapeshifterPanel Panel { get; } = panel;
        public string SortKey { get; } = sortKey;
    }

    private static string NormalizeForSearch(string? text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : text.Trim().ToLowerInvariant();
    }

    [HideFromIl2Cpp]
    private List<MenuEntry> GetFilteredEntries()
    {
        var query = NormalizeForSearch(searchText);
        if (string.IsNullOrEmpty(query))
        {
            return allEntries;
        }

        return allEntries
            .Where(e => e.SortKey.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.SortKey.Equals(query, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(e => e.SortKey.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(e => e.SortKey.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ThenBy(e => e.SortKey, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int GetTotalPages(int itemCount)
    {
        return Mathf.Max(1, Mathf.CeilToInt(itemCount / (float)ItemsPerPage));
    }

    private void RefreshControllerOverlay(Il2CppSystem.Collections.Generic.List<UiElement> list)
    {
        if (ControllerManager.Instance != null && backButton != null)
        {
            ControllerManager.Instance.OpenOverlayMenu(name, backButton, defaultButtonSelected, list);
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator CoRestoreFocus()
    {
        yield return null;
        if (searchTextbox != null)
        {
            searchTextbox.GiveFocus();
        }
    }

    private void NextPage()
    {
        var filtered = GetFilteredEntries();
        var pages = GetTotalPages(filtered.Count);
        currentPage = (currentPage + 1) % pages;
        var list = ShowPage();
        RefreshControllerOverlay(list);
    }

    private void PreviousPage()
    {
        var filtered = GetFilteredEntries();
        var pages = GetTotalPages(filtered.Count);
        currentPage = (currentPage - 1 + pages) % pages;
        var list = ShowPage();
        RefreshControllerOverlay(list);
    }

    public Il2CppSystem.Collections.Generic.List<UiElement> ShowPage()
    {
        foreach (var entry in allEntries)
        {
            entry.Panel.gameObject.SetActive(false);
        }

        var filtered = GetFilteredEntries();
        if (noResultsText != null)
        {
            noResultsText.gameObject.SetActive(filtered.Count == 0 && !string.IsNullOrWhiteSpace(searchText));
        }
        var totalPages = GetTotalPages(filtered.Count);
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        var list = filtered.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage).ToList();
        var list2 = new Il2CppSystem.Collections.Generic.List<UiElement>();

        for (var i = 0; i < list.Count; i++)
        {
            var entry = list[i];
            var num = i % 3;
            var num2 = i / 3 % 5;
            entry.Panel.transform.localPosition =
                new Vector3(xStart + num * xOffset, yStart + num2 * yOffset, -1f);
            entry.Panel.gameObject.SetActive(true);
            list2.Add(entry.Panel.Button);
        }

        return list2;
    }

    [HideFromIl2Cpp]
    private void EnsureSearchUi()
    {
        if (searchTextbox != null)
        {
            return;
        }

        var gridCenterX = xStart + xOffset;
        var desiredSearchBarCenterY = yStart + 0.55f;

        var wikiPrefab = TouAssets.WikiPrefab.LoadAsset();
        var prefabTextBox = wikiPrefab.GetComponentInChildren<TextBoxTMP>(true);
        if (prefabTextBox == null)
        {
            return;
        }

        var searchRoot = prefabTextBox.transform.parent != null ? prefabTextBox.transform.parent.gameObject : prefabTextBox.gameObject;
        var searchObj = Instantiate(searchRoot, transform);
        searchObj.name = "GuesserSearchBar";

        foreach (var aspect in searchObj.GetComponentsInChildren<AspectPosition>(true))
        {
            aspect.DestroyImmediate();
        }

        searchTextbox = searchObj.GetComponentInChildren<TextBoxTMP>(true);
        if (searchTextbox == null)
        {
            return;
        }

        try
        {
            var placeholder = searchTextbox.transform.parent.GetChild(2).GetComponent<TextMeshPro>();
            if (placeholder != null)
            {
                placeholder.gameObject.SetActive(false);
            }
        }
        catch
        {
            foreach (var tmp in searchObj.GetComponentsInChildren<TextMeshPro>(true))
            {
                if (tmp != searchTextbox.outputText && 
                    (tmp.text.Contains("Search", StringComparison.OrdinalIgnoreCase) || 
                     tmp.text.Contains("Here", StringComparison.OrdinalIgnoreCase)))
                {
                    tmp.gameObject.SetActive(false);
                }
            }
        }

        searchObj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        searchObj.transform.localPosition = new Vector3(gridCenterX, desiredSearchBarCenterY, -1f);

        var searchBounds = CalcSpriteBoundsInParentSpace(transform, searchObj);
        var deltaX = gridCenterX - searchBounds.center.x;
        var deltaY = desiredSearchBarCenterY - searchBounds.center.y;
        searchObj.transform.localPosition += new Vector3(deltaX, deltaY, 0f);
        searchBounds = CalcSpriteBoundsInParentSpace(transform, searchObj);

        var wikiClickSound = HudManager.Instance?.MapButton?.ClickSound;
        
        var searchFocusButton = searchTextbox.gameObject.GetComponent<PassiveButton>();
        if (searchFocusButton == null)
        {
            searchFocusButton = searchTextbox.gameObject.AddComponent<PassiveButton>();
        }
        if (wikiClickSound != null)
        {
            searchFocusButton.ClickSound = wikiClickSound;
        }
        searchFocusButton.OnClick.RemoveAllListeners();
        searchFocusButton.OnClick.AddListener((UnityAction)(Action)(() => 
        { 
            searchTextbox.GiveFocus();
        }));

        searchTextbox.SetText(string.Empty);
        searchTextbox.OnChange.RemoveAllListeners();
        searchTextbox.OnChange.AddListener((UnityAction)(Action)(() =>
        {
            searchText = searchTextbox.outputText.text ?? string.Empty;
            currentPage = 0;
            var list = ShowPage();
            RefreshControllerOverlay(list);
            
            if (searchTextbox != null)
            {
                Coroutines.Start(CoRestoreFocus());
            }
        }));

        var label = Instantiate(HudManager.Instance?.TaskPanel.taskText, transform);
        if (label != null)
        {
            label.name = "GuesserSearchLabel";
            label.text = TouLocale.Get("Search", "Search");
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = label.fontSizeMin = label.fontSizeMax = 2.1f;
            label.color = Color.white;
            label.transform.localPosition = new Vector3(gridCenterX, searchBounds.max.y + 0.18f, -1f);

            if (searchTextbox.outputText != null)
            {
                label.font = searchTextbox.outputText.font;
                label.fontMaterial = searchTextbox.outputText.fontMaterial;
            }
        }

        noResultsText = Instantiate(HudManager.Instance?.TaskPanel.taskText, transform);
        if (noResultsText != null)
        {
            noResultsText.name = "GuesserNoResultsText";
            noResultsText.text = "No results";
            noResultsText.alignment = TextAlignmentOptions.Center;
            noResultsText.fontSize = noResultsText.fontSizeMin = noResultsText.fontSizeMax = 2.25f;
            noResultsText.color = Color.white;
            noResultsText.transform.localPosition = new Vector3(gridCenterX, yStart + 0.1f, -1f);
            noResultsText.gameObject.SetActive(false);
        }

        if (backButton != null && searchTextbox != null)
        {
            var clearButtonX = searchBounds.max.x + 0.2f;
            var clearButtonY = searchBounds.center.y;
            
            var clearObj = Instantiate(backButton.gameObject, transform);
            clearObj.name = "ClearSearchButton";
            clearObj.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            clearObj.transform.localPosition = new Vector3(clearButtonX, clearButtonY, -1f);

            clearObj.GetComponent<CloseButtonConsoleBehaviour>()?.DestroyImmediate();
            clearObj.GetComponent<AspectPosition>()?.DestroyImmediate();

            var clearSearchButton = clearObj.GetComponent<PassiveButton>();
            if (clearSearchButton != null)
            {
                if (wikiClickSound != null)
                {
                    clearSearchButton.ClickSound = wikiClickSound;
                }
                
                clearSearchButton.OnClick.RemoveAllListeners();
                clearSearchButton.OnClick = new Button.ButtonClickedEvent();
                clearSearchButton.OnClick.AddListener((UnityAction)(() =>
                {
                    if (searchTextbox == null)
                    {
                        return;
                    }

                    searchTextbox.SetText(string.Empty);
                    searchText = string.Empty;
                    currentPage = 0;
                    var list = ShowPage();
                    RefreshControllerOverlay(list);
                }));
            }
        }
    }

    [HideFromIl2Cpp]
    private static Bounds CalcSpriteBoundsInParentSpace(Transform parent, GameObject root)
    {
        var first = true;
        var bounds = new Bounds();

        foreach (var r in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (r == null || r.sprite == null) continue;

            var b = r.bounds;
            var c = b.center;
            var e = b.extents;

            var p1 = parent.InverseTransformPoint(new Vector3(c.x - e.x, c.y - e.y, c.z));
            var p2 = parent.InverseTransformPoint(new Vector3(c.x - e.x, c.y + e.y, c.z));
            var p3 = parent.InverseTransformPoint(new Vector3(c.x + e.x, c.y - e.y, c.z));
            var p4 = parent.InverseTransformPoint(new Vector3(c.x + e.x, c.y + e.y, c.z));

            if (first)
            {
                bounds = new Bounds(p1, Vector3.zero);
                first = false;
            }

            bounds.Encapsulate(p1);
            bounds.Encapsulate(p2);
            bounds.Encapsulate(p3);
            bounds.Encapsulate(p4);
        }

        return bounds;
    }

    [HideFromIl2Cpp]
    public void Begin(Func<RoleBehaviour, bool> roleMatch, Action<RoleBehaviour> roleClickHandler,
        Func<BaseModifier, bool>? modifierMatch = null, Action<BaseModifier>? modifierClickHandler = null)
    {
        MinigameStubs.Begin(this, null);

        onRoleClick = roleClickHandler;
        onModifierClick = modifierClickHandler;
        allEntries = [];
        searchText = string.Empty;
        currentPage = 0;

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
            LocalSettingsTabSingleton<TownOfUsLocalSettings>.Instance.SortGuessingByAlignmentToggle.Value
                ? MiscUtils.GetParsedRoleAlignment(x) + x.GetRoleName()
                : x.GetRoleName()).ToList();

        for (var i = 0; i < newRoleList.Count; i++)
        {
            var role = newRoleList[i];

            var shapeshifterPanel = Instantiate(panelPrefab, transform);
            shapeshifterPanel!.transform.localPosition = new Vector3(0f, 0f, -1f);
            shapeshifterPanel.SetRole(i, role, () => { onRoleClick(role); });
            shapeshifterPanel.gameObject.transform.FindChild("Nameplate").FindChild("Highlight")
                .FindChild("ShapeshifterIcon").gameObject.SetActive(false);

            allEntries.Add(new MenuEntry(shapeshifterPanel, role.GetRoleName()));
        }

        if (modifierMatch != null && onModifierClick != null)
        {
            var modifiers = MiscUtils.AllModifiers.Where(modifierMatch).OrderBy(x => x.ModifierName).ToList();

            for (var i = 0; i < modifiers.Count; i++)
            {
                var index = newRoleList.Count + i;
                var modifier = modifiers[i];

                var shapeshifterPanel = Instantiate(panelPrefab, transform);
                shapeshifterPanel!.transform.localPosition = new Vector3(0f, 0f, -1f);
                shapeshifterPanel.SetModifier(index, modifier, () => { onModifierClick(modifier); });
                shapeshifterPanel.gameObject.transform.FindChild("Nameplate").FindChild("Highlight")
                    .FindChild("ShapeshifterIcon").gameObject.SetActive(false);

                allEntries.Add(new MenuEntry(shapeshifterPanel, modifier.ModifierName));
            }
        }

        EnsureSearchUi();

        var list2 = ShowPage();

        ControllerManager.Instance.OpenOverlayMenu(name, backButton, defaultButtonSelected, list2);

        if (MeetingHud.Instance != null)
        {
            MeetingHud.Instance.playerStates.Do(x => x.gameObject.SetActive(false));
        }
    }

    public override void Close()
    {
        MinigameStubs.Close(this);

        if (MeetingHud.Instance != null)
        {
            MeetingHud.Instance.playerStates.Do(x => x.gameObject.SetActive(true));
        }
    }
}
