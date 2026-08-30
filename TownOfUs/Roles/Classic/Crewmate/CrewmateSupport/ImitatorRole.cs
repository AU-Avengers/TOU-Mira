using System.Text;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TMPro;
using TownOfUs.Modifiers.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class ImitatorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Perception;
    public string IdPart => "Imitator";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public bool CanShowSecondTab => true;

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