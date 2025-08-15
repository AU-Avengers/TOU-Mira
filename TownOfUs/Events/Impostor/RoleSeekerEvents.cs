using MiraAPI.Events;
using MiraAPI.Modifiers;
using MiraAPI.Events.Vanilla.Gameplay;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Impostor;
using Il2CppInterop.Runtime.Attributes;
using TownOfUs.Utilities;
using TownOfUs.Options.Modifiers.Impostor;
using MiraAPI.GameOptions;

namespace TownOfUs.Events.Modifiers
{
    public static class RoleSeekerEvents
    {
        public static float InGameReveal = OptionGroupSingleton<RoleSeekerOptions>.Instance.InGameReveal;
        public static float NotInGameReveal = OptionGroupSingleton<RoleSeekerOptions>.Instance.NotInGameReveal;

        private static readonly IReadOnlyList<string> ImpostorRoles = new List<string>
        {
            "Ambassador", "Ambusher", "Blackmailer", "Bomber", "Eclipsal", "Escapist",
            "Grenadier", "Hypnotist", "Janitor", "Miner", "Morphling", "Scavenger",
            "Swooper", "Traitor", "Undertaker", "Venerer", "Warlock"
        };

        private static readonly IReadOnlyList<string> PossibleRoles = new List<string>
        {
            "Altruist", "Ambassador", "Ambusher", "Amnesiac", "Arsonist", "Aurial", "Blackmailer", "Bomber",
            "Cleric", "Deputy", "Detective", "Doomsayer", "Eclipsal", "Engineer", "Escapist", "Executioner",
            "Glitch", "Grenadier", "Guardian Angel", "Haunter", "Hunter", "Hypnotist", "Imitator", "Inquisitor",
            "Investigator", "Jailor", "Janitor", "Jester", "Juggernaut", "Lookout", "Mayor", "Medic", "Medium",
            "Mercenary", "Miner", "Mirrorcaster", "Morphling", "Mystic", "Oracle", "Pestilence", "Phantom",
            "Plaguebearer", "Plumber", "Politician", "Prosecutor", "Scavenger", "Seer", "Sheriff", "Snitch",
            "Soul Collector", "Spy", "Survivor", "Swapper", "Swooper", "Tracker", "Traitor", "Trapper",
            "Transporter", "Undertaker", "Vampire", "Venerer", "Veteran", "Vigilante", "Warlock", "Warden",
            "Werewolf"
        };

        [HideFromIl2Cpp] 
        public static List<RoleBehaviour> PotentiallyRevealedRoles { get; set; } = new();

        [HideFromIl2Cpp] 
        private static Dictionary<string, int> InGameRoleCounts { get; set; } = new();

        [HideFromIl2Cpp] 
        private static Dictionary<string, int> RevealedInGameRoles { get; set; } = new();

        [HideFromIl2Cpp] 
        private static HashSet<string> RevealedNotInGameRoles { get; set; } = new();

        [RegisterEvent]
        public static void GameStartEventHandler(RoundStartEvent @event)
        {
            if (!@event.TriggeredByIntro)
                return;

            if (!PlayerControl.LocalPlayer.HasModifier<RoleSeekerModifier>())
                return;

            PotentiallyRevealedRoles.Clear();
            InGameRoleCounts.Clear();
            RevealedInGameRoles.Clear();
            RevealedNotInGameRoles.Clear();

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data?.Role != null)
                {
                    PotentiallyRevealedRoles.Add(player.Data.Role);
                    var roleName = player.Data.Role.NiceName;
                    if (!InGameRoleCounts.ContainsKey(roleName))
                        InGameRoleCounts[roleName] = 0;
                    InGameRoleCounts[roleName]++;
                }
            }
        }

        [RegisterEvent]
        public static void AfterMurderEventHandler(AfterMurderEvent @event)
        {
            var source = @event.Source;

            if (source.GetModifier<RoleSeekerModifier>() == null)
                return;

            if (source.TryGetModifier<AllianceGameModifier>(out var allyMod) && !allyMod.GetsPunished)
                return;

            if (PotentiallyRevealedRoles.Count <= 0 || !source.HasModifier<RoleSeekerModifier>())
                return;

            float roll = UnityEngine.Random.value;
            string? roleName = null;
            bool? inGame = null;

            var inGameRoleNames = PotentiallyRevealedRoles
                .Select(r => r.NiceName)
                .Where(r => !ImpostorRoles.Contains(r))
                .ToList();

            if (roll < InGameReveal / 100f && inGameRoleNames.Count > 0)
            {
                var availableRoles = inGameRoleNames
                    .Where(r => !RevealedInGameRoles.ContainsKey(r) || RevealedInGameRoles[r] < InGameRoleCounts[r])
                    .ToList();

                if (availableRoles.Count > 0)
                {
                    roleName = availableRoles[UnityEngine.Random.Range(0, availableRoles.Count)];
                    inGame = true;
                    if (!RevealedInGameRoles.ContainsKey(roleName))
                        RevealedInGameRoles[roleName] = 0;
                    RevealedInGameRoles[roleName]++;
                }
            }
            else
            {
                var notInGameRoles = PossibleRoles
                    .Where(r => !inGameRoleNames.Contains(r) && !ImpostorRoles.Contains(r) && !RevealedNotInGameRoles.Contains(r))
                    .ToList();

                if (notInGameRoles.Count > 0)
                {
                    roleName = notInGameRoles[UnityEngine.Random.Range(0, notInGameRoles.Count)];
                    inGame = false;
                    RevealedNotInGameRoles.Add(roleName);
                }
            }

            if (roleName != null && inGame.HasValue)
            {
                var prefix = roleName.StartsWithVowel() ? "an" : "a";
                var title = "<color=#D64042>Role Seeker Info</color>";
                string message = inGame.Value
                    ? $"<color=#FFFFFF>You have revealed that {prefix} {roleName} is in this game!</color>"
                    : $"<color=#FFFFFF>You have revealed that {prefix} {roleName} is not in this game!</color>";

                MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, message, false, true);
            }
        }
    }
}