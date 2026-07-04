using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Modules.DraftMode
{
    public static class DraftUiManager
    {
        public static List<DraftRoleCard> BuildCards(List<ushort> roleIds)
        {
            var cards = new List<DraftRoleCard>();
            var offered = OptionGroupSingleton<RoleOptions>.Instance?.OfferedRolesCount.Value ?? 0;
            int count = System.Math.Min(roleIds.Count, (int)offered);
            for (int i = 0; i < count; i++)
            {
                ushort id   = roleIds[i];
                var    role = ResolveRole(id);

                string displayName = role?.NiceName          ?? $"Role {id}";
                string team        = GetTeamLabel(role)       ?? "Unknown";
                Sprite icon        = GetRoleIcon(role);
                Color  color       = GetRoleColor(role);

                cards.Add(new DraftRoleCard(displayName, team, icon, color, i, GetRoleDescription(role)));
            }

            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts == null || roleOpts.ShowRandomOption)
                cards.Add(new DraftRoleCard(
                    "Random", "Random",
                    TouRoleIcons.RandomAny.LoadAsset(),
                    Color.white,
                    roleIds.Count, "Locks in a completely random role for you."));
            return cards;
        }

        public static string GetRoleDescription(RoleBehaviour role)
        {
            if (role == null) return string.Empty;
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
            try
            {
                return MiscUtils.GetRegisteredRole((RoleTypes)roleId) ?? RoleManager.Instance?.GetRole((RoleTypes)roleId);
            }
            catch { return null; }
        }

        public static string GetTeamLabel(RoleBehaviour role)
        {
            if (role == null) return "Unknown";
            try { return MiscUtils.GetParsedRoleAlignment(role); } catch { }
            var factionName = GetRoleFactionName(role);
            if (factionName != null && factionName.Contains("Impostor", System.StringComparison.OrdinalIgnoreCase))
                return "Impostor";
            if (factionName != null && factionName.Contains("Neutral", System.StringComparison.OrdinalIgnoreCase))
                return "Neutral";
            return "Crewmate";
        }
         public static string GetBroadFaction(RoleBehaviour role)
        {
            if (role == null) return "Crewmate";
            string alignment = null;
            try { alignment = MiscUtils.GetParsedRoleAlignment(role); } catch { }
            alignment ??= GetRoleFactionName(role);

            if (alignment != null && alignment.Contains("Impostor", System.StringComparison.OrdinalIgnoreCase))
                return "Impostor";
            if (alignment != null && alignment.Contains("Neutral", System.StringComparison.OrdinalIgnoreCase))
                return "Neutral";
            return "Crewmate";
        }

        public static string GetRoleFactionName(RoleBehaviour role)
        {
            if (role == null) return null;
            try
            {
                return role.GetType().Name;
            }
            catch { return null; }
        }

        public static Sprite GetRoleIcon(RoleBehaviour role)
        {
            if (role is ICustomRole cr && cr.Configuration.Icon != null)
            {
                try { return cr.Configuration.Icon.LoadAsset(); } catch { }
            }
            if (role?.RoleIconSolid != null) return role.RoleIconSolid;
            return TouRoleIcons.RandomAny.LoadAsset();
        }

        public static Color GetRoleColor(RoleBehaviour role)
        {
            if (role is ICustomRole cr) return cr.RoleColor;
            if (role != null)           return role.TeamColor;
            return Color.white;
        }

    }
}
