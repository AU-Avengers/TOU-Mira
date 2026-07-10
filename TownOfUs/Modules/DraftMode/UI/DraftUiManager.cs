using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Modules.DraftMode
{
    public static class DraftUiManager
    {
        public static List<DraftRoleCard> BuildCards(List<ushort> roleIds)
        {
            var cards = new List<DraftRoleCard>();
            var offered = OptionGroupSingleton<RoleOptions>.Instance.OfferedRolesCount.Value;
            int count = System.Math.Min(roleIds.Count, (int)offered);
            for (int i = 0; i < count; i++)
            {
                ushort id   = roleIds[i];
                var    role = ResolveRole(id);

                string displayName = role ? role.GetRoleName() : $"Role {id}";
                string team        = role ? MiscUtils.GetParsedRoleAlignment(role) : "Unknown";
                Sprite icon        = role ? role.GetRoleIcon() : TouRoleIcons.RandomAny.LoadAsset();
                Color  color       = role ? role.TeamColor : Color.white;

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
            return RoleManager.Instance.GetRole((RoleTypes)roleId);
        }

        public static string GetTeamLabel(RoleBehaviour role)
        {
            var faction = TouLocale.Get("CrewmateKeyword");
            if (role.IsNeutral())
            {
                faction = TouLocale.Get("NeutralKeyword");
            }
            else if (role.IsImpostor())
            {
                faction = TouLocale.Get("ImpostorKeyword");
            }

            return faction;
        }

    }
}
