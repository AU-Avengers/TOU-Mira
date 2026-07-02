using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Modules.DraftMode
{
    public static class DraftUiManager
    {
        public static void ShowPicker(List<ushort> roleIds)
        {
            if (HudManager.Instance == null || roleIds == null || roleIds.Count == 0) return;
            DraftStatusOverlay.SetState(OverlayState.BackgroundOnly);
            DraftScreenController.Show(roleIds.ToArray());
        }

        public static void RefreshTurnList()
        {
            DraftStatusOverlay.Refresh();
        }

        public static void CloseAll()
        {
            DraftScreenController.Hide();

            if (DraftManager.IsDraftActive)
                DraftStatusOverlay.SetState(OverlayState.Waiting);
        }

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

                cards.Add(new DraftRoleCard(displayName, team, icon, color, i, id, GetRoleDescription(role)));
            }

            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts == null || roleOpts.ShowRandomOption)
                cards.Add(new DraftRoleCard(
                    "Random", "Random",
                    TouRoleIcons.RandomAny.LoadAsset(),
                    Color.white,
                    roleIds.Count, 0, "Locks in a completely random role for you."));
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

        private static string GetRoleFactionName(RoleBehaviour role)
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

        public static string Normalize(string s) =>
            (s ?? string.Empty).Replace(" ", "").Replace("-", "");
    }
}