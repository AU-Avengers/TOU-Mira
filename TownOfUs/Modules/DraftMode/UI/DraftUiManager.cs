using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;


namespace TownOfUs.Modules.DraftMode
{
    public static class DraftUiManager
    {
        public static List<DraftRoleCard> BuildCards(List<ushort> roleIds, List<string> roleNames = null!)
        {
            var cards = new List<DraftRoleCard>();
            for (int i = 0; i < roleIds.Count; i++)
            {
                ushort id = roleIds[i];
                var role = ResolveRole(id);
                string fallbackName = roleNames != null && i < roleNames.Count ? roleNames[i] : string.Empty;

                string displayName;
                string team;
                Sprite icon;
                Color color;
                DraftFaction faction;
                string description;

                if (role)
                {
                    displayName = role.GetRoleName();
                    team = MiscUtils.GetParsedRoleAlignment(role!);
                    icon = role.GetRoleIcon();
                    color = role.TeamColor;
                    faction = GetDraftFaction(role);
                    description = GetRoleDescription(role);
                }
                else if (!string.IsNullOrWhiteSpace(fallbackName))
                {
                    displayName = fallbackName;
                    team = GetTeamLabelForRoleName(fallbackName);
                    icon = TouRoleIcons.RandomAny.LoadAsset();
                    color = GetColorForRoleName(fallbackName);
                    faction = GetDraftFactionForRoleName(fallbackName);
                    description = string.Empty;
                }
                else
                {
                    displayName = MiraLocaleManager.Get("TouDraftUnknownRoleLabel", "Role <id>").Replace("<id>", id.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    team = MiraLocaleManager.Get("TouDraftUnknownTeamLabel", "Unknown");
                    icon = TouRoleIcons.RandomAny.LoadAsset();
                    color = Color.white;
                    faction = DraftFaction.Other;
                    description = string.Empty;
                }

                cards.Add(new DraftRoleCard(displayName, team, icon, color, i, faction, description));
            }

            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts.ShowRandomOption)
                cards.Add(new DraftRoleCard(
                    MiraLocaleManager.Get("Random"), MiraLocaleManager.Get("Random"),
                    TouRoleIcons.RandomAny.LoadAsset(),
                    Color.white,
                    roleIds.Count,
                    DraftFaction.Other,
                    MiraLocaleManager.Get("TouDraftRandomDescription", "Locks in a completely random role for you.")));
            return cards;
        }

        private static string GetTeamLabelForRoleName(string roleName)
        {
            if (DraftRolePool.IsImpostorRoleName(roleName)) return MiraLocaleManager.Get("MiraApi.RoleTeam.Impostor");
            if (DraftRolePool.IsNeutralRoleName(roleName)) return MiraLocaleManager.Get("MiraApi.RoleTeam.Neutral");
            return MiraLocaleManager.Get("MiraApi.RoleTeam.Crewmate");
        }

        private static Color GetColorForRoleName(string roleName)
        {
            if (DraftRolePool.IsImpostorRoleName(roleName)) return TownOfUsColors.ImpSoft;
            if (DraftRolePool.IsNeutralRoleName(roleName)) return TownOfUsColors.Neutral;
            return TownOfUsColors.Crewmate;
        }

        private static DraftFaction GetDraftFactionForRoleName(string roleName)
        {
            if (DraftRolePool.IsImpostorRoleName(roleName)) return DraftFaction.Impostor;
            if (DraftRolePool.IsNeutralRoleName(roleName)) return DraftFaction.Neutral;
            return DraftFaction.Crewmate;
        }

        public static string GetRoleDescription(RoleBehaviour role)
        {
            if (!role) return string.Empty;
            try
            {
                string s = role.BlurbLong;
                if (string.IsNullOrWhiteSpace(s)) s = role.Blurb;
                return s ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        public static RoleBehaviour ResolveRole(ushort roleId)
        {
            if (roleId == (ushort)RoleTypes.Crewmate || roleId == (ushort)RoleTypes.Impostor)
                return null!;

            try
            {
                return MiscUtils.GetRegisteredRole((RoleTypes)roleId)!;
            }
            catch
            {
                return null!;
            }
        }

        public static DraftFaction GetDraftFaction(RoleBehaviour role)
        {
            if (role)
            {
                if (role.IsCrewmate())
                {
                    return DraftFaction.Crewmate;
                }
                if (role.IsNeutral())
                {
                    return DraftFaction.Neutral;
                }
                if (role.IsImpostor())
                {
                    return DraftFaction.Impostor;
                }
            }
            return DraftFaction.Other;
        }

        public static string GetTeamLabel(RoleBehaviour role)
        {
            var faction = MiraLocaleManager.Get("MiraApi.RoleTeam.Crewmate");
            if (role)
            {
                if (role!.IsNeutral())
                {
                    faction = MiraLocaleManager.Get("MiraApi.RoleTeam.Neutral");
                }
                else if (role!.IsImpostor())
                {
                    faction = MiraLocaleManager.Get("MiraApi.RoleTeam.Impostor");
                }
            }

            return faction;
        }

    }
}