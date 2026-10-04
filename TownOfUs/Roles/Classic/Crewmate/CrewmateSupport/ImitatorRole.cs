using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TMPro;
using TownOfUs.Modifiers.Crewmate;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Roles.Crewmate;

public sealed class ImitatorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Perception;
    public string IdPart => "Imitator";

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public bool CanShowSecondTab => true;

    [HideFromIl2Cpp]
    public List<AdvancedWikiAbilityDescription> WikiAbilities
    {
        get
        {
            var variantRoles = MiscUtils.AllRoles.Where(x => x is ICrewVariant).OrderBy(x => x.GetRoleName()).ToList();
            var neutEquivalents = new Dictionary<RoleBehaviour, RoleBehaviour>();
            var impEquivalents = new Dictionary<RoleBehaviour, RoleBehaviour>();
            foreach (var role in variantRoles)
            {
                var crewVariant = role as ICrewVariant;
                if (role.IsNeutral())
                {
                    neutEquivalents.Add(role, crewVariant!.CrewVariant);
                }
                else
                {
                    impEquivalents.Add(role, crewVariant!.CrewVariant);
                }
            }
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}NeutralCounterparts"),
                    MiraLocaleManager.Get("TownOfUsMira.Role.Imitator.CounterpartHint"),
                    GetRoleEquivalents(neutEquivalents),
                    TouRoleIcons.Neutral),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}ImpostorCounterparts"),
                    MiraLocaleManager.Get("TownOfUsMira.Role.Imitator.CounterpartHint"),
                    GetRoleEquivalents(impEquivalents),
                    TouRoleIcons.Impostor)
            ];
        }
    }

    public string RoleWikiDescription => GetAdvancedDescription() +
                                         "\n\n" + MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}CrewmateImitation.WikiDescription");
    GameObject ICustomRole.GetAdvancedWiki(MatchInfoGuide guide, TextMeshPro titleText, Scroller parent)
    {
        parent.ScrollToTop();
        var custom = this as ICustomRole;
        var obj = Helpers.CreateAdvancedWikiTab(
            guide,
            custom.RoleNameLocale,
            custom.RoleName + $" ({custom.RoleFactionTitle})",
            custom.RoleWikiDescription,
            titleText,
            out var desc);
        var num = 0;
        var grid = Instantiate(parent.Inner, obj.transform);
        var layoutGroup = grid.GetComponent<GridLayoutGroup>();
        layoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.spacing = new Vector2(0.75f, 0.4f);
        grid.DestroyChildren();
        var maxTextSize = 0f;
        foreach (var ability in WikiAbilities)
        {
            num++;
            var panel = Instantiate(
                guide.MatchInfoRolePanelPrefab,
                grid);
            panel.roleCount.text = ability.AbilityType;
            panel.roleCount.transform.localPosition += new Vector3(-0.04f, 0);
            panel.roleName.text = ability.Name;
            panel.roleName.transform.localPosition += new Vector3(-0.04f, 0);
            panel.roleDescription.text = ability.Description;
            panel.roleDescription.rectTransform.sizeDelta = new Vector2(2.601f, 0.8f);
            panel.roleDescription.transform.localPosition += new Vector3(0, -0.1f);
            panel.roleDescription.alignment = TextAlignmentOptions.Top;
            panel.roleDescription.fontSizeMin = 1.5f;
            panel.roleDescription.ForceMeshUpdate();
            if (maxTextSize < panel.roleDescription.textBounds.size.y)
            {
                maxTextSize = panel.roleDescription.textBounds.size.y;
            }
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

        obj.transform.SetParent(parent.Inner.transform);
        obj.transform.localPosition = new Vector3(0f, 0f, 0f);
        parent.SetYBoundsMax(Mathf.Clamp((desc.textBounds.size.y - 2) + maxTextSize + 0.475f, 0f, 999f));
        return obj;
    }

    public float ShowAbilitiesTab(Transform abilityTemplate, Transform abilityTemplateLong, Transform abilityScroller)
    {
        var listOfAbilities = new List<GameObject>();
            var newAbility = Instantiate(abilityTemplate, abilityScroller);
            var icon = newAbility.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
            var text = newAbility.GetChild(1).GetComponent<TextMeshPro>();
            var desc = newAbility.GetChild(2).GetComponent<TextMeshPro>();

            icon.sprite = TouCrewAssets.InspectSprite.LoadAsset();
            icon.size = new Vector2(0.8f, 0.8f * icon.sprite.bounds.size.y / icon.sprite.bounds.size.x);
            // icon.tileMode = SpriteTileMode.Adaptive;

            text.text =
                $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}CrewmateImitation")}</font>";
            desc.text =
                $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}CrewmateImitation.WikiDescription")}</font>";
            newAbility.gameObject.SetActive(true);
            listOfAbilities.Add(newAbility.gameObject);
            var variantRoles = MiscUtils.AllRoles.Where(x => x is ICrewVariant).OrderBy(x => x.GetRoleName()).ToList();
            var neutEquivalents = new Dictionary<RoleBehaviour, RoleBehaviour>();
            var impEquivalents = new Dictionary<RoleBehaviour, RoleBehaviour>();
            foreach (var role in variantRoles)
            {
                var crewVariant = role as ICrewVariant;
                if (role.IsNeutral())
                {
                    neutEquivalents.Add(role, crewVariant!.CrewVariant);
                }
                else
                {
                    impEquivalents.Add(role, crewVariant!.CrewVariant);
                }
            }

            var newSubObject = new GameObject("NewContainer");
            newSubObject.layer = newAbility.gameObject.layer;
            newSubObject.transform.SetParent(abilityScroller);
            var newTransform = newSubObject.AddComponent<RectTransform>();
            newTransform.offsetMax = new Vector2(5.5f, -2.4f);
            newTransform.offsetMin = new Vector2(3f, 0f);
            newTransform.transform.localPosition = new Vector3(0, 0f, -10f);
            listOfAbilities.Add(AddTabInfo(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}NeutralCounterparts"), neutEquivalents, abilityTemplateLong, 1.025f, newSubObject.transform));
            listOfAbilities.Add(AddTabInfo(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}ImpostorCounterparts"), impEquivalents, abilityTemplateLong, -1.025f, newSubObject.transform));

        return Mathf.Max(0f, 3.4f);
    }

    public static GameObject AddTabInfo(string title, Dictionary<RoleBehaviour, RoleBehaviour> equivalentRoles, Transform abilityTemplate, float xOffset, Transform abilityScroller)
    {
        var newAbility = Instantiate(abilityTemplate, abilityScroller);
        newAbility.localPosition = new Vector3(xOffset, -3.44f, 0f);
        var text = newAbility.GetChild(0).GetComponent<TextMeshPro>();
        var desc = newAbility.GetChild(1).GetComponent<TextMeshPro>();

        text.text =
            $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{title}</font>";
        var description = new StringBuilder();
        foreach (var rolePair in equivalentRoles)
        {
            var ogRole = rolePair.Key;
            var newRole = rolePair.Value;
            description.AppendLine(TownOfUsPlugin.Culture,
                $"{ogRole.GetRoleName()} ⇨ {newRole.GetRoleName()}");
        }
        desc.text =
            $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{description}</font>";
        newAbility.gameObject.SetActive(true);
        return newAbility.gameObject;
    }

    public static string GetRoleEquivalents(Dictionary<RoleBehaviour, RoleBehaviour> equivalentRoles)
    {
        var description = new StringBuilder();
        foreach (var rolePair in equivalentRoles)
        {
            var ogRole = rolePair.Key;
            var newRole = rolePair.Value;
            description.AppendLine(TownOfUsPlugin.Culture,
                $"{MiscUtils.GetMaskedRoleTmpIcon(ogRole)}{ogRole.GetRoleName()} ⇨ {newRole.GetRoleName()} {MiscUtils.GetMaskedRoleTmpIcon(newRole)}");
        }
        return description.ToString();
    }

    public Color RoleColor => TownOfUsColors.Imitator;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Imitator.LoadAsset(), "TouMira.Role.Crewmate.Imitator", 1.45f),
        Icon = TouRoleIcons.Imitator,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.SpyIntroSound
    };



    public string SecondTabName => MiraLocaleManager.Get("WikiRoleGuideTab", "Role Guide");

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        if (!player.HasModifier<ImitatorCacheModifier>())
        {
            player.AddModifier<ImitatorCacheModifier>();
        }
    }
}