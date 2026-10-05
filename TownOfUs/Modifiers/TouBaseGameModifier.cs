using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using TMPro;
using TownOfUs.Modules;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers;

[MiraIgnore]
public abstract class TouBaseGameModifier : GameModifier
{
    public override string IdPrefix => "TownOfUsMira.Modifier";
    public override string IdPart => "KEY_MISS";
    public virtual string IntroInfo => $"{MiraLocaleManager.Get("Modifier")}: {ModifierName}";
    public virtual float IntroSize => 4f;
    public virtual ModifierFaction FactionType => ModifierFaction.Universal;
    public override string ModifierCategoryTitle => MiscUtils.GetParsedModifierFaction(FactionType);
    public override Color GeneralColor => MiscUtils.GetRoleColour(IdPart);
    public override string ModifierWikiDescription => this is IWikiDiscoverable wiki ? wiki.GetAdvancedDescription() : MiraLocaleManager.Get(ModifierWikiDescriptionLocale, GetDescription()) +
                                                      Helpers.GetOptionsText(GetType());
    public virtual ModifierUiConfiguration Configuration => new(MiscUtils.GetRoleColour(IdPart));
    public override GameObject GetAdvancedWiki(MatchInfoGuide guide, TextMeshPro titleText, Scroller parent)
    {
        parent.ScrollToTop();
        var obj = Helpers.CreateAdvancedWikiTab(
            guide,
            ModifierNameLocale,
            ModifierName + $" ({ModifierCategoryTitle})",
            ModifierWikiDescription,
            titleText,
            out var desc);
        var num = 0;
        if (WikiAbilities.Count != 0)
        {
            var grid = Object.Instantiate(parent.Inner, obj.transform);
            var layoutGroup = grid.GetComponent<GridLayoutGroup>();
            layoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.spacing = new Vector2(0.75f, 0.4f);
            grid.DestroyChildren();
            foreach (var ability in WikiAbilities)
            {
                num++;
                var panel = Object.Instantiate(
                    guide.MatchInfoRolePanelPrefab,
                    grid);
                panel.roleCount.text = ability.AbilityType;
                panel.roleCount.transform.localPosition += new Vector3(-0.04f, 0);
                panel.roleName.text = ability.Name;
                panel.roleName.transform.localPosition += new Vector3(-0.04f, 0);
                panel.roleDescription.text = ability.Description;
                panel.roleDescription.rectTransform.sizeDelta = new Vector2(2.601f, 0.8f);
                panel.roleDescription.transform.localPosition += new Vector3(0, -0.1f);
                panel.roleIcon.sprite = ability.Icon.LoadAsset();
                panel.roleIcon.SetSizeLimit(0.13f);

                panel.roleIcon.material.SetInt(PlayerMaterial.MaskLayer, 50);
                panel.roleName.fontMaterial.SetFloat(panel.STENCIL_NAME, 50f);
                panel.roleDescription.fontMaterial.SetFloat(panel.STENCIL_NAME, 50f);
                panel.roleCount.fontMaterial.SetFloat(panel.STENCIL_NAME, 50f);
                panel.roleIcon.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
            }

            grid.localPosition = new Vector3(-3.9f, 1.1f - desc.textBounds.size.y, 0f);
            grid.localScale = new Vector3(1.3f, 1.3f, 1);
        }
        else if (this is IWikiDiscoverable wiki && wiki.Abilities.HasAny())
        {
            var grid = Object.Instantiate(parent.Inner, obj.transform);
            var layoutGroup = grid.GetComponent<GridLayoutGroup>();
            layoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.spacing = new Vector2(0.75f, 0.4f);
            grid.DestroyChildren();
            foreach (var ability in wiki.Abilities)
            {
                num++;
                var panel = Object.Instantiate(
                    guide.MatchInfoRolePanelPrefab,
                    grid);
                panel.roleCount.text = "Ability";
                panel.roleCount.transform.localPosition += new Vector3(-0.04f, 0);
                panel.roleName.text = ability.Name;
                panel.roleName.transform.localPosition += new Vector3(-0.04f, 0);
                panel.roleDescription.text = ability.Description;
                panel.roleDescription.rectTransform.sizeDelta = new Vector2(2.601f, 0.8f);
                panel.roleDescription.transform.localPosition += new Vector3(0, -0.1f);
                panel.roleIcon.sprite = ability.Icon.LoadAsset();
                panel.roleIcon.SetSizeLimit(0.13f);

                panel.roleIcon.material.SetInt(PlayerMaterial.MaskLayer, 50);
                panel.roleName.fontMaterial.SetFloat(panel.STENCIL_NAME, 50f);
                panel.roleDescription.fontMaterial.SetFloat(panel.STENCIL_NAME, 50f);
                panel.roleCount.fontMaterial.SetFloat(panel.STENCIL_NAME, 50f);
                panel.roleIcon.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
            }

            grid.localPosition = new Vector3(-3.9f, 1.1f - desc.textBounds.size.y, 0f);
            grid.localScale = new Vector3(1.3f, 1.3f, 1);
        }

        obj.transform.SetParent(parent.Inner.transform);
        obj.transform.localPosition = new Vector3(0f, 0f, 0f);
        parent.SetYBoundsMax(Mathf.Clamp((desc.textBounds.size.y - 2) + (Mathf.Ceil(num / 2f) * 1.45f), 0f, 999f));
        return obj;
    }
    
    /// <summary>
    /// Method that runs before <see cref="GameModifier.IsModifierValidOn"/> is run by MiraAPI. This is used for Assailant modifiers to determine if they may spawn.
    /// </summary>
    public virtual void BeforeModifierSpawns()
    {
        // Empty!
    }

    public virtual int CustomAmount => GetAmountPerGame();
    public virtual int CustomChance => GetAssignmentChance();

    public override bool HideOnUi => false;

    public override int GetAmountPerGame()
    {
        return 1;
    }

    public override void OnActivate()
    {
        base.OnActivate();
        AddModifierToStats(GameHistory.PlayerStats[Player.PlayerId]);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        RemoveModifierFromStats(GameHistory.PlayerStats[Player.PlayerId]);
    }

    public virtual void AddModifierToStats(PlayerStats stats)
    {
        // stats.LastKnownModifiers.Add(this);
    }
    public virtual void RemoveModifierFromStats(PlayerStats stats)
    {
        if (stats.LastKnownModifiers.Contains(this))
        {
            stats.LastKnownModifiers.Remove(this);
        }
    }
}

/// <summary>
/// Used to configure the specific visuals for option notifications.
/// </summary>
public record struct ModifierUiConfiguration
{
#pragma warning disable S1133
    [Obsolete("Default constructor is not supported")]
#pragma warning restore S1133
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public ModifierUiConfiguration()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        throw new NotImplementedException("Default constructor is not supported.");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModifierUiConfiguration"/> struct from scratch.
    /// </summary>
    /// <param name="color">The text <see cref="Color"/> for the configuration.</param>
    /// <param name="asset">The <see cref="TMP_SpriteAsset"/> icon for the configuration.</param>
    public ModifierUiConfiguration(Color color, TMP_SpriteAsset asset = null!)
    {
        PopUpIconTmp = asset;
        UiColor = color;
    }

    /// <summary>
    /// Gets or sets the <see cref="TMP_SpriteAsset"/> for the icon used in ui.
    /// </summary>
    public TMP_SpriteAsset PopUpIconTmp { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Color"/> for the modifier, used in the wiki and other ui.
    /// </summary>
    public Color UiColor { get; set; }
}

public enum ModifierFaction
{
    Alliance,
    Universal,
    Crewmate,
    Neutral,
    Impostor,
    CrewmateAlliance,
    CrewmateUtility,
    CrewmateVisibility,
    CrewmatePostmortem,
    CrewmatePassive,
    NeutralAlliance,
    NeutralUtility,
    NeutralVisibility,
    NeutralPostmortem,
    NeutralPassive,
    ImpostorAlliance,
    ImpostorUtility,
    ImpostorVisibility,
    ImpostorPostmortem,
    ImpostorPassive,
    UniversalUtility,
    UniversalVisibility,
    UniversalPostmortem,
    UniversalPassive,
    AssailantUtility,
    AssailantVisibility,
    AssailantPostmortem,
    AssailantPassive,
    NonCrewmate,
    NonCrewUtility,
    NonCrewVisibility,
    NonCrewPostmortem,
    NonCrewPassive,
    NonNeutral,
    NonNeutUtility,
    NonNeutVisibility,
    NonNeutPostmortem,
    NonNeutPassive,
    NonImpostor,
    NonImpUtility,
    NonImpVisibility,
    NonImpPostmortem,
    NonImpPassive,
    HiderUtility,
    HiderVisibility,
    HiderPostmortem,
    HiderPassive,
    SeekerUtility,
    SeekerVisibility,
    SeekerPostmortem,
    SeekerPassive,
    External,
    Other
}