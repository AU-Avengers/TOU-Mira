using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Modules.Wiki;

[RegisterInIl2Cpp]
public sealed class IngameWikiMinigame(nint cppPtr) : Minigame(cppPtr)
{
    public GameObject SearchIcon;
    private List<Transform> _activeItems = [];

    private WikiPage _currentPage = WikiPage.Homepage;
    private bool _modifiersSelected;
    private IWikiDiscoverable? _selectedItem;
    private SoftWikiInfo? _selectedSoftItem;
    public Il2CppReferenceField<Scroller> AbilityScroller;
    public Il2CppReferenceField<Transform> AbilityTemplate;
    public Il2CppReferenceField<PassiveButton> CloseButton;
    public Il2CppReferenceField<TextMeshPro> DetailDescription;

    public Il2CppReferenceField<Transform> DetailScreen;
    public Il2CppReferenceField<PassiveButton> DetailScreenBackBtn;
    public Il2CppReferenceField<SpriteRenderer> DetailScreenIcon;
    public Il2CppReferenceField<TextMeshPro> DetailScreenItemName;
    public Il2CppReferenceField<Transform> Homepage;
    public Il2CppReferenceField<PassiveButton> HomepageModifiersBtn;
    public Il2CppReferenceField<PassiveButton> HomepageRolesBtn;
    public Il2CppReferenceField<PassiveButton> OutsideCloseButton;
    public Il2CppReferenceField<Transform> SearchItemTemplate;
    public Il2CppReferenceField<SpriteRenderer> SearchPageIcon;
    public Il2CppReferenceField<TextMeshPro> SearchPageText;

    public Il2CppReferenceField<Transform> SearchScreen;
    public Il2CppReferenceField<PassiveButton> SearchScreenBackBtn;
    public Il2CppReferenceField<Scroller> SearchScroller;
    public Il2CppReferenceField<TextBoxTMP> SearchTextbox;
    public Il2CppReferenceField<PassiveButton> ToggleAbilitiesBtn;

    private void Awake()
    {
        if (MeetingHud.Instance)
        {
            MeetingHud.Instance.playerStates.Do(x => x.gameObject.SetActive(false));
        }

        if (GameStartManager.InstanceExists && LobbyBehaviour.Instance)
        {
            GameStartManager.Instance.HostInfoPanel.gameObject.SetActive(false);
        }

        UpdatePage(WikiPage.Homepage);

        var closeAction = new Action(() => { Close(); });

        CloseButton.Value.OnClick.AddListener((UnityAction)closeAction);
        OutsideCloseButton.Value.OnClick.AddListener((UnityAction)closeAction);

        HomepageModifiersBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            _modifiersSelected = true;
            UpdatePage(WikiPage.SearchScreen);
        }));

        HomepageRolesBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            _modifiersSelected = false;
            UpdatePage(WikiPage.SearchScreen);
        }));

        SearchScreenBackBtn.Value.OnClick.AddListener((UnityAction)(() => { UpdatePage(WikiPage.Homepage); }));

        DetailScreenBackBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            _selectedItem = null;
            _selectedSoftItem = null;
            UpdatePage(WikiPage.SearchScreen);
        }));

        SearchTextbox.Value.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            SearchTextbox.Value.GiveFocus();
        }));

        SearchTextbox.Value.OnChange.AddListener((UnityAction)(() =>
        {
            if (_currentPage != WikiPage.SearchScreen || _activeItems.Count == 0)
            {
                return;
            }

            var text = SearchTextbox.Value.outputText.text;
            _activeItems = _activeItems
                .OrderByDescending(child => child.name.Equals(text, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(child => child.name.Contains(text, StringComparison.InvariantCultureIgnoreCase))
                .ThenBy(child => child.name.ToLowerInvariant())
                .ToList();

            for (var i = 0; i < _activeItems.Count; i++)
            {
                _activeItems[i].SetSiblingIndex(i);
            }

            SearchScroller.Value.ScrollToTop();
        }));

        ToggleAbilitiesBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            if (DetailDescription.Value.gameObject.activeSelf)
            {
                ToggleAbilitiesBtn.Value.buttonText.text = "Description";
                DetailDescription.Value.gameObject.SetActive(false);
                AbilityScroller.Value.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                ToggleAbilitiesBtn.Value.buttonText.text =
                    _selectedItem != null ? _selectedItem.SecondTabName : "Abilities";
                DetailDescription.Value.gameObject.SetActive(true);
                AbilityScroller.Value.transform.parent.gameObject.SetActive(false);
            }
        }));

        foreach (var text in GetComponentsInChildren<TextMeshPro>(true))
        {
            if (text.color == Color.black)
            {
                continue;
            }

            text.font = HudManager.Instance.TaskPanel.taskText.font;
            text.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
        }

        foreach (var btn in GetComponentsInChildren<PassiveButton>(true))
        {
            btn.ClickSound = HudManager.Instance.MapButton.ClickSound;
        }
    }

    private void UpdatePage(WikiPage newPage)
    {
        TownOfUsColors.UseBasic = false;
        _currentPage = newPage;
        Homepage.Value.gameObject.SetActive(false);
        SearchScreen.Value.gameObject.SetActive(false);
        DetailScreen.Value.gameObject.SetActive(false);
        if (SearchIcon)
        {
            SearchIcon.SetActive(false);
        }

        if (MeetingHud.Instance)
        {
            MeetingHud.Instance.playerStates.Do(x => x.gameObject.SetActive(false));
        }

        switch (newPage)
        {
            default:
                Homepage.Value.gameObject.SetActive(true);
                
                var activeMods = PlayerControl.LocalPlayer.GetModifiers<GameModifier>()
                    .Where(x => x is IWikiDiscoverable).ToList();
                
                if (activeMods.Count > 0 && HomepageModifiersBtn.Value.transform.GetChild(0).TryGetComponent<SpriteRenderer>(out var modIcon))
                {
                    modIcon.sprite = activeMods.Random()!.ModifierIcon?.LoadAsset() ?? TouModifierIcons.Bait.LoadAsset();
                }
                
                var customAliveRole = PlayerControl.LocalPlayer.GetRoleWhenAlive() as ICustomRole;
                if (customAliveRole != null && HomepageRolesBtn.Value.transform.GetChild(0).TryGetComponent<SpriteRenderer>(out var roleIcon))
                {
                    roleIcon.sprite = customAliveRole.Configuration.Icon?.LoadAsset() ?? TouRoleIcons.Warlock.LoadAsset();
                }
                break;

            case WikiPage.SearchScreen:
                LoadSearchScreen();
                break;

            case WikiPage.DetailScreen:
                LoadDetailScreen();
                break;
        }

        TownOfUsColors.UseBasic = TownOfUsPlugin.UseCrewmateTeamColor.Value;
    }

    private void LoadDetailScreen()
    {
        if (_selectedItem == null && _selectedSoftItem == null)
        {
            UpdatePage(WikiPage.Homepage);
            return;
        }

        DetailScreen.Value.gameObject.SetActive(true);

        ToggleAbilitiesBtn.Value.gameObject.SetActive((_selectedItem != null) ? _selectedItem.Abilities.Count != 0 : _selectedSoftItem!.Abilities.Count != 0);
        DetailDescription.Value.gameObject.SetActive(true);
        AbilityScroller.Value.transform.parent.gameObject.SetActive(false);
        ToggleAbilitiesBtn.Value.buttonText.text = (_selectedItem != null) ? _selectedItem.SecondTabName : _selectedSoftItem!.SecondTabName;

        DetailDescription.Value.text = (_selectedItem != null) ? _selectedItem.GetAdvancedDescription() : _selectedSoftItem!.GetAdvancedDescription;
        DetailDescription.Value.fontSizeMax = 2.4f;

        if (_selectedItem is ITownOfUsRole touRole)
        {
            DetailScreenItemName.Value.text =
                $"{touRole.RoleName}\n<size=60%>{touRole.RoleColor.ToTextColor()}{touRole.RoleAlignment.ToDisplayString()}</size></color>";
            DetailScreenIcon.Value.sprite = touRole.Configuration.Icon != null
                ? touRole.Configuration.Icon.LoadAsset()
                : TouRoleIcons.RandomAny.LoadAsset();
        }
        else if (_selectedItem is BaseModifier baseModifier)
        {
            DetailScreenItemName.Value.text = baseModifier.ModifierName;
            DetailScreenIcon.Value.sprite = baseModifier.ModifierIcon != null
                ? baseModifier.ModifierIcon.LoadAsset()
                : TouRoleIcons.RandomAny.LoadAsset();
        }
        else if (_selectedSoftItem != null)
        {
            DetailScreenItemName.Value.text =
                $"{_selectedSoftItem.EntryName}\n<size=60%>{_selectedSoftItem.EntryColor.ToTextColor()}{_selectedSoftItem.TeamName}</size></color>";
            DetailScreenIcon.Value.sprite = _selectedSoftItem.Icon != null
                ? _selectedSoftItem.Icon
                : TouRoleIcons.RandomAny.LoadAsset();
        }

        DetailScreenIcon.Value.SetSizeLimit(0.75f);

        AbilityScroller.Value.Inner.DestroyChildren();

        var max = 0f;
        if (_selectedItem != null)
        {
            foreach (var ability in _selectedItem.Abilities)
            {
                var newAbility = Instantiate(AbilityTemplate.Value, AbilityScroller.Value.Inner.transform);
                var icon = newAbility.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
                var text = newAbility.GetChild(1).GetComponent<TextMeshPro>();
                var desc = newAbility.GetChild(2).GetComponent<TextMeshPro>();

                icon.sprite = ability.icon.LoadAsset();
                icon.size = new Vector2(0.8f, 0.8f * icon.sprite.bounds.size.y / icon.sprite.bounds.size.x);
                icon.tileMode = SpriteTileMode.Adaptive;

                text.text =
                    $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{ability.name}</font>";
                desc.text =
                    $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{ability.description}</font>";
                newAbility.gameObject.SetActive(true);
            }
            max = Mathf.Max(0f, _selectedItem.Abilities.Count * 0.875f);
        }
        else if (_selectedSoftItem != null)
        {
            foreach (var ability in _selectedSoftItem.Abilities)
            {
                var newAbility = Instantiate(AbilityTemplate.Value, AbilityScroller.Value.Inner.transform);
                var icon = newAbility.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
                var text = newAbility.GetChild(1).GetComponent<TextMeshPro>();
                var desc = newAbility.GetChild(2).GetComponent<TextMeshPro>();

                icon.sprite = ability.icon.LoadAsset();
                icon.size = new Vector2(0.8f, 0.8f * icon.sprite.bounds.size.y / icon.sprite.bounds.size.x);
                icon.tileMode = SpriteTileMode.Adaptive;

                text.text =
                    $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{ability.name}</font>";
                desc.text =
                    $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{ability.description}</font>";
                newAbility.gameObject.SetActive(true);
            }
            max = Mathf.Max(0f, _selectedSoftItem.Abilities.Count * 0.875f);
        }
        AbilityScroller.Value.SetBounds(new FloatRange(-0.5f, max), null);
        AbilityScroller.Value.ScrollToTop();
    }

    private void LoadSearchScreen()
    {
        SearchScreen.Value.gameObject.SetActive(true);
        SearchPageText.Value.text = _modifiersSelected ? "Modifiers" : "Roles";
        SearchPageIcon.Value.sprite = _modifiersSelected
            ? TouModifierIcons.Bait.LoadAsset()
            : TouRoleIcons.Warlock.LoadAsset();
        if (!SearchIcon)
        {
            SearchIcon = Instantiate(SearchPageIcon.Value.gameObject, Instance.gameObject.transform);
            SearchIcon.transform.localPosition += new Vector3(0.625f, 0.796f, -1.1f);
            SearchIcon.transform.localScale *= 0.25f;
            var renderer = SearchIcon.GetComponent<SpriteRenderer>();
            renderer.sprite = TouRoleIcons.Detective.LoadAsset();
            SearchIcon.name = "SearchboxIcon";
        }

        SearchIcon.SetActive(true);

        var oldMax = Mathf.Max(0f, SearchScroller.Value.Inner.GetChildCount() * 0.725f);

        _activeItems.Do(x => x.gameObject.DestroyImmediate());
        _activeItems.Clear();

        SearchTextbox.Value.SetText(string.Empty);

        if (_modifiersSelected)
        {
            var activeModifiers = PlayerControl.LocalPlayer.GetModifiers<GameModifier>()
                .Where(x => x is IWikiDiscoverable)
                .Select(x => MiscUtils.GetModifierTypeId(x));
            var comparer = new ModifierComparer(activeModifiers);
            
            var activeMods = PlayerControl.LocalPlayer.GetModifiers<GameModifier>()
                .Where(x => x is IWikiDiscoverable).ToList();

            if (activeMods.Count > 0)
            {
                SearchPageIcon.Value.sprite = activeMods.Random()!.ModifierIcon?.LoadAsset() ?? TouModifierIcons.Bait.LoadAsset();
            }

            var modifiers = MiscUtils.AllModifiers
                .Where(x => x is IWikiDiscoverable wikiMod && !wikiMod.IsHiddenFromList)
                .OrderBy(x => x, comparer)
                .ToList();

            foreach (var modifier in modifiers)
            {
                var color = MiscUtils.GetRoleColour(modifier.ModifierName.Replace(" ", string.Empty));
                if (modifier is IColoredModifier colorMod)
                {
                    color = colorMod.ModifierColor;
                }
                var newItem = CreateNewItem(modifier.ModifierName, modifier.ModifierIcon?.LoadAsset(), color);
                newItem.transform.GetChild(2).gameObject.SetActive(false);
                var alignment = "External";
                if (modifier is UniversalGameModifier uniMod)
                {
                    alignment = uniMod.FactionType.ToDisplayString();
                }

                if (modifier is TouGameModifier touMod)
                {
                    alignment = touMod.FactionType.ToDisplayString();
                }

                if (modifier is AllianceGameModifier allyMod)
                {
                    alignment = allyMod.FactionType.ToDisplayString();
                }

                if (alignment.Contains("Non "))
                {
                    alignment = alignment.Replace("Non ", "Non-");
                }

                var amount = modifier is GameModifier gameMod ? gameMod.GetAmountPerGame() : 0;
                var chance = modifier is GameModifier gameMod2 ? gameMod2.GetAssignmentChance() : 0;
                var amountTxt = newItem.transform.FindChild("AmountTxt").gameObject.GetComponent<TextMeshPro>();
                if (modifier is UniversalGameModifier uniMod2)
                {
                    amount = uniMod2.CustomAmount;
                    chance = uniMod2.CustomChance;
                }
                else if (modifier is TouGameModifier touMod2)
                {
                    amount = touMod2.CustomAmount;
                    chance = touMod2.CustomChance;
                }
                else if (modifier is AllianceGameModifier allyMod2)
                {
                    amount = allyMod2.CustomAmount;
                    chance = allyMod2.CustomChance;
                }

                var txt = amount != 0 ? $"Amount: {amount} - Chance: {chance}%" : "Amount: 0";

                amountTxt.text =
                    $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{txt}</font>";
                amountTxt.fontSizeMin = 1.85f;
                amountTxt.fontSizeMax = 1.85f;
                amountTxt.fontSize = 1.85f;
                newItem.GetChild(1).GetComponent<TextMeshPro>().transform.localPosition += new Vector3(0f, 0.12f);

                var team = newItem.transform.GetChild(2).gameObject.GetComponent<TextMeshPro>();
                team.fontSizeMax = 2.65f;
                team.text =
                    $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Masked\">{alignment}</font>";
                team.gameObject.SetActive(true);
                team.SetOutlineColor(Color.black);
                team.SetOutlineThickness(0.35f);

                if (modifier is IWikiDiscoverable wikiDiscoverable) SetupForItem(newItem.gameObject.GetComponent<PassiveButton>(), wikiDiscoverable);
                else SetupForItem(newItem.gameObject.GetComponent<PassiveButton>(), SoftWikiEntries.ModifierEntries.GetValueOrDefault(modifier));
            }
        }
        else
        {
            List<ushort> roleList = [(ushort)PlayerControl.LocalPlayer.Data.Role.Role];
            if (PlayerControl.LocalPlayer.Data.IsDead &&
                !roleList.Contains((ushort)PlayerControl.LocalPlayer.GetRoleWhenAlive().Role))
            {
                roleList.Add((ushort)PlayerControl.LocalPlayer.GetRoleWhenAlive().Role);
            }

            var customAliveRole = PlayerControl.LocalPlayer.GetRoleWhenAlive() as ICustomRole;
            if (customAliveRole != null)
            {
                SearchPageIcon.Value.sprite = customAliveRole.Configuration.Icon?.LoadAsset() ?? TouRoleIcons.Warlock.LoadAsset();
            }

            var comparer = new RoleComparer(roleList);
            var roles = MiscUtils.AllRoles.OrderBy(x => x, comparer);

            foreach (var role in roles)
            {
                if (role is not IWikiDiscoverable && !SoftWikiEntries.RoleEntries.ContainsKey(role))
                {
                    continue;
                }
                var customRole = role as ICustomRole;

                if (customRole == null)
                {
                    continue;
                }
                // Hides hidden roles from other mods, but keeps them visible for Pest/Mayor
                if (customRole.Configuration.HideSettings && role is not IWikiDiscoverable)
                {
                    continue;
                }
                
                var teamName = role.GetRoleAlignment().ToDisplayString();

                var newItem = CreateNewItem(customRole.RoleName, customRole.Configuration.Icon?.LoadAsset(), customRole.RoleColor);
                var team = newItem.transform.GetChild(2).gameObject.GetComponent<TextMeshPro>();
                team.fontSizeMax = 2.65f;
                team.text =
                    $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Masked\">{teamName}</font>";
                team.gameObject.SetActive(true);
                team.SetOutlineColor(Color.black);
                team.SetOutlineThickness(0.35f);

                if (customRole.Configuration.MaxRoleCount != 0 &&
                    !customRole.Configuration.HideSettings)
                {
                    var amount = customRole.GetCount();
                    var chance = customRole.GetChance();
                    var amountTxt = newItem.transform.FindChild("AmountTxt").gameObject.GetComponent<TextMeshPro>();

                    var txt = amount != 0 ? $"Amount: {amount} - Chance: {chance}%" : "Amount: 0";
                    amountTxt.text =
                        $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{txt}</font>";
                    amountTxt.fontSizeMin = 1.85f;
                    amountTxt.fontSizeMax = 1.85f;
                    amountTxt.fontSize = 1.85f;
                    newItem.GetChild(1).GetComponent<TextMeshPro>().transform.localPosition += new Vector3(0f, 0.12f);
                }

                if (role is IWikiDiscoverable wikiDiscoverable) SetupForItem(newItem.gameObject.GetComponent<PassiveButton>(), wikiDiscoverable);
                else SetupForItem(newItem.gameObject.GetComponent<PassiveButton>(), SoftWikiEntries.RoleEntries.GetValueOrDefault(role));
            }
        }

        var max = Mathf.Max(0f, SearchScroller.Value.Inner.GetChildCount() * 0.725f);
        SearchScroller.Value.SetBounds(new FloatRange(-0.4f, max), null);
        if (oldMax != max)
        {
            SearchScroller.Value.ScrollToTop();
        }
    }

    [HideFromIl2Cpp]
    private void SetupForItem(PassiveButton passiveButton, IWikiDiscoverable? wikiDiscoverable)
    {
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            _selectedItem = wikiDiscoverable;
            _selectedSoftItem = null;
            UpdatePage(WikiPage.DetailScreen);
        }));
    }
    [HideFromIl2Cpp]
    private void SetupForItem(PassiveButton passiveButton, SoftWikiInfo? softInfo)
    {
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            _selectedSoftItem = softInfo;
            _selectedItem = null;
            UpdatePage(WikiPage.DetailScreen);
        }));
    }

    private Transform CreateNewItem(string itemName, Sprite? sprite, Color color)
    {
        var newItem = Instantiate(SearchItemTemplate.Value, SearchScroller.Value.Inner);
        var icon = newItem.GetChild(0).GetComponent<SpriteRenderer>();
        var itemText = newItem.GetChild(1).GetComponent<TextMeshPro>();
        var bgColor = newItem.GetChild(3).GetComponent<SpriteRenderer>();

        var bgSprite = bgColor.GetComponent<SpriteRenderer>();
        bgSprite.color = color;
        if (SearchScroller.Value.Inner.GetChildCount() > 1)
        {
            newItem.GetChild(3).localPosition += Vector3.up * 0.015f;
        }

        var amountTextObj =
            Instantiate(newItem.GetChild(1).gameObject, newItem.GetChild(1).gameObject.transform.parent);
        amountTextObj.name = "AmountTxt";
        amountTextObj.transform.localPosition -= new Vector3(0f, 0.22f);

        newItem.name = itemName.ToLowerInvariant();
        icon.sprite = sprite != null ? sprite : TouRoleIcons.RandomAny.LoadAsset();
        icon.SetSizeLimit(0.725f);
        amountTextObj.GetComponent<TextMeshPro>().text = string.Empty;
        itemText.text =
            $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{itemName}</font>";
        newItem.gameObject.SetActive(true);
        _activeItems.Add(newItem);
        return newItem;
    }

    public static IngameWikiMinigame Create()
    {
        var gameObject = Instantiate(TouAssets.WikiPrefab.LoadAsset(), HudManager.Instance.transform);
        gameObject.transform.SetParent(Camera.main!.transform, false);
        gameObject.transform.localPosition = new Vector3(0f, 0f, -50f);
        return gameObject.GetComponent<IngameWikiMinigame>();
    }

    public override void Close()
    {
        MinigameStubs.Close(this);

        if (GameStartManager.InstanceExists && LobbyBehaviour.Instance)
        {
            GameStartManager.Instance.HostInfoPanel.gameObject.SetActive(true);
        }

        if (MeetingHud.Instance)
        {
            MeetingHud.Instance.playerStates.Do(x => x.gameObject.SetActive(true));
        }

        TownOfUsColors.UseBasic = TownOfUsPlugin.UseCrewmateTeamColor.Value;
    }

    [HideFromIl2Cpp]
    public void OpenFor(IWikiDiscoverable? wikiDiscoverable)
    {
        _selectedItem = wikiDiscoverable;
        _selectedSoftItem = null;
        UpdatePage(WikiPage.DetailScreen);
    }
    
    [HideFromIl2Cpp]
    public void OpenFor(SoftWikiInfo? softWikiInfo)
    {
        _selectedItem = null;
        _selectedSoftItem = softWikiInfo;
        UpdatePage(WikiPage.DetailScreen);
    }
}

public enum WikiPage
{
    Homepage,
    SearchScreen,
    DetailScreen
}