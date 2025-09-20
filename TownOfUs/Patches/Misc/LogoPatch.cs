using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using Reactor.Localization.Utilities;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class LogoPatch
{
    public static void Postfix()
    {
        RoleManager.Instance.GetRole(RoleTypes.CrewmateGhost).StringName =
            CustomStringName.CreateAndRegister("Crewmate Ghost");
        RoleManager.Instance.GetRole(RoleTypes.ImpostorGhost).StringName =
            CustomStringName.CreateAndRegister("Impostor Ghost");

        var roles = MiscUtils.AllRoles.Where(x =>
            x is not IWikiDiscoverable || x is ICustomRole custom && !custom.Configuration.HideSettings);

        if (roles.Any())
        {
            foreach (var role in roles)
            {
                SoftWikiEntries.RegisterRoleEntry(role);
            }
        }

        List<RoleBehaviour> vanillaRoles = new()
        {
            // RoleManager.Instance.GetRole(RoleTypes.Crewmate),
            RoleManager.Instance.GetRole(RoleTypes.Scientist),
            RoleManager.Instance.GetRole(RoleTypes.Noisemaker),
            RoleManager.Instance.GetRole(RoleTypes.Engineer),
            RoleManager.Instance.GetRole(RoleTypes.Tracker),
            RoleManager.Instance.GetRole(RoleTypes.GuardianAngel),
            RoleManager.Instance.GetRole(RoleTypes.Detective),
            // RoleManager.Instance.GetRole(RoleTypes.Impostor),
            RoleManager.Instance.GetRole(RoleTypes.Shapeshifter),
            RoleManager.Instance.GetRole(RoleTypes.Phantom),
            RoleManager.Instance.GetRole(RoleTypes.Viper),
        };
        foreach (var role in vanillaRoles)
        {
            SoftWikiEntries.RegisterVanillaRoleEntry(role);
        }

        var newLogo = GameObject.Find("LOGO-AU");
        var sizer = GameObject.Find("Sizer");
        if (newLogo != null)
        {
            newLogo.GetComponent<SpriteRenderer>().sprite = TouAssets.Banner.LoadAsset();
        }

        if (sizer != null)
        {
            sizer.GetComponent<AspectSize>().PercentWidth = 0.3f;
        }

        var menuBg = GameObject.Find("BackgroundTexture");

        if (menuBg != null)
        {
            var render = menuBg.GetComponent<SpriteRenderer>();
            render.flipY = true;
            render.color = new Color(1f, 1f, 1f, 0.65f);
        }
        var tint = GameObject.Find("MainUI").transform.GetChild(0).gameObject;
        if (tint != null)
        {
            tint.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.1f);
            tint.transform.localScale = new Vector3(7.5f, 7.5f, 1f);
        }
    }
}