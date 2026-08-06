using AmongUs.GameOptions;

namespace TownOfUs.Modules.DraftMode
{
    public static class DraftExclusiveImpostorRoles
    {
        private static readonly HashSet<RoleTypes> RegisteredRoles = new();
        private static bool _defaultsRegistered;

        public static void Register(RoleTypes roleType) => RegisteredRoles.Add(roleType);

        public static void Register(ushort roleId) => RegisteredRoles.Add((RoleTypes)roleId);

        public static void Register(RoleBehaviour role)
        {
            if (role != null) RegisteredRoles.Add(role.Role);
        }

        public static bool IsRegistered(RoleTypes roleType)
        {
            EnsureDefaults();
            return RegisteredRoles.Contains(roleType);
        }

        public static bool IsRegistered(ushort roleId) => IsRegistered((RoleTypes)roleId);

        public static IReadOnlyCollection<RoleTypes> All => RegisteredRoles;

        private static void EnsureDefaults()
        {
            if (_defaultsRegistered) return;
            _defaultsRegistered = true;

            var recruiter = MiscUtils.AllRoles.FirstOrDefault(r =>
                r != null && r.GetType().Name.Contains("Recruiter", System.StringComparison.OrdinalIgnoreCase));
            if (recruiter != null) Register(recruiter);
        }
    }
}
