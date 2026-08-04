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
            for (int i = 0; i < roleIds.Count; i++)
            {
                ushort id   = roleIds[i];
                var    role = ResolveRole(id);

                string displayName = role ? role.GetRoleName() : TouLocale.GetParsed("TouDraftUnknownRoleLabel", "Role <id>").Replace("<id>", id.ToString(System.Globalization.CultureInfo.InvariantCulture));
                string team        = role ? MiscUtils.GetParsedRoleAlignment(role!) : TouLocale.GetParsed("TouDraftUnknownTeamLabel", "Unknown");
                Sprite icon        = role ? role.GetRoleIcon() : TouRoleIcons.RandomAny.LoadAsset();
                Color  color       = role ? role.TeamColor : Color.white;

                cards.Add(new DraftRoleCard(displayName, team, icon, color, i, GetDraftFaction(role), GetRoleDescription(role)));
            }

            var roleOpts = OptionGroupSingleton<RoleOptions>.Instance;
            if (roleOpts.ShowRandomOption)
                cards.Add(new DraftRoleCard(
                    TouLocale.Get("Random"), TouLocale.Get("Random"),
                    TouRoleIcons.RandomAny.LoadAsset(),
                    Color.white,
                    roleIds.Count,
                    DraftFaction.Other,
                    TouLocale.GetParsed("TouDraftRandomDescription", "Locks in a completely random role for you.")));
            return cards;
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
            try
            {
                return MiscUtils.GetRegisteredRole((RoleTypes)roleId);
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
            var faction = TouLocale.Get("CrewmateKeyword");
            if (role)
            {
                if (role!.IsNeutral())
                {
                    faction = TouLocale.Get("NeutralKeyword");
                }
                else if (role!.IsImpostor())
                {
                    faction = TouLocale.Get("ImpostorKeyword");
                }
            }

            return faction;
        }

    }
}

