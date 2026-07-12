using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using System.Text;
using TMPro;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Roles;
using UnityEngine;
using UnityEngine.Events;

namespace TownOfUs.Modules.Wiki;

public sealed class IngameWikiMinigame : Minigame
{
    public GameObject SearchIcon;
    private List<Transform> _activeItems = [];
    private readonly List<InGameModifierWikiEntry> _modifierEntries = [];
    private readonly List<InGameRoleWikiEntry> _roleEntries = [];
    private List<RoleBehaviour> _roleList = [];

    private WikiPage _currentPage = WikiPage.Homepage;
    private bool _modifiersSelected;
    private IWikiDiscoverable _selectedItem;
    private SoftWikiInfo _selectedSoftItem;
    private TermWikiInfo _selectedTermPage;
    public readonly List<TermWikiInfo> _activeTerms = [];
    private OptionWikiInfo _selectedSettingsPage;
    public readonly List<OptionWikiInfo> _activeSettings = [];
    public Scroller AbilityScroller;
    public Transform AbilityTemplate;
    public PassiveButton CloseButton;
    public TextMeshPro DetailDescription;

    public Transform DetailScreen;
    public PassiveButton DetailScreenBackBtn;
    public SpriteRenderer DetailScreenIcon;
    public TextMeshPro DetailScreenItemName;
    public Transform Homepage;
    public PassiveButton HomepageModifiersBtn;
    public PassiveButton HomepageRolesBtn;
    public PassiveButton HomepageTermsBtn;
    public PassiveButton HomepageSettingsBtn;
    public PassiveButton OutsideCloseButton;
    public InGameModifierWikiEntry ModifierSearchItemTemplate;
    public InGameRoleWikiEntry RoleSearchItemTemplate;
    public SpriteRenderer SearchPageIcon;
    public TextMeshPro SearchPageText;

    public Transform SearchScreen;
    public PassiveButton SearchScreenBackBtn;
    public Scroller SearchScroller;
    public TextBoxTMP SearchTextbox;
    public PassiveButton ToggleAbilitiesBtn;

    public Transform TermsScreen;
    public TextMeshPro TermsDescription;
    public PassiveButton TermsPreviousBtn;
    public PassiveButton TermsNextBtn;
    public PassiveButton TermsBackBtn;
    public SpriteRenderer TermsScreenIcon;
    public TextMeshPro TermsScreenSectionName;
    public TextMeshPro TermsScreenTabCount;

    public Transform SettingsScreen;
    public TextMeshPro SettingsDescription;
    public PassiveButton SettingsPreviousBtn;
    public PassiveButton SettingsNextBtn;
    public PassiveButton SettingsBackBtn;
    public SpriteRenderer SettingsScreenIcon;
    public TextMeshPro SettingsScreenSectionName;
    public TextMeshPro SettingsScreenTabCount;

    public static void AddNewTerms(IngameWikiMinigame instance)
    {
        instance._activeTerms.Add(new TermWikiInfo("TermsTargetSymbolsTitle", "TermsTargetSymbolsInfo", TouRoleIcons.Executioner));
        instance._activeTerms.Add(new TermWikiInfo("TermsProtectionSymbolsTitle", "TermsProtectionSymbolsInfo", TouRoleIcons.Fairy));
        instance._activeTerms.Add(new TermWikiInfo("TermsStatusEffectSymbolsTitle", "TermsStatusEffectInfo", TouRoleIcons.Monarch));
        instance._activeTerms.Add(new TermWikiInfo("TermsCrewRoleAlignmentsTitle", "TermsCrewRoleAlignmentsInfo", TouRoleIcons.Crewmate));
        instance._activeTerms.Add(new TermWikiInfo("TermsNeutRoleAlignmentsTitle", "TermsNeutRoleAlignmentsInfo", TouRoleIcons.Neutral));
        instance._activeTerms.Add(new TermWikiInfo("TermsImpRoleAlignmentsTitle", "TermsImpRoleAlignmentsInfo", TouRoleIcons.Impostor));
        instance._activeTerms.Add(new TermWikiInfo("TermsRoleBucketsTitle", "TermsRoleBucketsInfo", TouRoleIcons.Traitor));
        instance._activeTerms.Add(new TermWikiInfo("TermsCommonSlangTitle", "TermsCommonSlangInfo", TouAssets.TerminologySprite));
    }

    public static void AddNewSettings(IngameWikiMinigame instance)
    {
        instance._activeSettings.Add(new OptionWikiInfo("WikiSettingsAmongUsGameSettingsTitle", [], TouRoleIcons.Detective, true));
        instance._activeSettings.Add(new OptionWikiInfo("WikiSettingsTouMiraGameSettingsTitle",
            [
                OptionGroupSingleton<GeneralOptions>.Instance, OptionGroupSingleton<VanillaTweakOptions>.Instance,
                OptionGroupSingleton<GameMechanicOptions>.Instance, OptionGroupSingleton<PostmortemOptions>.Instance,
                OptionGroupSingleton<GameTimerOptions>.Instance, OptionGroupSingleton<TaskTrackingOptions>.Instance
            ], TouRoleIcons.Engineer));
        instance._activeSettings.Add(new OptionWikiInfo("WikiSettingsMapsSabotageSettingsTitle",
            [
                OptionGroupSingleton<GlobalBetterMapOptions>.Instance, OptionGroupSingleton<AdvancedUtilityOptions>.Instance,
                OptionGroupSingleton<AdvancedSabotageOptions>.Instance
            ], TouModifierIcons.Operative));
        instance._activeSettings.Add(new OptionWikiInfo("WikiSettingsBetterMapsSettingsTitle",
            [
                OptionGroupSingleton<BetterSkeldOptions>.Instance, OptionGroupSingleton<BetterMiraHqOptions>.Instance,
                OptionGroupSingleton<BetterPolusOptions>.Instance, OptionGroupSingleton<BetterAirshipOptions>.Instance,
                OptionGroupSingleton<BetterFungleOptions>.Instance, OptionGroupSingleton<BetterSubmergedOptions>.Instance,
                OptionGroupSingleton<BetterLevelImpostorOptions>.Instance
            ], TouRoleIcons.Spy));
    }
    private void Awake()
    {
        AddNewTerms(this);
        AddNewSettings(this);
        if (MeetingHud.Instance)
        {
            MeetingHud.Instance.playerStates.Do(x => x.gameObject.SetActive(false));
        }

        if (GameStartManager.InstanceExists && LobbyBehaviour.Instance)
        {
            GameStartManager.Instance.HostInfoPanel.gameObject.SetActive(false);
        }

        SearchPageIcon.SetSizeLimit(1.44f);
        DetailScreenIcon.SetSizeLimit(1.44f);
        if (HomepageModifiersBtn.transform.GetChild(0).TryGetComponent<SpriteRenderer>(out var modIcon))
        {
            modIcon.SetSizeLimit(1.44f);
        }

        if (HomepageRolesBtn.transform.GetChild(0).TryGetComponent<SpriteRenderer>(out var roleIcon))
        {
            roleIcon.SetSizeLimit(1.44f);
        }

        UpdatePage(WikiPage.Homepage);

        var closeAction = new UnityAction(() => { Close(); });

        CloseButton.OnClick.AddListener(closeAction);
        OutsideCloseButton.OnClick.AddListener(closeAction);
        HomepageModifiersBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("Modifiers", "Modifiers");
        HomepageModifiersBtn.OnClick.AddListener((UnityAction)(() =>
        {
            _modifiersSelected = true;
            UpdatePage(WikiPage.SearchScreen);
        }));

        HomepageRolesBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("Roles", "Roles");
        HomepageRolesBtn.OnClick.AddListener((UnityAction)(() =>
        {
            _modifiersSelected = false;
            UpdatePage(WikiPage.SearchScreen);
        }));

        HomepageTermsBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("Terminology", "Terminology");
        HomepageTermsBtn.OnClick.AddListener((UnityAction)(() =>
        {
            UpdatePage(WikiPage.TermsScreen);
        }));

        TermsBackBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        TermsBackBtn.OnClick.AddListener((UnityAction)(() => { UpdatePage(WikiPage.Homepage); }));

        TermsPreviousBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("PreviousButtonText", "Previous");
        TermsPreviousBtn.OnClick.AddListener((UnityAction)(() => { ShiftTermsPage(false); }));

        TermsNextBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("NextButtonText", "Next");
        TermsNextBtn.OnClick.AddListener((UnityAction)(() => { ShiftTermsPage(true); }));

        SearchScreenBackBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        SearchScreenBackBtn.OnClick.AddListener((UnityAction)(() => { UpdatePage(WikiPage.Homepage); }));

        DetailScreenBackBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        DetailScreenBackBtn.OnClick.AddListener((UnityAction)(() =>
        {
            _selectedItem = null!;
            _selectedSoftItem = null!;
            UpdatePage(WikiPage.SearchScreen);
        }));

        HomepageSettingsBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("GameSettings", "GameSettings");
        HomepageSettingsBtn.OnClick.AddListener((UnityAction)(() =>
        {
            UpdatePage(WikiPage.SettingsScreen);
        }));

        SettingsBackBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        SettingsBackBtn.OnClick.AddListener((UnityAction)(() => { UpdatePage(WikiPage.Homepage); }));

        SettingsPreviousBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("PreviousButtonText", "Previous");
        SettingsPreviousBtn.OnClick.AddListener((UnityAction)(() => { ShiftSettingsPage(false); }));

        SettingsNextBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("NextButtonText", "Next");
        SettingsNextBtn.OnClick.AddListener((UnityAction)(() => { ShiftSettingsPage(true); }));

        SearchScreenBackBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        SearchScreenBackBtn.OnClick.AddListener((UnityAction)(() => { UpdatePage(WikiPage.Homepage); }));

        DetailScreenBackBtn.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        DetailScreenBackBtn.OnClick.AddListener((UnityAction)(() =>
        {
            _selectedItem = null!;
            _selectedSoftItem = null!;
            UpdatePage(WikiPage.SearchScreen);
        }));

        SearchTextbox.transform.GetParent().GetChild(2).GetComponent<TextMeshPro>().text =
            TouLocale.Get("SearchboxHeadsUp", "Search Here");
        SearchTextbox.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((UnityAction)(() =>
        {
            SearchTextbox.GiveFocus();
        }));

        SearchTextbox.OnChange.AddListener((UnityAction)(() =>
        {
            if (_currentPage != WikiPage.SearchScreen || _activeItems.Count == 0)
            {
                return;
            }

            var text = SearchTextbox.outputText.text;
            _activeItems = _activeItems
                .OrderByDescending(child => child.name.Equals(text, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(child => child.name.Contains(text, StringComparison.InvariantCultureIgnoreCase))
                .ThenBy(child => child.name.ToLowerInvariant())
                .ToList();

            for (var i = 0; i < _activeItems.Count; i++)
            {
                _activeItems[i].SetSiblingIndex(i);
            }

            SearchScroller.ScrollToTop();
        }));

        ToggleAbilitiesBtn.OnClick.AddListener((UnityAction)(() =>
        {
            if (DetailDescription.gameObject.activeSelf)
            {
                ToggleAbilitiesBtn.buttonText.text = TouLocale.Get("WikiDescriptionTab", "Description");
                DetailDescription.gameObject.SetActive(false);
                AbilityScroller.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                ToggleAbilitiesBtn.buttonText.text =
                    _selectedItem != null
                        ? _selectedItem.SecondTabName
                        : TouLocale.Get("WikiAbilitiesTab", "Abilities");
                DetailDescription.gameObject.SetActive(true);
                AbilityScroller.transform.parent.gameObject.SetActive(false);
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
        Homepage.gameObject.SetActive(false);
        SearchScreen.gameObject.SetActive(false);
        DetailScreen.gameObject.SetActive(false);
        TermsScreen.gameObject.SetActive(false);
        SettingsScreen.gameObject.SetActive(false);
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
                Homepage.gameObject.SetActive(true);

                var activeMods = PlayerControl.LocalPlayer.GetModifiers<GameModifier>()
                    .Where(x => x is IWikiDiscoverable || SoftWikiEntries.ModifierEntries.ContainsKey(x)).ToList();
                SpriteRenderer? modifierIcon = null!;
                SpriteRenderer? playerRoleIcon = null!;

                if (activeMods.Count > 0 && HomepageModifiersBtn.transform.GetChild(0)
                        .TryGetComponent<SpriteRenderer>(out var modIcon))
                {
                    modifierIcon = modIcon;
                    modIcon.sprite = activeMods.Random()!.ModifierIcon?.LoadAsset() ??
                                     TouModifierIcons.Bait.LoadAsset();
                }

                var aliveRole = PlayerControl.LocalPlayer.GetRoleWhenAlive();
                if (aliveRole != null && HomepageRolesBtn.transform.GetChild(0)
                        .TryGetComponent<SpriteRenderer>(out var roleIcon))
                {
                    playerRoleIcon = roleIcon;
                    roleIcon.sprite = aliveRole.RoleIconSolid ?? TouRoleIcons.Parasite.LoadAsset();
                }

                modifierIcon?.SetSizeLimit(1.44f);

                playerRoleIcon?.SetSizeLimit(1.44f);

                break;

            case WikiPage.SearchScreen:
                LoadSearchScreen();
                break;

            case WikiPage.DetailScreen:
                LoadDetailScreen();
                break;

            case WikiPage.TermsScreen:
                LoadTermsScreen();
                break;

            case WikiPage.SettingsScreen:
                LoadSettingsScreen();
                break;
        }

        TownOfUsColors.UseBasic =
            LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance.UseCrewmateTeamColorToggle.Value;
    }

    private void LoadSettingsScreen()
    {
        SettingsScreen.gameObject.SetActive(true);
        if (_selectedSettingsPage == null)
        {
            SelectSettingsPage(_activeSettings[0], false);
        }
    }

    private void ShiftSettingsPage(bool goNext)
    {
        if (_selectedSettingsPage == null)
        {
            SelectSettingsPage(_activeSettings[0], false);
        }
        var index = _activeSettings.IndexOf(_selectedSettingsPage);
        if (goNext)
        {
            if (SettingsDescription.pageToDisplay < SettingsDescription.textInfo.pageCount)
            {
                ++SettingsDescription.pageToDisplay;
            }
            else if (_activeSettings.Count > (index + 1))
            {
                SelectSettingsPage(_activeSettings[index + 1], false);
            }
            else
            {
                SelectSettingsPage(_activeSettings[0], false);
            }
        }
        else
        {
            if (SettingsDescription.pageToDisplay > 1)
            {
                --SettingsDescription.pageToDisplay;
            }
            else if (index == 0)
            {
                SelectSettingsPage(_activeSettings[^1], true);
            }
            else
            {
                SelectSettingsPage(_activeSettings[index - 1], true);
            }
        }

        SettingsScreenTabCount.text = TouLocale.GetParsed("TermsPageCount")
            .Replace("<po>", $"{SettingsDescription.pageToDisplay}")
            .Replace("<pt>", $"{SettingsDescription.textInfo.pageCount}")
            .Replace("<so>", $"{_activeSettings.IndexOf(_selectedSettingsPage) + 1}")
            .Replace("<st>", $"{_activeSettings.Count}");
        // Error($"Page Count: {SettingsDescription.Value.textInfo.pageCount}, current page is {SettingsDescription.Value.pageToDisplay}");
    }

    private void SelectSettingsPage(OptionWikiInfo newTerms, bool lastPage)
    {
        _selectedSettingsPage = newTerms;
        var sBuilder = new StringBuilder();
        var isFirst = true;
        if (newTerms.IsVanilla)
        {
            foreach (var rulesCategory in GameManager.Instance.GameSettingsList.AllCategories)
            {
                sBuilder.AppendLine(GetCategoryHeader(rulesCategory.CategoryName, isFirst));
                isFirst = false;
                foreach (BaseGameSetting baseGameSetting in rulesCategory.AllGameSettings)
                {
                    sBuilder.AppendLine(GetVanillaOptionData(baseGameSetting));
                }
            }
        }
        else
        {
            var mainOptionGroups =
                AccessTools.Field(typeof(ModdedOptionsManager), "Groups").GetValue(null) as List<AbstractOptionGroup>;
            foreach (var optionsCategory in newTerms.OptionGroups)
            {
                var options = mainOptionGroups?.FirstOrDefault(x => x == optionsCategory)?.Children;
                IWikiOptionsSummaryProvider? summaryProvider = null!;
                HashSet<StringNames>? hiddenKeys = null!;
                try
                {
                    var optionGroups =
                        AccessTools.Field(typeof(ModdedOptionsManager), "Groups").GetValue(null) as
                            List<AbstractOptionGroup>;
                    summaryProvider =
                        optionGroups?.FirstOrDefault(x => x == optionsCategory) as IWikiOptionsSummaryProvider;
                    hiddenKeys = summaryProvider?.WikiHiddenOptionKeys;
                }
                catch
                {
                    summaryProvider = null!;
                    hiddenKeys = null!;
                }

                if (options == null || !optionsCategory.GroupVisible())
                {
                    continue;
                }

                sBuilder.AppendLine(GetCategoryHeader(optionsCategory.GroupName, isFirst));
                isFirst = false;

                var insertedSummary = false;
                foreach (var option in options)
                {
                    if (!insertedSummary && summaryProvider != null && hiddenKeys != null)
                    {
                        StringNames? key = option switch
                        {
                            ModdedToggleOption t => t.StringName,
                            ModdedEnumOption e => e.StringName,
                            ModdedNumberOption n => n.StringName,
                            _ => null
                        };
                        if (key.HasValue && hiddenKeys.Contains(key.Value))
                        {
                            foreach (var line in summaryProvider.GetWikiOptionSummaryLines())
                            {
                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    sBuilder.AppendLine(line);
                                }
                            }

                            insertedSummary = true;
                        }
                    }

                    switch (option)
                    {
                        case ModdedToggleOption toggleOption:
                            if (!toggleOption.Visible())
                            {
                                continue;
                            }

                            if (hiddenKeys != null && hiddenKeys.Contains(toggleOption.StringName))
                            {
                                continue;
                            }

                            sBuilder.AppendLine(TranslationController.Instance.GetString(toggleOption.StringName) +
                                                ": " +
                                                toggleOption.Value);
                            break;
                        /*case ModdedMultiSelectOption<Enum> enumOption:
                            if (!enumOption.Visible())
                            {
                                continue;
                            }

                            builder.AppendLine(enumOption.Title + ": " + enumOption.Values[enumOption.Value]);
                            break;*/
                        case ModdedEnumOption enumOption:
                            if (!enumOption.Visible())
                            {
                                continue;
                            }

                            if (hiddenKeys != null && hiddenKeys.Contains(enumOption.StringName))
                            {
                                continue;
                            }

                            sBuilder.AppendLine(TranslationController.Instance.GetString(enumOption.StringName) + ": " +
                                                TouLocale.GetParsed(enumOption.Values[enumOption.Value],
                                                    enumOption.Values[enumOption.Value]));
                            break;
                        case ModdedNumberOption numberOption:
                            if (!numberOption.Visible())
                            {
                                continue;
                            }

                            if (hiddenKeys != null && hiddenKeys.Contains(numberOption.StringName))
                            {
                                continue;
                            }

                            var optionStr = numberOption.Data.GetValueString(numberOption.Value);
                            if (optionStr.Contains(".000"))
                            {
                                optionStr = optionStr.Replace(".000", "");
                            }
                            else if (optionStr.Contains(".00"))
                            {
                                optionStr = optionStr.Replace(".00", "");
                            }
                            else if (optionStr.Contains(".0"))
                            {
                                optionStr = optionStr.Replace(".0", "");
                            }

                            var title = TranslationController.Instance.GetString(numberOption.StringName);
                            if (numberOption is { NegativeWordValue: not "#", Value: -1 })
                            {
                                sBuilder.AppendLine(title + $": {numberOption.NegativeWordValue}");
                            }
                            else if (numberOption is { ZeroWordValue: not "#", Value: 0 })
                            {
                                sBuilder.AppendLine(title + $": {numberOption.ZeroWordValue}");
                            }
                            else
                            {
                                sBuilder.AppendLine(title + ": " + optionStr);
                            }

                            break;
                    }
                }
            }
        }

        SettingsDescription.text = sBuilder.ToString();
        SettingsDescription.ForceMeshUpdate();
        SettingsScreenSectionName.text = TouLocale.GetParsed(newTerms.Title);

        SettingsDescription.pageToDisplay = lastPage ? SettingsDescription.textInfo.pageCount : 1;
        SettingsScreenTabCount.text = TouLocale.GetParsed("TermsPageCount")
            .Replace("<po>", $"{SettingsDescription.pageToDisplay}")
            .Replace("<pt>", $"{SettingsDescription.textInfo.pageCount}")
            .Replace("<so>", $"{_activeSettings.IndexOf(_selectedSettingsPage) + 1}")
            .Replace("<st>", $"{_activeSettings.Count}");

        SettingsScreenIcon.sprite = newTerms.DefaultIcon.LoadAsset();
        SettingsScreenIcon.SetSizeLimit(1.44f);
        // Error($"Page Count: {SettingsDescription.Value.textInfo.pageCount}, current page is {SettingsDescription.Value.pageToDisplay}");
    }

    public static string GetVanillaOptionData(BaseGameSetting option)
    {
        var gameOpts = GameOptionsManager.Instance.CurrentGameOptions;
        var value = option.GetValueString(gameOpts.GetValue(option));
        return TranslationController.Instance.GetString(option.Title) + ": " + value;
    }

    public static string GetVanillaOptionData(NumberOption option)
    {
        return TranslationController.Instance.GetString(option.Title) + ": " +
               option.GetValueString(option.GetFloat());
    }

    public static string GetVanillaOptionData(StringOption option)
    {
        return TranslationController.Instance.GetString(option.Title) + ": " +
               option.GetValueString(option.GetFloat());
    }

    public static string GetCategoryHeader(StringNames stringName, bool first = false)
    {
        var text = TranslationController.Instance.GetString(stringName);

        if (first)
        {
            return $"<b><color=#FFFF99>{text}</color></b>";
        }

        return $"<size=50%> </size>\n<b><color=#FFFF99>{text}</color></b>";
    }

    public static string GetCategoryHeader(string title, bool first = false)
    {
        if (first)
        {
            return $"<b><color=#FFFF99>{title}</color></b>";
        }

        return $"<size=50%> </size>\n<b><color=#FFFF99>{title}</color></b>";
    }

    public static string GetLocale(StringNames stringName)
    {
        if (TranslationController.InstanceExists)
        {
            return TranslationController.Instance.GetString(stringName);
        }

        return stringName.ToDisplayString();
    }

    private void LoadTermsScreen()
    {
        TermsScreen.gameObject.SetActive(true);
        if (_selectedTermPage == null)
        {
            SelectTermsPage(_activeTerms[0], false);
        }
    }

    private void ShiftTermsPage(bool goNext)
    {
        if (_selectedTermPage == null)
        {
            SelectTermsPage(_activeTerms[0], false);
        }
        var index = _activeTerms.IndexOf(_selectedTermPage);
        if (goNext)
        {
            if (TermsDescription.pageToDisplay < TermsDescription.textInfo.pageCount)
            {
                ++TermsDescription.pageToDisplay;
            }
            else if (_activeTerms.Count > (index + 1))
            {
                SelectTermsPage(_activeTerms[index + 1], false);
            }
            else
            {
                SelectTermsPage(_activeTerms[0], false);
            }
        }
        else
        {
            if (TermsDescription.pageToDisplay > 1)
            {
                --TermsDescription.pageToDisplay;
            }
            else if (index == 0)
            {
                SelectTermsPage(_activeTerms[^1], true);
            }
            else
            {
                SelectTermsPage(_activeTerms[index - 1], true);
            }
        }

        TermsScreenTabCount.text = TouLocale.GetParsed("TermsPageCount")
            .Replace("<po>", $"{TermsDescription.pageToDisplay}")
            .Replace("<pt>", $"{TermsDescription.textInfo.pageCount}")
            .Replace("<so>", $"{_activeTerms.IndexOf(_selectedTermPage) + 1}")
            .Replace("<st>", $"{_activeTerms.Count}");
        // Error($"Page Count: {TermsDescription.Value.textInfo.pageCount}, current page is {TermsDescription.Value.pageToDisplay}");
    }

    private void SelectTermsPage(TermWikiInfo newTerms, bool lastPage)
    {
        _selectedTermPage = newTerms;
        TermsDescription.text = TouLocale.GetParsed(newTerms.Description).Replace(" • ", "\n• ");
        TermsDescription.ForceMeshUpdate();
        TermsScreenSectionName.text = TouLocale.GetParsed(newTerms.Title);

        TermsDescription.pageToDisplay = lastPage ? TermsDescription.textInfo.pageCount : 1;
        TermsScreenTabCount.text = TouLocale.GetParsed("TermsPageCount")
            .Replace("<po>", $"{TermsDescription.pageToDisplay}")
            .Replace("<pt>", $"{TermsDescription.textInfo.pageCount}")
            .Replace("<so>", $"{_activeTerms.IndexOf(_selectedTermPage) + 1}")
            .Replace("<st>", $"{_activeTerms.Count}");

        TermsScreenIcon.sprite = newTerms.Icon.LoadAsset();
        TermsScreenIcon.SetSizeLimit(1.44f);
        // Error($"Page Count: {TermsDescription.Value.textInfo.pageCount}, current page is {TermsDescription.Value.pageToDisplay}");
    }

    private void LoadDetailScreen()
    {
        if (_selectedItem == null && _selectedSoftItem == null)
        {
            UpdatePage(WikiPage.Homepage);
            return;
        }

        DetailScreen.gameObject.SetActive(true);

        ToggleAbilitiesBtn.gameObject.SetActive((_selectedItem != null)
            ? _selectedItem.Abilities.Count != 0
            : _selectedSoftItem!.Abilities.Count != 0);
        DetailDescription.gameObject.SetActive(true);
        AbilityScroller.transform.parent.gameObject.SetActive(false);
        ToggleAbilitiesBtn.buttonText.text =
            (_selectedItem != null) ? _selectedItem.SecondTabName : _selectedSoftItem!.SecondTabName;

        DetailDescription.text = GetDetailDescription();
        DetailDescription.fontSizeMax = 2.4f;

        if (_selectedItem is ITownOfUsRole touRole)
        {
            DetailScreenItemName.text =
                $"{touRole.RoleName}\n<size=60%>{touRole.RoleColor.ToTextColor()}{MiscUtils.GetParsedRoleAlignment(touRole.RoleAlignment)}</size></color>";
            DetailScreenIcon.sprite = touRole.Configuration.Icon != null
                ? touRole.Configuration.Icon.LoadAsset()
                : TouRoleUtils.GetBasicRoleIcon(touRole);
        }
        else if (_selectedItem is BaseModifier baseModifier)
        {
            var faction = MiscUtils.GetModifierFaction(baseModifier);
            var alignment = MiscUtils.GetParsedModifierFaction(faction);
            var basicFaction = faction.ToString();
            var non = basicFaction.Contains("Non");
            var color = MiscUtils.GetModifierColour(baseModifier);
            if (baseModifier is not AllianceGameModifier)
            {
                if (basicFaction.Contains("Crew") && !non)
                {
                    color = TownOfUsColors.CrewmateWiki;
                }
                else if (basicFaction.Contains("Neut") && !non)
                {
                    color = TownOfUsColors.NeutralWiki;
                }
                else if (basicFaction.Contains("Imp") && !non)
                {
                    color = TownOfUsColors.ImpWiki;
                }
                else if (basicFaction.Contains("Game") || non)
                {
                    color = TownOfUsColors.Other;
                }
                else if (baseModifier is UniversalGameModifier || baseModifier is TouGameModifier)
                {
                    color = baseModifier.FreeplayFileColor;
                }
            }
            DetailScreenItemName.text =
                $"{baseModifier.ModifierName}\n<size=60%>{color.ToTextColor()}{alignment}</size></color>";
            DetailScreenIcon.sprite = baseModifier.ModifierIcon != null
                ? baseModifier.ModifierIcon.LoadAsset()
                : TouRoleIcons.RandomAny.LoadAsset();
        }
        else if (_selectedSoftItem != null)
        {
            DetailScreenItemName.text =
                $"{_selectedSoftItem.EntryName}\n<size=60%>{_selectedSoftItem.EntryColor.ToTextColor()}{TouLocale.Get(_selectedSoftItem.TeamName, _selectedSoftItem.TeamName)}</size></color>";
            DetailScreenIcon.sprite = _selectedSoftItem.Icon != null
                ? _selectedSoftItem.Icon
                : TouRoleIcons.RandomAny.LoadAsset();
            var possibleIcon = TouRoleUtils.TryGetVanillaRoleIcon(_selectedSoftItem.AssociatedRole);
            if (possibleIcon != null)
            {
                DetailScreenIcon.sprite = possibleIcon;
            }
        }

        DetailScreenIcon.SetSizeLimit(1.44f);

        AbilityScroller.Inner.DestroyChildren();

        var max = 0f;
        if (_selectedItem != null)
        {
            foreach (var ability in _selectedItem.Abilities)
            {
                LoadAbilityDetails(ability);
            }

            max = Mathf.Max(0f, _selectedItem.Abilities.Count * 0.875f);
        }
        else if (_selectedSoftItem != null)
        {
            foreach (var ability in _selectedSoftItem.Abilities)
            {
                LoadAbilityDetails(ability);
            }

            max = Mathf.Max(0f, _selectedSoftItem.Abilities.Count * 0.875f);
        }

        AbilityScroller.SetBounds(new FloatRange(-0.5f, max), null);
        AbilityScroller.ScrollToTop();
    }

    private string GetDetailDescription()
    {
        if (_selectedItem == null)
        {
            return _selectedSoftItem!.GetAdvancedDescription;
        }

        var description = _selectedItem.GetAdvancedDescription();
        if (_selectedItem is not BaseModifier mod || !AssassinModifier.IsModifierGuessable(mod))
        {
            return description;
        }
        var guessable = "\n<size=50%> \n</size>This modifier can be guessed by an Assassin.";

        int index = description.IndexOf("\n<size=50%> \n</size>", StringComparison.InvariantCulture);
        if (index != -1)
        {
            return description.Insert(index, guessable);
        }

        return description + guessable;
    }

    private void LoadAbilityDetails(CustomButtonWikiDescription ability)
    {
        var newAbility = Instantiate(AbilityTemplate, AbilityScroller.Inner.transform);
        var icon = newAbility.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        var text = newAbility.GetChild(1).GetComponent<TextMeshPro>();
        var desc = newAbility.GetChild(2).GetComponent<TextMeshPro>();

        icon.sprite = ability.Icon.LoadAsset();
        icon.size = new Vector2(0.8f, 0.8f * icon.sprite.bounds.size.y / icon.sprite.bounds.size.x);
        icon.tileMode = SpriteTileMode.Adaptive;

        text.text =
            $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{ability.Name}</font>";
        desc.text =
            $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{ability.Description}</font>";
        newAbility.gameObject.SetActive(true);
    }

    private void LoadSearchScreen()
    {
        SearchScreen.gameObject.SetActive(true);
        SearchPageText.text = TouLocale.Get(_modifiersSelected ? "Modifiers" : "Roles");
        SearchPageIcon.sprite = _modifiersSelected
            ? TouModifierIcons.Bait.LoadAsset()
            : TouRoleIcons.Parasite.LoadAsset();
        if (!SearchIcon)
        {
            SearchIcon = Instantiate(SearchPageIcon.gameObject, Instance.gameObject.transform);
            SearchIcon.transform.localPosition += new Vector3(0.625f, 0.796f, -1.1f);
            SearchIcon.transform.localScale *= 0.25f;
            var renderer = SearchIcon.GetComponent<SpriteRenderer>();
            renderer.sprite = TouRoleIcons.Forensic.LoadAsset();
            SearchIcon.name = "SearchboxIcon";
        }

        SearchIcon.SetActive(true);

        var oldMax = Mathf.Max(0f, _activeItems.Count * 0.725f);
        _activeItems.Clear();

        SearchTextbox.SetText(string.Empty);

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
                SearchPageIcon.sprite =
                    activeMods.Random()!.ModifierIcon?.LoadAsset() ?? TouModifierIcons.Bait.LoadAsset();
            }

            var modifiers = MiscUtils.AllModifiers
                .OrderBy(x => x, comparer)
                .ToList();

            ToggleRoles(false);
            if (!_modifierEntries.HasAny())
            {
                foreach (var modifier in modifiers)
                {
                    if ((modifier is not IWikiDiscoverable wikiMod || wikiMod.IsHiddenFromList) &&
                        !SoftWikiEntries.ModifierEntries.ContainsKey(modifier))
                    {
                        continue;
                    }

                    var faction = MiscUtils.GetModifierFaction(modifier);
                    var alignment = MiscUtils.GetParsedModifierFaction(faction);
                    var basicFaction = faction.ToString();
                    var color = MiscUtils.GetModifierColour(modifier);
                    var non = basicFaction.Contains("Non");
                    if (modifier is not AllianceGameModifier)
                    {
                        if (basicFaction.Contains("Crew") && !non)
                        {
                            color = TownOfUsColors.CrewmateWiki;
                        }
                        else if (basicFaction.Contains("Neut") && !non)
                        {
                            color = TownOfUsColors.NeutralWiki;
                        }
                        else if (basicFaction.Contains("Imp") && !non)
                        {
                            color = TownOfUsColors.ImpWiki;
                        }
                        else if (basicFaction.Contains("Game") || non)
                        {
                            color = TownOfUsColors.Other;
                        }
                        else if (modifier is UniversalGameModifier || modifier is TouGameModifier)
                        {
                            color = modifier.FreeplayFileColor;
                        }
                    }

                    var modInfoTxt = RemoveNonCaps(modifier.ParentMod.MiraPlugin.OptionsTitleText);

                    var newItem = CreateNewModifierItem(modifier, modifier.ModifierIcon?.LoadAsset(), alignment, color,
                        modInfoTxt);
                    if (modifier is IWikiDiscoverable wikiDiscoverable)
                    {
                        SetupForItem(newItem.gameObject.GetComponent<PassiveButton>(), wikiDiscoverable);
                    }
                    else
                    {
                        SetupForItem(newItem.gameObject.GetComponent<PassiveButton>(),
                            SoftWikiEntries.ModifierEntries.GetValueOrDefault(modifier)!);
                    }
                }
            }
            else
            {
                _activeItems = _modifierEntries.Select(x => x.transform).ToList();
                foreach (var entry in _modifierEntries)
                {
                    entry.SetData();
                }
            }
        }
        else
        {
            List<ushort> roleList = [];

            var curRole = PlayerControl.LocalPlayer.Data.Role.Role;

            if (PlayerControl.LocalPlayer.GetModifiers<BaseModifier>().FirstOrDefault(x =>
                    x is ICachedRole cached && cached.CachedRole.Role != curRole) is ICachedRole cachedMod)
            {
                roleList.Add((ushort)cachedMod.CachedRole.Role);
            }

            roleList.Add((ushort)curRole);

            if (PlayerControl.LocalPlayer.Data.IsDead &&
                !roleList.Contains((ushort)PlayerControl.LocalPlayer.GetRoleWhenAlive().Role))
            {
                roleList.Add((ushort)PlayerControl.LocalPlayer.GetRoleWhenAlive().Role);
            }

            var aliveRole = PlayerControl.LocalPlayer.GetRoleWhenAlive();
            if (aliveRole != null)
            {
                SearchPageIcon.sprite = aliveRole.RoleIconSolid ?? TouRoleIcons.Parasite.LoadAsset();
            }

            var comparer = new RoleComparer(roleList);
            if (!_roleList.HasAny())
            {
                _roleList = MiscUtils.AllRegisteredRoles.Excluding(role =>
                    !SoftWikiEntries.RoleEntries.ContainsKey(role) && role is not IWikiDiscoverable ||
                    role is IWikiDiscoverable wikiMod && wikiMod.IsHiddenFromList).ToList();
            }

            var roles = _roleList.OrderBy(x => x, comparer);
            /*_activeItems = _activeItems
                .OrderByDescending(child => child.name.Equals(text, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(child => child.name.Contains(text, StringComparison.InvariantCultureIgnoreCase))
                .ThenBy(child => child.name.ToLowerInvariant())
                .ToList();*/

            ToggleRoles(true);
            if (!_roleEntries.HasAny())
            {
                foreach (var role in roles)
                {
                    var customRole = role as ICustomRole;
                    var color = role.IsCrewmate() ? TownOfUsColors.CrewmateWiki : TownOfUsColors.ImpWiki;

                    var teamName = MiscUtils.GetParsedRoleAlignment(role);
                    var roleImg = TouRoleUtils.GetBasicRoleIcon(role);
                    var modInfoTxt = "AU";
                    if (customRole != null)
                    {
                        // Hides hidden roles from other mods, but keeps them visible for Pest/Mayor
                        if (customRole.Configuration.HideSettings && role is not IWikiDiscoverable)
                        {
                            continue;
                        }
                        modInfoTxt = RemoveNonCaps(customRole.ParentMod.MiraPlugin.OptionsTitleText);

                        if (customRole.Team is ModdedRoleTeams.Crewmate)
                        {
                            color = TownOfUsColors.CrewmateWiki;
                        }
                        else if (customRole.Team is ModdedRoleTeams.Impostor)
                        {
                            color = TownOfUsColors.ImpWiki;
                        }
                        else
                        {
                            color = TownOfUsColors.NeutralWiki;
                        }

                        if (customRole.Configuration.Icon != null)
                        {
                            roleImg = customRole.Configuration.Icon.LoadAsset();
                        }
                    }
                    else if (role.RoleIconSolid != null)
                    {
                        roleImg = role.RoleIconSolid;
                    }

                    var newItem = customRole == null
                        ? CreateNewRoleItem(role, roleImg, teamName, color, modInfoTxt)
                        : CreateNewRoleItem(role, customRole, roleImg, teamName, color, modInfoTxt);

                    if (role is IWikiDiscoverable wikiDiscoverable)
                    {
                        SetupForItem(newItem.gameObject.GetComponent<PassiveButton>(), wikiDiscoverable);
                    }
                    else
                    {
                        SetupForItem(newItem.gameObject.GetComponent<PassiveButton>(),
                            SoftWikiEntries.RoleEntries.GetValueOrDefault(role)!);
                    }
                }
            }
            else
            {
                _activeItems = _roleEntries.Select(x => x.transform).ToList();
                foreach (var entry in _roleEntries)
                {
                    entry.SetData();
                }
            }
        }

        SearchPageIcon.SetSizeLimit(1.44f);

        var max = Mathf.Max(0f, _activeItems.Count * 0.725f);
        SearchScroller.SetBounds(new FloatRange(-0.4f, max), null);
        if (oldMax != max)
        {
            SearchScroller.ScrollToTop();
        }
    }

    public void ToggleRoles(bool showRoles)
    {
        _roleEntries.Do(x => x.gameObject.SetActive(showRoles));
        _modifierEntries.Do(x => x.gameObject.SetActive(!showRoles));
    }
    private void SetupForItem(PassiveButton passiveButton, IWikiDiscoverable wikiDiscoverable)
    {
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            _selectedItem = wikiDiscoverable;
            _selectedSoftItem = null!;
            UpdatePage(WikiPage.DetailScreen);
        }));
    }

    private void SetupForItem(PassiveButton passiveButton, SoftWikiInfo softInfo)
    {
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            _selectedSoftItem = softInfo;
            _selectedItem = null!;
            UpdatePage(WikiPage.DetailScreen);
        }));
    }

    private Transform CreateNewRoleItem(RoleBehaviour role, Sprite? sprite, string team, Color color, string source)
    {
        var newItem = Instantiate(RoleSearchItemTemplate, SearchScroller.Inner);
        newItem.gameObject.SetActive(true);
        var newSprite = sprite ?? TouRoleIcons.RandomAny.LoadAsset();

        newItem.SetInitialData(role, newSprite, team, color, source);
        _activeItems.Add(newItem.transform);
        _roleEntries.Add(newItem);
        return newItem.transform;
    }

    private Transform CreateNewRoleItem(RoleBehaviour role, ICustomRole customRole, Sprite? sprite, string team, Color color, string source)
    {
        var newItem = Instantiate(RoleSearchItemTemplate, SearchScroller.Inner);
        newItem.gameObject.SetActive(true);
        var newSprite = sprite ?? TouRoleIcons.RandomAny.LoadAsset();

        newItem.SetInitialData(role, customRole, newSprite, team, color, source);
        _activeItems.Add(newItem.transform);
        _roleEntries.Add(newItem);
        return newItem.transform;
    }

    private Transform CreateNewModifierItem(BaseModifier mod, Sprite? sprite, string team, Color color, string source)
    {
        var newItem = Instantiate(ModifierSearchItemTemplate, SearchScroller.Inner);
        newItem.gameObject.SetActive(true);
        var newSprite = sprite ?? TouRoleIcons.RandomAny.LoadAsset();

        newItem.SetInitialData(mod, newSprite, team, color, source);
        _activeItems.Add(newItem.transform);
        _modifierEntries.Add(newItem);
        return newItem.transform;
    }

    public static IngameWikiMinigame Create()
    {
        var gameObject = Instantiate(TouAssets.WikiPrefab.LoadAsset(), HudManager.Instance.transform);
        gameObject.transform.SetParent(Camera.main!.transform, false);
        gameObject.transform.localPosition = new Vector3(0f, 0f, -150f);
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

        TownOfUsColors.UseBasic =
            LocalSettingsTabSingleton<TownOfUsLocalRoleSettings>.Instance.UseCrewmateTeamColorToggle.Value;
    }

    public void OpenFor(IWikiDiscoverable wikiDiscoverable)
    {
        _selectedItem = wikiDiscoverable;
        _selectedSoftItem = null!;
        UpdatePage(WikiPage.DetailScreen);
    }

    public void OpenFor(SoftWikiInfo softWikiInfo)
    {
        _selectedItem = null!;
        _selectedSoftItem = softWikiInfo;
        UpdatePage(WikiPage.DetailScreen);
    }

    private static string RemoveNonCaps(string text)
    {
        return new string(text.Where(c => !Char.IsLower(c) && !Char.IsWhiteSpace(c)).ToArray());
    }
}

public enum WikiPage
{
    Homepage,
    SearchScreen,
    DetailScreen,
    TermsScreen,
    SettingsScreen
}

public record struct TermWikiInfo(string Title, string Description, LoadableAsset<Sprite> Icon);
public record struct OptionWikiInfo(string Title, List<AbstractOptionGroup> OptionGroups, LoadableAsset<Sprite> DefaultIcon, bool IsVanilla = false);