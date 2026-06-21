using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
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

[RegisterInIl2Cpp]
public sealed class IngameWikiMinigame(nint cppPtr) : Minigame(cppPtr)
{
    public GameObject SearchIcon;
    // TODO: Improve the wiki to store all entries rather than destroy then. All items should also use a mono behaviour that allows the chances and amount to change immediately without issue.
    private List<Transform> _activeItems = [];
    /*private List<Transform> _inactiveItems = [];
    private List<Transform> _allItems = [];*/
    private readonly List<InGameModifierWikiEntry> _modifierEntries = [];
    private readonly List<InGameRoleWikiEntry> _roleEntries = [];
    private List<RoleBehaviour> _roleList = [];

    private WikiPage _currentPage = WikiPage.Homepage;
    private bool _modifiersSelected;
    private IWikiDiscoverable? _selectedItem;
    private SoftWikiInfo? _selectedSoftItem;
    private TermWikiInfo? _selectedTermPage;
    public readonly List<TermWikiInfo> _activeTerms = [];
    private OptionWikiInfo? _selectedSettingsPage;
    public readonly List<OptionWikiInfo> _activeSettings = [];
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
    public Il2CppReferenceField<PassiveButton> HomepageTermsBtn;
    public Il2CppReferenceField<PassiveButton> HomepageSettingsBtn;
    public Il2CppReferenceField<PassiveButton> OutsideCloseButton;
    public Il2CppReferenceField<InGameModifierWikiEntry> ModifierSearchItemTemplate;
    public Il2CppReferenceField<InGameRoleWikiEntry> RoleSearchItemTemplate;
    public Il2CppReferenceField<SpriteRenderer> SearchPageIcon;
    public Il2CppReferenceField<TextMeshPro> SearchPageText;

    public Il2CppReferenceField<Transform> SearchScreen;
    public Il2CppReferenceField<PassiveButton> SearchScreenBackBtn;
    public Il2CppReferenceField<Scroller> SearchScroller;
    public Il2CppReferenceField<TextBoxTMP> SearchTextbox;
    public Il2CppReferenceField<PassiveButton> ToggleAbilitiesBtn;

    public Il2CppReferenceField<Transform> TermsScreen;
    public Il2CppReferenceField<TextMeshPro> TermsDescription;
    public Il2CppReferenceField<PassiveButton> TermsPreviousBtn;
    public Il2CppReferenceField<PassiveButton> TermsNextBtn;
    public Il2CppReferenceField<PassiveButton> TermsBackBtn;
    public Il2CppReferenceField<SpriteRenderer> TermsScreenIcon;
    public Il2CppReferenceField<TextMeshPro> TermsScreenSectionName;
    public Il2CppReferenceField<TextMeshPro> TermsScreenTabCount;

    public Il2CppReferenceField<Transform> SettingsScreen;
    public Il2CppReferenceField<TextMeshPro> SettingsDescription;
    public Il2CppReferenceField<PassiveButton> SettingsPreviousBtn;
    public Il2CppReferenceField<PassiveButton> SettingsNextBtn;
    public Il2CppReferenceField<PassiveButton> SettingsBackBtn;
    public Il2CppReferenceField<SpriteRenderer> SettingsScreenIcon;
    public Il2CppReferenceField<TextMeshPro> SettingsScreenSectionName;
    public Il2CppReferenceField<TextMeshPro> SettingsScreenTabCount;

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
        instance._activeSettings.Add(new OptionWikiInfo("WikiSettingsAmongUsGameSettingsTitle", new List<AbstractOptionGroup>(), TouRoleIcons.Detective, true));
        instance._activeSettings.Add(new OptionWikiInfo("WikiSettingsTouMiraGameSettingsTitle",
            new List<AbstractOptionGroup>()
            {
                OptionGroupSingleton<GeneralOptions>.Instance, OptionGroupSingleton<VanillaTweakOptions>.Instance,
                OptionGroupSingleton<GameMechanicOptions>.Instance, OptionGroupSingleton<PostmortemOptions>.Instance,
                OptionGroupSingleton<GameTimerOptions>.Instance, OptionGroupSingleton<TaskTrackingOptions>.Instance
            }, TouRoleIcons.Engineer));
        instance._activeSettings.Add(new OptionWikiInfo("WikiSettingsMapsSabotageSettingsTitle",
            new List<AbstractOptionGroup>()
            {
                OptionGroupSingleton<GlobalBetterMapOptions>.Instance, OptionGroupSingleton<AdvancedUtilityOptions>.Instance,
                OptionGroupSingleton<AdvancedSabotageOptions>.Instance
            }, TouModifierIcons.Operative));
        instance._activeSettings.Add(new OptionWikiInfo("WikiSettingsBetterMapsSettingsTitle",
            new List<AbstractOptionGroup>()
            {
                OptionGroupSingleton<BetterSkeldOptions>.Instance, OptionGroupSingleton<BetterMiraHqOptions>.Instance,
                OptionGroupSingleton<BetterPolusOptions>.Instance, OptionGroupSingleton<BetterAirshipOptions>.Instance,
                OptionGroupSingleton<BetterFungleOptions>.Instance, OptionGroupSingleton<BetterSubmergedOptions>.Instance,
                OptionGroupSingleton<BetterLevelImpostorOptions>.Instance
            }, TouRoleIcons.Spy));
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

        SearchPageIcon.Value.SetSizeLimit(1.44f);
        DetailScreenIcon.Value.SetSizeLimit(1.44f);
        if (HomepageModifiersBtn.Value.transform.GetChild(0).TryGetComponent<SpriteRenderer>(out var modIcon))
        {
            modIcon.SetSizeLimit(1.44f);
        }

        if (HomepageRolesBtn.Value.transform.GetChild(0).TryGetComponent<SpriteRenderer>(out var roleIcon))
        {
            roleIcon.SetSizeLimit(1.44f);
        }

        UpdatePage(WikiPage.Homepage);

        var closeAction = new Action(() => { Close(); });

        CloseButton.Value.OnClick.AddListener((UnityAction)closeAction);
        OutsideCloseButton.Value.OnClick.AddListener((UnityAction)closeAction);
        HomepageModifiersBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("Modifiers", "Modifiers");
        HomepageModifiersBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            _modifiersSelected = true;
            UpdatePage(WikiPage.SearchScreen);
        }));

        HomepageRolesBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("Roles", "Roles");
        HomepageRolesBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            _modifiersSelected = false;
            UpdatePage(WikiPage.SearchScreen);
        }));

        HomepageTermsBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("Terminology", "Terminology");
        HomepageTermsBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            UpdatePage(WikiPage.TermsScreen);
        }));

        TermsBackBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        TermsBackBtn.Value.OnClick.AddListener((UnityAction)(() => { UpdatePage(WikiPage.Homepage); }));

        TermsPreviousBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("PreviousButtonText", "Previous");
        TermsPreviousBtn.Value.OnClick.AddListener((UnityAction)(() => { ShiftTermsPage(false); }));

        TermsNextBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("NextButtonText", "Next");
        TermsNextBtn.Value.OnClick.AddListener((UnityAction)(() => { ShiftTermsPage(true); }));

        SearchScreenBackBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        SearchScreenBackBtn.Value.OnClick.AddListener((UnityAction)(() => { UpdatePage(WikiPage.Homepage); }));

        DetailScreenBackBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        DetailScreenBackBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            _selectedItem = null;
            _selectedSoftItem = null;
            UpdatePage(WikiPage.SearchScreen);
        }));

        HomepageSettingsBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("GameSettings", "GameSettings");
        HomepageSettingsBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            UpdatePage(WikiPage.SettingsScreen);
        }));

        SettingsBackBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        SettingsBackBtn.Value.OnClick.AddListener((UnityAction)(() => { UpdatePage(WikiPage.Homepage); }));

        SettingsPreviousBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("PreviousButtonText", "Previous");
        SettingsPreviousBtn.Value.OnClick.AddListener((UnityAction)(() => { ShiftSettingsPage(false); }));

        SettingsNextBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("NextButtonText", "Next");
        SettingsNextBtn.Value.OnClick.AddListener((UnityAction)(() => { ShiftSettingsPage(true); }));

        SearchScreenBackBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        SearchScreenBackBtn.Value.OnClick.AddListener((UnityAction)(() => { UpdatePage(WikiPage.Homepage); }));

        DetailScreenBackBtn.Value.GetComponentInChildren<TextMeshPro>().text = TouLocale.Get("BackButtonText", "Back");
        DetailScreenBackBtn.Value.OnClick.AddListener((UnityAction)(() =>
        {
            _selectedItem = null;
            _selectedSoftItem = null;
            UpdatePage(WikiPage.SearchScreen);
        }));

        SearchTextbox.Value.transform.GetParent().GetChild(2).GetComponent<TextMeshPro>().text =
            TouLocale.Get("SearchboxHeadsUp", "Search Here");
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
                ToggleAbilitiesBtn.Value.buttonText.text = TouLocale.Get("WikiDescriptionTab", "Description");
                DetailDescription.Value.gameObject.SetActive(false);
                AbilityScroller.Value.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                ToggleAbilitiesBtn.Value.buttonText.text =
                    _selectedItem != null
                        ? _selectedItem.SecondTabName
                        : TouLocale.Get("WikiAbilitiesTab", "Abilities");
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
        TermsScreen.Value.gameObject.SetActive(false);
        SettingsScreen.Value.gameObject.SetActive(false);
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
                    .Where(x => x is IWikiDiscoverable || SoftWikiEntries.ModifierEntries.ContainsKey(x)).ToList();
                SpriteRenderer? modifierIcon = null;
                SpriteRenderer? playerRoleIcon = null;

                if (activeMods.Count > 0 && HomepageModifiersBtn.Value.transform.GetChild(0)
                        .TryGetComponent<SpriteRenderer>(out var modIcon))
                {
                    modifierIcon = modIcon;
                    modIcon.sprite = activeMods.Random()!.ModifierIcon?.LoadAsset() ??
                                     TouModifierIcons.Bait.LoadAsset();
                }

                var aliveRole = PlayerControl.LocalPlayer.GetRoleWhenAlive();
                if (aliveRole != null && HomepageRolesBtn.Value.transform.GetChild(0)
                        .TryGetComponent<SpriteRenderer>(out var roleIcon))
                {
                    playerRoleIcon = roleIcon;
                    roleIcon.sprite = aliveRole.RoleIconSolid ?? TouRoleIcons.Parasite.LoadAsset();
                }

                if (modifierIcon != null)
                {
                    modifierIcon.SetSizeLimit(1.44f);
                }

                if (playerRoleIcon != null)
                {
                    playerRoleIcon.SetSizeLimit(1.44f);
                }

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
        SettingsScreen.Value.gameObject.SetActive(true);
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
        var index = _activeSettings.IndexOf(_selectedSettingsPage!.Value);
        if (goNext)
        {
            if (SettingsDescription.Value.pageToDisplay < SettingsDescription.Value.textInfo.pageCount)
            {
                ++SettingsDescription.Value.pageToDisplay;
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
            if (SettingsDescription.Value.pageToDisplay > 1)
            {
                --SettingsDescription.Value.pageToDisplay;
            }
            else if (index == 0)
            {
                SelectSettingsPage(_activeSettings[_activeSettings.Count - 1], true);
            }
            else
            {
                SelectSettingsPage(_activeSettings[index - 1], true);
            }
        }

        SettingsScreenTabCount.Value.text = TouLocale.GetParsed("TermsPageCount")
            .Replace("<po>", $"{SettingsDescription.Value.pageToDisplay}")
            .Replace("<pt>", $"{SettingsDescription.Value.textInfo.pageCount}")
            .Replace("<so>", $"{_activeSettings.IndexOf(_selectedSettingsPage!.Value) + 1}")
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
                IWikiOptionsSummaryProvider? summaryProvider = null;
                IReadOnlySet<StringNames>? hiddenKeys = null;
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
                    summaryProvider = null;
                    hiddenKeys = null;
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

        SettingsDescription.Value.text = sBuilder.ToString();
        SettingsDescription.Value.ForceMeshUpdate();
        SettingsScreenSectionName.Value.text = TouLocale.GetParsed(newTerms.Title);

        SettingsDescription.Value.pageToDisplay = lastPage ? SettingsDescription.Value.textInfo.pageCount : 1;
        SettingsScreenTabCount.Value.text = TouLocale.GetParsed("TermsPageCount")
            .Replace("<po>", $"{SettingsDescription.Value.pageToDisplay}")
            .Replace("<pt>", $"{SettingsDescription.Value.textInfo.pageCount}")
            .Replace("<so>", $"{_activeSettings.IndexOf(_selectedSettingsPage!.Value) + 1}")
            .Replace("<st>", $"{_activeSettings.Count}");

        SettingsScreenIcon.Value.sprite = newTerms.DefaultIcon.LoadAsset();
        SettingsScreenIcon.Value.SetSizeLimit(1.44f);
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
        TermsScreen.Value.gameObject.SetActive(true);
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
        var index = _activeTerms.IndexOf(_selectedTermPage!.Value);
        if (goNext)
        {
            if (TermsDescription.Value.pageToDisplay < TermsDescription.Value.textInfo.pageCount)
            {
                ++TermsDescription.Value.pageToDisplay;
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
            if (TermsDescription.Value.pageToDisplay > 1)
            {
                --TermsDescription.Value.pageToDisplay;
            }
            else if (index == 0)
            {
                SelectTermsPage(_activeTerms[_activeTerms.Count - 1], true);
            }
            else
            {
                SelectTermsPage(_activeTerms[index - 1], true);
            }
        }

        TermsScreenTabCount.Value.text = TouLocale.GetParsed("TermsPageCount")
            .Replace("<po>", $"{TermsDescription.Value.pageToDisplay}")
            .Replace("<pt>", $"{TermsDescription.Value.textInfo.pageCount}")
            .Replace("<so>", $"{_activeTerms.IndexOf(_selectedTermPage!.Value) + 1}")
            .Replace("<st>", $"{_activeTerms.Count}");
        // Error($"Page Count: {TermsDescription.Value.textInfo.pageCount}, current page is {TermsDescription.Value.pageToDisplay}");
    }

    private void SelectTermsPage(TermWikiInfo newTerms, bool lastPage)
    {
        _selectedTermPage = newTerms;
        TermsDescription.Value.text = TouLocale.GetParsed(newTerms.Description).Replace(" • ", "\n• ");
        TermsDescription.Value.ForceMeshUpdate();
        TermsScreenSectionName.Value.text = TouLocale.GetParsed(newTerms.Title);

        TermsDescription.Value.pageToDisplay = lastPage ? TermsDescription.Value.textInfo.pageCount : 1;
        TermsScreenTabCount.Value.text = TouLocale.GetParsed("TermsPageCount")
            .Replace("<po>", $"{TermsDescription.Value.pageToDisplay}")
            .Replace("<pt>", $"{TermsDescription.Value.textInfo.pageCount}")
            .Replace("<so>", $"{_activeTerms.IndexOf(_selectedTermPage!.Value) + 1}")
            .Replace("<st>", $"{_activeTerms.Count}");

        TermsScreenIcon.Value.sprite = newTerms.Icon.LoadAsset();
        TermsScreenIcon.Value.SetSizeLimit(1.44f);
        // Error($"Page Count: {TermsDescription.Value.textInfo.pageCount}, current page is {TermsDescription.Value.pageToDisplay}");
    }

    private void LoadDetailScreen()
    {
        if (_selectedItem == null && _selectedSoftItem == null)
        {
            UpdatePage(WikiPage.Homepage);
            return;
        }

        DetailScreen.Value.gameObject.SetActive(true);

        ToggleAbilitiesBtn.Value.gameObject.SetActive((_selectedItem != null)
            ? _selectedItem.Abilities.Count != 0
            : _selectedSoftItem!.Abilities.Count != 0);
        DetailDescription.Value.gameObject.SetActive(true);
        AbilityScroller.Value.transform.parent.gameObject.SetActive(false);
        ToggleAbilitiesBtn.Value.buttonText.text =
            (_selectedItem != null) ? _selectedItem.SecondTabName : _selectedSoftItem!.SecondTabName;

        DetailDescription.Value.text = GetDetailDescription();
        DetailDescription.Value.fontSizeMax = 2.4f;

        if (_selectedItem is ITownOfUsRole touRole)
        {
            DetailScreenItemName.Value.text =
                $"{touRole.RoleName}\n<size=60%>{touRole.RoleColor.ToTextColor()}{MiscUtils.GetParsedRoleAlignment(touRole.RoleAlignment)}</size></color>";
            DetailScreenIcon.Value.sprite = touRole.Configuration.Icon != null
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
            DetailScreenItemName.Value.text =
                $"{baseModifier.ModifierName}\n<size=60%>{color.ToTextColor()}{alignment}</size></color>";
            DetailScreenIcon.Value.sprite = baseModifier.ModifierIcon != null
                ? baseModifier.ModifierIcon.LoadAsset()
                : TouRoleIcons.RandomAny.LoadAsset();
        }
        else if (_selectedSoftItem != null)
        {
            DetailScreenItemName.Value.text =
                $"{_selectedSoftItem.EntryName}\n<size=60%>{_selectedSoftItem.EntryColor.ToTextColor()}{TouLocale.Get(_selectedSoftItem.TeamName, _selectedSoftItem.TeamName)}</size></color>";
            DetailScreenIcon.Value.sprite = _selectedSoftItem.Icon != null
                ? _selectedSoftItem.Icon
                : TouRoleIcons.RandomAny.LoadAsset();
            var possibleIcon = TouRoleUtils.TryGetVanillaRoleIcon(_selectedSoftItem.AssociatedRole);
            if (possibleIcon != null)
            {
                DetailScreenIcon.Value.sprite = possibleIcon;
            }
        }

        DetailScreenIcon.Value.SetSizeLimit(1.44f);

        AbilityScroller.Value.Inner.DestroyChildren();

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

        AbilityScroller.Value.SetBounds(new FloatRange(-0.5f, max), null);
        AbilityScroller.Value.ScrollToTop();
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

    private void LoadSearchScreen()
    {
        SearchScreen.Value.gameObject.SetActive(true);
        SearchPageText.Value.text = TouLocale.Get(_modifiersSelected ? "Modifiers" : "Roles");
        SearchPageIcon.Value.sprite = _modifiersSelected
            ? TouModifierIcons.Bait.LoadAsset()
            : TouRoleIcons.Parasite.LoadAsset();
        if (!SearchIcon)
        {
            SearchIcon = Instantiate(SearchPageIcon.Value.gameObject, Instance.gameObject.transform);
            SearchIcon.transform.localPosition += new Vector3(0.625f, 0.796f, -1.1f);
            SearchIcon.transform.localScale *= 0.25f;
            var renderer = SearchIcon.GetComponent<SpriteRenderer>();
            renderer.sprite = TouRoleIcons.Forensic.LoadAsset();
            SearchIcon.name = "SearchboxIcon";
        }

        SearchIcon.SetActive(true);

        var oldMax = Mathf.Max(0f, _activeItems.Count * 0.725f);
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
                SearchPageIcon.Value.sprite =
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
                            SoftWikiEntries.ModifierEntries.GetValueOrDefault(modifier));
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

            var cachedMod =
                PlayerControl.LocalPlayer.GetModifiers<BaseModifier>().FirstOrDefault(x =>
                    x is ICachedRole cached && cached.CachedRole.Role != curRole) as ICachedRole;
            if (cachedMod != null)
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
                SearchPageIcon.Value.sprite = aliveRole.RoleIconSolid ?? TouRoleIcons.Parasite.LoadAsset();
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
                    if (customRole != null)
                    {
                        // Hides hidden roles from other mods, but keeps them visible for Pest/Mayor
                        if (customRole.Configuration.HideSettings && role is not IWikiDiscoverable)
                        {
                            continue;
                        }

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

                    var modInfoTxt = "AU";

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
                            SoftWikiEntries.RoleEntries.GetValueOrDefault(role));
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

        SearchPageIcon.Value.SetSizeLimit(1.44f);

        var max = Mathf.Max(0f, _activeItems.Count * 0.725f);
        SearchScroller.Value.SetBounds(new FloatRange(-0.4f, max), null);
        if (oldMax != max)
        {
            SearchScroller.Value.ScrollToTop();
        }
    }

    public void ToggleRoles(bool showRoles)
    {
        _roleEntries.Do(x => x.gameObject.SetActive(showRoles));
        _modifierEntries.Do(x => x.gameObject.SetActive(!showRoles));
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

    private Transform CreateNewRoleItem(RoleBehaviour role, Sprite? sprite, string team, Color color, string source)
    {
        var newItem = Instantiate(RoleSearchItemTemplate.Value, SearchScroller.Value.Inner);
        newItem.gameObject.SetActive(true);
        var newSprite = sprite != null ? sprite : TouRoleIcons.RandomAny.LoadAsset();

        newItem.SetInitialData(role, newSprite, team, color, source);
        _activeItems.Add(newItem.transform);
        _roleEntries.Add(newItem);
        return newItem.transform;
    }

    private Transform CreateNewRoleItem(RoleBehaviour role, ICustomRole customRole, Sprite? sprite, string team, Color color, string source)
    {
        var newItem = Instantiate(RoleSearchItemTemplate.Value, SearchScroller.Value.Inner);
        newItem.gameObject.SetActive(true);
        var newSprite = sprite != null ? sprite : TouRoleIcons.RandomAny.LoadAsset();

        newItem.SetInitialData(role, customRole, newSprite, team, color, source);
        _activeItems.Add(newItem.transform);
        _roleEntries.Add(newItem);
        return newItem.transform;
    }

    private Transform CreateNewModifierItem(BaseModifier mod, Sprite? sprite, string team, Color color, string source)
    {
        var newItem = Instantiate(ModifierSearchItemTemplate.Value, SearchScroller.Value.Inner);
        newItem.gameObject.SetActive(true);
        var newSprite = sprite != null ? sprite : TouRoleIcons.RandomAny.LoadAsset();

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