using UnityEngine;

namespace TownOfUs.Modules.DraftMode
{
    public sealed class DraftRoleCard
    {
        public string  RoleName    { get; }
        public string  TeamName    { get; }
        public Sprite  Icon        { get; }
        public Color   Color       { get; }
        public int     Index       { get; }
        public ushort  RoleId      { get; }
        public string  Description { get; }

        public DraftRoleCard(string roleName, string teamName, Sprite icon, Color color, int index, ushort roleId = 0, string description = "")
        {
            RoleName    = roleName;
            TeamName    = teamName;
            Icon        = icon;
            Color       = color;
            Index       = index;
            RoleId      = roleId;
            Description = description;
        }
    }
}
