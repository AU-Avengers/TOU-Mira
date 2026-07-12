using TMPro;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public class IngameWikiMinigame : Minigame
{
    public Transform Homepage;
    public PassiveButton HomepageModifiersBtn;
    public PassiveButton HomepageRolesBtn;
    public PassiveButton HomepageTermsBtn;
    public PassiveButton HomepageSettingsBtn;
    public PassiveButton CloseButton;
    public PassiveButton OutsideCloseButton;

    public Transform SearchScreen;
    public PassiveButton SearchScreenBackBtn;
    public TextBoxTMP SearchTextbox;
    public Scroller SearchScroller;
    public InGameModifierWikiEntry ModifierSearchItemTemplate;
    public InGameRoleWikiEntry RoleSearchItemTemplate;
    public SpriteRenderer SearchPageIcon;
    public TextMeshPro SearchPageText;

    public Transform DetailScreen;
    public TextMeshPro DetailDescription;
    public PassiveButton ToggleAbilitiesBtn;
    public Transform AbilityTemplate;
    public Scroller AbilityScroller;
    public PassiveButton DetailScreenBackBtn;
    public SpriteRenderer DetailScreenIcon;
    public TextMeshPro DetailScreenItemName;

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
}
