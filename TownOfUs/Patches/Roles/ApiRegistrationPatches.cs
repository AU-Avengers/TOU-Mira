using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Translation;
using Reactor.Localization.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Roles;

namespace TownOfUs.Patches.Roles;

[HarmonyPatch(typeof(MainMenuManager))]
[HarmonyAfter(nameof(MiraAPI.Patches.Roles.GameStartupPatch))]
public static class ApiRegistrationPatches
{
    private static bool _runOnce;

    /// <summary>
    /// This is used for registering roles when the game opens, might be a janky solution, but it works.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MainMenuManager.Start))]
    public static void StartPostfix()
    {
        if (_runOnce)
        {
            return;
        }

        _runOnce = true;

        var newList = CustomRoleManager.CustomRoleBehaviours.Where(x =>
            !MiscUtils.IsBasicGhost(x.Role)).ToList();
        MiscUtils.AllRoles = newList;
        var touList = MiscUtils.AllRoles.OfType<ITownOfUsRole>().ToList();
        MiscUtils.AllTouRoles = touList;
        foreach (var role in MiscUtils.AllTouRoles)
        {
            role.InitialSetup();
        }

        var newList2 = RoleManager.Instance.AllRoles.ToArray().Where(x =>
            !MiscUtils.IsBasicGhost(x.Role)).ToList();
        MiscUtils.AllInGameRoles = newList2;
        var newModifiers = new List<TouBaseGameModifier>();
        var assignableMods = new List<IAssignableTargets>();
        MiscUtils.AllModifiers = ModifierManager.Modifiers.ToList();
        foreach (var mod in MiscUtils.AllModifiers)
        {
            if (mod is TouBaseGameModifier touMod)
            {
                newModifiers.Add(touMod);
            }
            if (mod is IAssignableTargets assignMod)
            {
                assignableMods.Add(assignMod);
            }
        }
        MiscUtils.AllTouWikiModifiers = ModifierManager.Modifiers.Where(x => x is IWikiDiscoverable).ToList();
        MiscUtils.AllOverallWikiModifiers = MiscUtils.AllTouWikiModifiers;
        MiscUtils.AllBaseGameModifiers = newModifiers;
        MiscUtils.AssignableTargetModifiers = assignableMods;

        RoleManager.Instance.GetRole(RoleTypes.CrewmateGhost).StringName =
            MiraLocaleManager.GetOrCreateLocaleString("Crewmate Ghost");
        RoleManager.Instance.GetRole(RoleTypes.ImpostorGhost).StringName =
            MiraLocaleManager.GetOrCreateLocaleString("Impostor Ghost");

        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Neutral.LoadAsset(), "AmongUs.Role.Custom",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Neutral.LoadAsset(), "AmongUs.Role.Neutral",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Crewmate.LoadAsset(), "AmongUs.Role.Crewmate",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Impostor.LoadAsset(), "AmongUs.Role.Impostor",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Scientist.LoadAsset(), "AmongUs.Role.Scientist",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Engineer.LoadAsset(), "AmongUs.Role.Engineer",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.GuardianAngel.LoadAsset(), "AmongUs.Role.GuardianAngel",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Shapeshifter.LoadAsset(), "AmongUs.Role.Shapeshifter",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Crewmate.LoadAsset(), "AmongUs.Role.CrewmateGhost",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Impostor.LoadAsset(), "AmongUs.Role.ImpostorGhost",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Noisemaker.LoadAsset(), "AmongUs.Role.Noisemaker",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Phantom.LoadAsset(), "AmongUs.Role.Phantom",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Tracker.LoadAsset(), "AmongUs.Role.Tracker",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Detective.LoadAsset(), "AmongUs.Role.Detective",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Viper.LoadAsset(), "AmongUs.Role.Viper",
            1.45f);
        TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Prosecutor.LoadAsset(), "AmongUs.Role.Judge",
            1.45f);
    }
}