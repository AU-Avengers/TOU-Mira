using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class TrapperRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public List<RoleBehaviour> TrappedPlayers { get; set; } = new();

    public DoomableType DoomHintType => DoomableType.Insight;
    public string LocaleKey => "Trapper";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Trap", "Trap"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}TrapWikiDescription"),
                    TouCrewAssets.TrapSprite)
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Trapper;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Trapper,
        IntroSound = CustomRoleUtils.GetIntroSound(RoleTypes.Tracker)
    };

    public void LobbyStart()
    {
        Clear();
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        Clear();
    }

    public void Clear()
    {
        TrappedPlayers.Clear();
        Trap.Clear();
    }

    public void Report()
    {
        // Logger<TownOfUsPlugin>.Error($"TrapperRole.Report");
        if (!Player.AmOwner)
        {
            return;
        }

        var minAmountOfPlayersInTrap = OptionGroupSingleton<TrapperOptions>.Instance.MinAmountOfPlayersInTrap;
        var msg = "No players entered any of your traps";

        if (TrappedPlayers.Count < minAmountOfPlayersInTrap)
        {
            msg = "Not enough players triggered your traps";
        }
        else if (TrappedPlayers.Count != 0)
        {
            var message = new StringBuilder("Roles caught in your trap:\n");

            TrappedPlayers.Shuffle();

            foreach (var role in TrappedPlayers)
            {
                message.Append(TownOfUsPlugin.Culture, $"{role.GetRoleName()}, ");
            }

            message = message.Remove(message.Length - 2, 2);

            var finalMessage = message.ToString();

            if (string.IsNullOrWhiteSpace(finalMessage))
            {
                return;
            }

            msg = finalMessage;
        }

        var title = $"<color=#{TownOfUsColors.Trapper.ToHtmlStringRGBA()}>{RoleName} Report</color>";
        MiscUtils.AddFakeChat(Player.Data, title, msg, false, true);
    }
}