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
        
        var roles = MiscUtils.AllRoles.Where(x => x is not IWikiDiscoverable || x is ICustomRole custom && !custom.Configuration.HideSettings);
        // var modifiers = MiscUtils.AllModifiers.Where(x => x is GameModifier && x is not IWikiDiscoverable);

        if (roles.Any())
        {
            foreach (var role in roles)
            {
                SoftWikiEntries.RegisterRoleEntry(role);
            }
        }
        /*foreach (var modifier in modifiers)
        {
            SoftWikiEntries.RegisterModifierEntry(modifier);
        }*/
        

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
    }
}