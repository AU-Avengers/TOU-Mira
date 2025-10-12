using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Impostor;
using MiraAPI.Patches.Stubs;
using TownOfUs.Options;

namespace TownOfUs.Roles.Impostor;

public sealed class SpellslingerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Spellslinger";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");
    private static List<PlayerControl> _alivePlayersList;

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(TouLocale.GetParsed($"TouRole{LocaleKey}Hex", "Hex"),
            TouLocale.GetParsed($"TouRole{LocaleKey}HexWikiDescription"),
            TouImpAssets.HexSprite),
        new(TouLocale.GetParsed($"TouRole{LocaleKey}HexBomb", "Hex Bomb"),
            TouLocale.GetParsed($"TouRole{LocaleKey}HexBombWikiDescription"),
            TouImpAssets.HexSprite)
    ];

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Spellslinger,
        MaxRoleCount = 1,
        IntroSound = TouAudio.ArsoIgniteSound,
    };

    public static void OnRoundStart()
    {
        _alivePlayersList = Helpers.GetAlivePlayers();
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        _alivePlayersList = Helpers.GetAlivePlayers();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);

        var hexed = _alivePlayersList
            .Where(p => p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        var unhexedNonImpostors = _alivePlayersList
            .Where(p => !p.IsImpostor() && !p.HasModifier<SpellslingerHexedModifier>())
            .ToList();

        if (hexed.Count > 0)
        {
            sb.Append("\n<b>Hexed Players:</b>");
            foreach (var player in hexed)
            {
                var color = player.IsImpostor() ? "red" : "white";
                sb.Append(TownOfUsPlugin.Culture, $"\n<color={color}><size=75%>{player.Data.PlayerName}</size></color>");
            }
        }

        sb.Append(TownOfUsPlugin.Culture, $"\n\n<b>Players Left to Hex: {unhexedNonImpostors.Count}</b>");
        // foreach (var player in unhexedNonImpostors)
        // {
        //     sb.Append(TownOfUsPlugin.Culture, $"\n{player.Data.PlayerName}");
        // }

        return sb;
    }

    public static bool EveryoneHexed()
    {
        return PlayerControl.AllPlayerControls
            .ToArray()
            .Where(p => p.Data.Role is not SpellslingerRole && !p.HasDied() && (!p.IsImpostor() || OptionGroupSingleton<GeneralOptions>.Instance.FFAImpostorMode))
            .All(p => p.HasModifier<SpellslingerHexedModifier>());
    }

}