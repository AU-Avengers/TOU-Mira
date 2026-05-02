using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using TownOfUs.Options;
using TownOfUs.Patches;
using UnityEngine;

namespace TownOfUs.Utilities;

public static class BucketTooltipData
{
    public sealed class RoleEntry(string display, RoleTypes id, string classFullName, Color col)
    {
        public string DisplayName = display;
        public RoleTypes RoleId = id;
        public string ClassFullName = classFullName;
        public Color Col = col;
    }

    public readonly struct TooltipInfo
    {
        internal readonly RoleEntry[] Roles;
        public TooltipInfo(RoleEntry[] roles) { Roles = roles; }
    }

    // ── All possible roles per bucket ─────────────────────────────────────────
    public static Dictionary<RoleListOption, RoleEntry[]> AllRoles => _allRoles;

    private static readonly Dictionary<RoleListOption, RoleEntry[]> _allRoles = new();

    // Group buckets map to multiple specific buckets
    private static readonly Dictionary<RoleListOption, RoleListOption[]> _groupBuckets = new()
    {
        {
            RoleListOption.CrewCommon,
            [RoleListOption.CrewInvest, RoleListOption.CrewProtective, RoleListOption.CrewSupport]
        },
        { RoleListOption.CrewSpecial, [RoleListOption.CrewKilling, RoleListOption.CrewPower] },
        {
            RoleListOption.CrewRandom,
            [
                RoleListOption.CrewInvest, RoleListOption.CrewKilling, RoleListOption.CrewProtective,
                RoleListOption.CrewPower, RoleListOption.CrewSupport
            ]
        },
        { RoleListOption.NeutCommon, [RoleListOption.NeutBenign, RoleListOption.NeutEvil] },
        { RoleListOption.NeutSpecial, [RoleListOption.NeutKilling, RoleListOption.NeutOutlier] },
        {
            RoleListOption.NeutWildcard,
            [RoleListOption.NeutBenign, RoleListOption.NeutEvil, RoleListOption.NeutOutlier]
        },
        {
            RoleListOption.NeutRandom,
            [RoleListOption.NeutBenign, RoleListOption.NeutEvil, RoleListOption.NeutKilling, RoleListOption.NeutOutlier]
        },
        { RoleListOption.ImpCommon, [RoleListOption.ImpConceal, RoleListOption.ImpSupport] },
        { RoleListOption.ImpSpecial, [RoleListOption.ImpKilling, RoleListOption.ImpPower] },
        {
            RoleListOption.ImpRandom,
            [RoleListOption.ImpConceal, RoleListOption.ImpKilling, RoleListOption.ImpPower, RoleListOption.ImpSupport]
        },
        {
            RoleListOption.NonImp,
            [
                RoleListOption.CrewInvest, RoleListOption.CrewKilling, RoleListOption.CrewProtective,
                RoleListOption.CrewPower, RoleListOption.CrewSupport, RoleListOption.NeutBenign,
                RoleListOption.NeutEvil, RoleListOption.NeutKilling, RoleListOption.NeutOutlier
            ]
        },
        {
            RoleListOption.Any,
            [
                RoleListOption.CrewInvest, RoleListOption.CrewKilling, RoleListOption.CrewProtective,
                RoleListOption.CrewPower, RoleListOption.CrewSupport, RoleListOption.NeutBenign,
                RoleListOption.NeutEvil, RoleListOption.NeutKilling, RoleListOption.NeutOutlier,
                RoleListOption.ImpConceal, RoleListOption.ImpKilling, RoleListOption.ImpPower, RoleListOption.ImpSupport
            ]
        },
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
        {
            buckets = grouped;
        }
        else if (_allRoles.ContainsKey(bucket))
        {
            buckets = [ bucket ];
        }
        else
        {
            Error($"Bucket missing!");
            return System.Array.Empty<RoleEntry>();
        }

        var result = new List<RoleEntry>();
        foreach (var b in buckets)
        {
            if (!_allRoles.TryGetValue(b, out var entries))
            {
                Error($"All Roles doesn't contain {b.ToDisplayString()}!");
                continue;
            }

            if (!HudManagerPatches.TooltipAlignments.TryGetValue(b, out var alignment))
            {
                Error($"All Roles doesn't contain alignment: {b.ToDisplayString()}!");
                continue;
            }
            var allRoles = MiscUtils.GetRegisteredRoles(alignment).ToList();
            foreach (var role in allRoles)
            {
                var entry = entries.FirstOrDefault(x => x.RoleId == role.Role);
                if (entry == null)
                {
                    Error("Missing entry...");
                    continue;
                }

                entry.DisplayName = role.GetRoleName();
                entry.Col = role.TeamColor;
                if (role is ICustomRole customRole && customRole.Configuration.MaxRoleCount != 0 && (int)customRole.GetCount()! > 0 &&
                    (int)customRole.GetChance()! > 0)
                {
                    result.Add(entry);
                }
                else
                {
                    var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                    var roleOptions = currentGameOptions.RoleOptions;

                    if (roleOptions.GetNumPerGame(role.Role) > 0 && roleOptions.GetChancePerGame(role.Role) > 0)
                    {
                        result.Add(entry);
                    }
                }
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
            {
                sb.Append(TownOfUsPlugin.Culture, $"<link=\"{r.ClassFullName}:{i}\"><color={ColorHex(r.Col)}>{displayName}</color></link>");
            }
            else
            {
                sb.Append(TownOfUsPlugin.Culture, $"<color={ColorHex(r.Col)}>{displayName}</color>");
            }

            if (i < info.Roles.Length - 1)
                sb.Append(i % 2 == 1 ? "\n" : "  ");
        }
        return sb.ToString();
    }
}
