using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modules.Wiki;

public sealed class SoftWikiInfo(Type type)
{
    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; set; } = [];
    public Type EntryType => type;
    public Sprite Icon { get; set; }
    public string EntryName { get; set; } = "Unknown";
    public string TeamName { get; set; } = "Unknown";
    public Color EntryColor { get; set; } = Color.red;
    public string SecondTabName { get; set; } = "Abilities";
    public bool IsHiddenFromList { get; set; }
    public uint FakeTypeId => ModifierManager.GetModifierTypeId(GetType()) ?? throw new InvalidOperationException("Modifier is not registered.");
    public string GetAdvancedDescription {get; set; }
}

public static class SoftWikiEntries
{
    public static readonly Dictionary<RoleBehaviour, SoftWikiInfo> RoleEntries = [];
    public static readonly Dictionary<BaseModifier, SoftWikiInfo> ModifierEntries = [];
    
    public static void RegisterVanillaRoleEntry(RoleBehaviour role)
    {
        if (!RoleEntries.TryGetValue(role, out _))
        {
            RoleEntries.Add(role, new SoftWikiInfo(role.GetType()));
        }
        var roleEntry = RoleEntries.FirstOrDefault(x => x.Key.Role == role.Role);
        if (roleEntry.Key != null && roleEntry.Value != null)
        {
            var entry = roleEntry.Value;
            entry.EntryName = role.GetRoleName();
            var teamName = $"{role.GetRoleAlignment()}";

            entry.TeamName = teamName;
            entry.EntryColor = role.TeamColor;
            switch (role.Role)
            {
                default:
                    entry.GetAdvancedDescription = role.BlurbLong;
                    break;
            }

            var roleImg = TouRoleIcons.RandomAny.LoadAsset();

            if (role.RoleIconSolid != null)
            {
                roleImg = role.RoleIconSolid;
            }

            entry.Icon = roleImg;
        }
    }
    public static void RegisterRoleEntry(RoleBehaviour role)
    {
        if (!RoleEntries.TryGetValue(role, out _))
        {
            RoleEntries.Add(role, new SoftWikiInfo(role.GetType()));
        }
        var roleEntry = RoleEntries.FirstOrDefault(x => x.Key.Role == role.Role);
        if (roleEntry.Key != null && roleEntry.Value != null)
        {
            var entry = roleEntry.Value;
            entry.EntryName = role.GetRoleName();
            var teamName = $"{role.GetRoleAlignment()}";

            entry.TeamName = teamName;
            entry.EntryColor = role is ICustomRole miraRole2 ? miraRole2.RoleColor : role.TeamColor;
            entry.GetAdvancedDescription = $"{role.BlurbLong}{MiscUtils.AppendOptionsText(entry.EntryType)}";
            
            var roleImg = TouRoleIcons.RandomAny.LoadAsset();

            if (role is ICustomRole customRole)
            {
                if (customRole.Configuration.Icon != null)
                {
                    roleImg = customRole.Configuration.Icon.LoadAsset();
                }
                else if (customRole.Team == ModdedRoleTeams.Crewmate)
                {
                    roleImg = TouRoleIcons.RandomCrew.LoadAsset();
                }
                else if (customRole.Team == ModdedRoleTeams.Impostor)
                {
                    roleImg = TouRoleIcons.RandomImp.LoadAsset();
                }
                else
                {
                    roleImg = TouRoleIcons.RandomNeut.LoadAsset();
                }
            }
            else
            {
                if (role.RoleIconSolid != null)
                {
                    roleImg = role.RoleIconSolid;
                }
            }
            entry.Icon = roleImg;
        }
    }
    public static void RegisterModifierEntry(BaseModifier modifier)
    {
        if (!ModifierEntries.TryGetValue(modifier, out _))
        {
            ModifierEntries.Add(modifier, new SoftWikiInfo(modifier.GetType()));
        }
        var roleEntry = ModifierEntries.FirstOrDefault(x => x.Key == modifier);
        if (roleEntry.Key != null && roleEntry.Value != null)
        {
            var entry = roleEntry.Value;
            entry.EntryName = modifier.ModifierName;
            entry.TeamName = $"{modifier.GetModifierFaction()}";
            entry.EntryColor = Color.grey;
            entry.GetAdvancedDescription = $"{modifier.GetDescription()}{MiscUtils.AppendOptionsText(entry.EntryType)}";
            entry.Icon = modifier.ModifierIcon?.LoadAsset() ?? TouRoleIcons.RandomAny.LoadAsset();
        }
    }
}