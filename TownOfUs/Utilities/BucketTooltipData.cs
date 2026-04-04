using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Utilities;

public static class BucketTooltipData
{
    public readonly struct RoleEntry
    {
        public readonly string DisplayName;
        public readonly string ClassFullName;
        public readonly Color Col;

        public RoleEntry(string display, string classFullName, Color col)
        {
            DisplayName   = display;
            ClassFullName = classFullName;
            Col           = col;
        }
    }

    public readonly struct TooltipInfo
    {
        public readonly RoleEntry[] Roles;
        public TooltipInfo(RoleEntry[] roles) { Roles = roles; }
    }

    // ── All possible roles per bucket ─────────────────────────────────────────

    private static readonly Dictionary<RoleListOption, RoleEntry[]> _allRoles = new()
    {
        [RoleListOption.CrewInvest] = new RoleEntry[]
        {
            new("Aurial",       "TownOfUs.Roles.Crewmate.AurialRole",       TownOfUsColors.Aurial),
            new("Forensic",     "TownOfUs.Roles.Crewmate.ForensicRole",     TownOfUsColors.Forensic),
            new("Haunter",      "TownOfUs.Roles.Crewmate.HaunterRole",      TownOfUsColors.Haunter),
            new("Investigator", "TownOfUs.Roles.Crewmate.InvestigatorRole", TownOfUsColors.Investigator),
            new("Lookout",      "TownOfUs.Roles.Crewmate.LookoutRole",      TownOfUsColors.Lookout),
            new("Medium",       "TownOfUs.Roles.Crewmate.MediumRole",       TownOfUsColors.Medium),
            new("Mystic",       "TownOfUs.Roles.Crewmate.MysticRole",       TownOfUsColors.Mystic),
            new("Seer",         "TownOfUs.Roles.Crewmate.SeerRole",         TownOfUsColors.Seer),
            new("Snitch",       "TownOfUs.Roles.Crewmate.SnitchRole",       TownOfUsColors.Snitch),
            new("Sonar",        "TownOfUs.Roles.Crewmate.SonarRole",        TownOfUsColors.Sonar),
            new("Spy",          "TownOfUs.Roles.Crewmate.SpyRole",          TownOfUsColors.Spy),
            new("Trapper",      "TownOfUs.Roles.Crewmate.TrapperRole",      TownOfUsColors.Trapper),
        },

        [RoleListOption.CrewKilling] = new RoleEntry[]
        {
            new("Deputy",    "TownOfUs.Roles.Crewmate.DeputyRole",    TownOfUsColors.Deputy),
            new("Hunter",    "TownOfUs.Roles.Crewmate.HunterRole",    TownOfUsColors.Hunter),
            new("Sheriff",   "TownOfUs.Roles.Crewmate.SheriffRole",   TownOfUsColors.Sheriff),
            new("Veteran",   "TownOfUs.Roles.Crewmate.VeteranRole",   TownOfUsColors.Veteran),
            new("Vigilante", "TownOfUs.Roles.Crewmate.VigilanteRole", TownOfUsColors.Vigilante),
        },

        [RoleListOption.CrewProtective] = new RoleEntry[]
        {
            new("Altruist",     "TownOfUs.Roles.Crewmate.AltruistRole",     TownOfUsColors.Altruist),
            new("Cleric",       "TownOfUs.Roles.Crewmate.ClericRole",       TownOfUsColors.Cleric),
            new("Medic",        "TownOfUs.Roles.Crewmate.MedicRole",        TownOfUsColors.Medic),
            new("Mirrorcaster", "TownOfUs.Roles.Crewmate.MirrorcasterRole", TownOfUsColors.Mirrorcaster),
            new("Oracle",       "TownOfUs.Roles.Crewmate.OracleRole",       TownOfUsColors.Oracle),
            new("Warden",       "TownOfUs.Roles.Crewmate.WardenRole",       TownOfUsColors.Warden),
        },

        [RoleListOption.CrewPower] = new RoleEntry[]
        {
            new("Jailor",     "TownOfUs.Roles.Crewmate.JailorRole",     TownOfUsColors.Jailor),
            new("Mayor",      "TownOfUs.Roles.Crewmate.MayorRole",      TownOfUsColors.Mayor),
            new("Monarch",    "TownOfUs.Roles.Crewmate.MonarchRole",    TownOfUsColors.Monarch),
            new("Politician", "TownOfUs.Roles.Crewmate.PoliticianRole", TownOfUsColors.Politician),
            new("Prosecutor", "TownOfUs.Roles.Crewmate.ProsecutorRole", TownOfUsColors.Prosecutor),
            new("Swapper",    "TownOfUs.Roles.Crewmate.SwapperRole",    TownOfUsColors.Swapper),
            new("Time Lord",  "TownOfUs.Roles.Crewmate.TimeLordRole",   TownOfUsColors.TimeLord),
        },

        [RoleListOption.CrewSupport] = new RoleEntry[]
        {
            new("Engineer",    "TownOfUs.Roles.Crewmate.EngineerTouRole",  TownOfUsColors.Engineer),
            new("Imitator",    "TownOfUs.Roles.Crewmate.ImitatorRole",     TownOfUsColors.Imitator),
            new("Plumber",     "TownOfUs.Roles.Crewmate.PlumberRole",      TownOfUsColors.Plumber),
            new("Sentry",      "TownOfUs.Roles.Crewmate.SentryRole",       TownOfUsColors.Sentry),
            new("Transporter", "TownOfUs.Roles.Crewmate.TransporterRole",  TownOfUsColors.Transporter),
        },

        [RoleListOption.NeutBenign] = new RoleEntry[]
        {
            new("Amnesiac",  "TownOfUs.Roles.Neutral.AmnesiacRole",  TownOfUsColors.Amnesiac),
            new("Fairy",     "TownOfUs.Roles.Neutral.FairyRole",     TownOfUsColors.Fairy),
            new("Mercenary", "TownOfUs.Roles.Neutral.MercenaryRole", TownOfUsColors.Mercenary),
            new("Survivor",  "TownOfUs.Roles.Neutral.SurvivorRole",  TownOfUsColors.Survivor),
        },

        [RoleListOption.NeutEvil] = new RoleEntry[]
        {
            new("Doomsayer",   "TownOfUs.Roles.Neutral.DoomsayerRole",   TownOfUsColors.Doomsayer),
            new("Executioner", "TownOfUs.Roles.Neutral.ExecutionerRole", TownOfUsColors.Executioner),
            new("Jester",      "TownOfUs.Roles.Neutral.JesterRole",      TownOfUsColors.Jester),
            new("Spectre",     "TownOfUs.Roles.Neutral.SpectreRole",     TownOfUsColors.Spectre),
        },

        [RoleListOption.NeutKilling] = new RoleEntry[]
        {
            new("Arsonist",       "TownOfUs.Roles.Neutral.ArsonistRole",     TownOfUsColors.Arsonist),
            new("Glitch",         "TownOfUs.Roles.Neutral.GlitchRole",       TownOfUsColors.Glitch),
            new("Juggernaut",     "TownOfUs.Roles.Neutral.JuggernautRole",   TownOfUsColors.Juggernaut),
            new("Plaguebearer",   "TownOfUs.Roles.Neutral.PlaguebearerRole", TownOfUsColors.Plaguebearer),
            new("Pestilence",     "TownOfUs.Roles.Neutral.PestilenceRole",   TownOfUsColors.Pestilence),
            new("Soul Collector", "TownOfUs.Roles.Neutral.SoulCollectorRole",TownOfUsColors.SoulCollector),
            new("Vampire",        "TownOfUs.Roles.Neutral.VampireRole",      TownOfUsColors.Vampire),
            new("Werewolf",       "TownOfUs.Roles.Neutral.WerewolfRole",     TownOfUsColors.Werewolf),
        },

        [RoleListOption.NeutOutlier] = new RoleEntry[]
        {
            new("Chef",       "TownOfUs.Roles.Neutral.ChefRole",      TownOfUsColors.Chef),
            new("Inquisitor", "TownOfUs.Roles.Neutral.InquisitorRole",TownOfUsColors.Inquisitor),
        },

        [RoleListOption.ImpConceal] = new RoleEntry[]
        {
            new("Eclipsal",  "TownOfUs.Roles.Impostor.EclipsalRole",  Palette.ImpostorRed),
            new("Escapist",  "TownOfUs.Roles.Impostor.EscapistRole",  Palette.ImpostorRed),
            new("Grenadier", "TownOfUs.Roles.Impostor.GrenadierRole", Palette.ImpostorRed),
            new("Morphling", "TownOfUs.Roles.Impostor.MorphlingRole", Palette.ImpostorRed),
            new("Swooper",   "TownOfUs.Roles.Impostor.SwooperRole",   Palette.ImpostorRed),
            new("Venerer",   "TownOfUs.Roles.Impostor.VenererRole",   Palette.ImpostorRed),
        },

        [RoleListOption.ImpKilling] = new RoleEntry[]
        {
            new("Ambusher",  "TownOfUs.Roles.Impostor.AmbusherRole",  Palette.ImpostorRed),
            new("Bomber",    "TownOfUs.Roles.Impostor.BomberRole",    Palette.ImpostorRed),
            new("Parasite",  "TownOfUs.Roles.Impostor.ParasiteRole",  Palette.ImpostorRed),
            new("Scavenger", "TownOfUs.Roles.Impostor.ScavengerRole", Palette.ImpostorRed),
            new("Warlock",   "TownOfUs.Roles.Impostor.WarlockRole",   Palette.ImpostorRed),
        },

        [RoleListOption.ImpPower] = new RoleEntry[]
        {
            new("Ambassador",   "TownOfUs.Roles.Impostor.AmbassadorRole",   Palette.ImpostorRed),
            new("Puppeteer",    "TownOfUs.Roles.Impostor.PuppeteerRole",    Palette.ImpostorRed),
            new("Spellslinger", "TownOfUs.Roles.Impostor.SpellslingerRole", Palette.ImpostorRed),
            new("Traitor",      "TownOfUs.Roles.Impostor.TraitorRole",      Palette.ImpostorRed),
        },

        [RoleListOption.ImpSupport] = new RoleEntry[]
        {
            new("Blackmailer", "TownOfUs.Roles.Impostor.BlackmailerRole", Palette.ImpostorRed),
            new("Hypnotist",   "TownOfUs.Roles.Impostor.HypnotistRole",   Palette.ImpostorRed),
            new("Janitor",     "TownOfUs.Roles.Impostor.JanitorRole",     Palette.ImpostorRed),
            new("Miner",       "TownOfUs.Roles.Impostor.MinerRole",       Palette.ImpostorRed),
            new("Undertaker",  "TownOfUs.Roles.Impostor.UndertakerRole",  Palette.ImpostorRed),
        },
    };

    // Group buckets map to multiple specific buckets
    private static readonly Dictionary<RoleListOption, RoleListOption[]> _groupBuckets = new()
    {
        [RoleListOption.CrewCommon]   = new[] { RoleListOption.CrewInvest, RoleListOption.CrewProtective, RoleListOption.CrewSupport },
        [RoleListOption.CrewSpecial]  = new[] { RoleListOption.CrewKilling, RoleListOption.CrewPower },
        [RoleListOption.CrewRandom]   = new[] { RoleListOption.CrewInvest, RoleListOption.CrewKilling, RoleListOption.CrewProtective, RoleListOption.CrewPower, RoleListOption.CrewSupport },
        [RoleListOption.NeutCommon]   = new[] { RoleListOption.NeutBenign, RoleListOption.NeutEvil },
        [RoleListOption.NeutSpecial]  = new[] { RoleListOption.NeutKilling, RoleListOption.NeutOutlier },
        [RoleListOption.NeutWildcard] = new[] { RoleListOption.NeutBenign, RoleListOption.NeutEvil, RoleListOption.NeutOutlier },
        [RoleListOption.NeutRandom]   = new[] { RoleListOption.NeutBenign, RoleListOption.NeutEvil, RoleListOption.NeutKilling, RoleListOption.NeutOutlier },
        [RoleListOption.ImpCommon]    = new[] { RoleListOption.ImpConceal, RoleListOption.ImpSupport },
        [RoleListOption.ImpSpecial]   = new[] { RoleListOption.ImpKilling, RoleListOption.ImpPower },
        [RoleListOption.ImpRandom]    = new[] { RoleListOption.ImpConceal, RoleListOption.ImpKilling, RoleListOption.ImpPower, RoleListOption.ImpSupport },
        [RoleListOption.NonImp]       = new[] { RoleListOption.CrewInvest, RoleListOption.CrewKilling, RoleListOption.CrewProtective, RoleListOption.CrewPower, RoleListOption.CrewSupport, RoleListOption.NeutBenign, RoleListOption.NeutEvil, RoleListOption.NeutKilling, RoleListOption.NeutOutlier },
        [RoleListOption.Any]          = new[] { RoleListOption.CrewInvest, RoleListOption.CrewKilling, RoleListOption.CrewProtective, RoleListOption.CrewPower, RoleListOption.CrewSupport, RoleListOption.NeutBenign, RoleListOption.NeutEvil, RoleListOption.NeutKilling, RoleListOption.NeutOutlier, RoleListOption.ImpConceal, RoleListOption.ImpKilling, RoleListOption.ImpPower, RoleListOption.ImpSupport },
    };

    public static bool TryGet(RoleListOption bucket, out TooltipInfo info)
    {
        var activeRoles = GetActiveRoles(bucket);
        if (activeRoles.Length == 0)
        {
            info = default;
            return false;
        }
        info = new TooltipInfo(activeRoles);
        return true;
    }

    private static RoleEntry[] GetActiveRoles(RoleListOption bucket)
    {
        // Resolve group bucket to specific buckets
        RoleListOption[] buckets;
        if (_groupBuckets.TryGetValue(bucket, out var grouped))
            buckets = grouped;
        else if (_allRoles.ContainsKey(bucket))
            buckets = new[] { bucket };
        else
            return System.Array.Empty<RoleEntry>();

        var result = new System.Collections.Generic.List<RoleEntry>();
        foreach (var b in buckets)
        {
            if (!_allRoles.TryGetValue(b, out var entries)) continue;
            foreach (var entry in entries)
            {
                // Check if role is active via ICustomRole.GetCount()
                var role = MiscUtils.AllRoles.FirstOrDefault(
                    r => r.GetType().FullName == entry.ClassFullName);


                if (role is ICustomRole customRole && customRole.GetCount() > 0)
                    result.Add(entry);
            }
        }

        return result.ToArray();
    }

    public static string ColorHex(Color c)
        => $"#{(int)(c.r * 255):X2}{(int)(c.g * 255):X2}{(int)(c.b * 255):X2}";

    public static string BuildTooltipText(in TooltipInfo info)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < info.Roles.Length; i++)
        {
            var r = info.Roles[i];

            var displayName = r.DisplayName;
            if (!string.IsNullOrEmpty(r.ClassFullName))
            {
                var role = MiscUtils.AllRoles.FirstOrDefault(x => x.GetType().FullName == r.ClassFullName);
                if (role != null)
                    displayName = role.GetRoleName();
            }

            if (!string.IsNullOrEmpty(r.ClassFullName))
                sb.Append($"<link=\"{r.ClassFullName}:{i}\"><color={ColorHex(r.Col)}>{displayName}</color></link>");
            else
                sb.Append($"<color={ColorHex(r.Col)}>{displayName}</color>");

            if (i < info.Roles.Length - 1)
                sb.Append(i % 2 == 1 ? "\n" : "  ");
        }
        return sb.ToString();
    }
}
