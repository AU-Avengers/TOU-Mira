using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class TrapperRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;

    [HideFromIl2Cpp] public List<RoleBehaviour> TrappedPlayers { get; set; } = [];

    public DoomableType DoomHintType => DoomableType.Insight;
    public string IdPart => "Trapper";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Trap", "Trap"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Trap.WikiDescription"),
                    TouCrewAssets.TrapSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Trapper;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Trapper.LoadAsset(), "TouMira.Role.Crewmate.Trapper", 1.45f),
        Icon = TouRoleIcons.Trapper,
        OptionsScreenshot = TouBanners.TrapperRoleBanner,
        IntroSound = TouAudio.SuspenseIntro,
    };

    public void LobbyStart()
    {
        Clear();
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
        // Error($"TrapperRole.Report");
        if (!Player.AmOwner)
        {
            return;
        }

        var minAmountOfPlayersInTrap = OptionGroupSingleton<TrapperOptions>.Instance.MinAmountOfPlayersInTrap;
        var msg = MiraLocaleManager.Get("TownOfUsMira.Role.TrapperNoPlayers");

        if (TrappedPlayers.Count < minAmountOfPlayersInTrap)
        {
            msg = MiraLocaleManager.Get("TownOfUsMira.Role.TrapperNotEnoughPLayers");
        }
        else if (TrappedPlayers.Count != 0)
        {
            var message = new StringBuilder($"{MiraLocaleManager.Get("TownOfUsMira.Role.TrapperRolesCaught")}\n");

            TrappedPlayers.Shuffle();

            foreach (var role in TrappedPlayers)
            {
                message.Append(TownOfUsPlugin.Culture, $"{MiscUtils.GetHyperlinkText(role)}, ");
            }

            message = message.Remove(message.Length - 2, 2);

            var finalMessage = message.ToString();

            if (string.IsNullOrWhiteSpace(finalMessage))
            {
                return;
            }

            msg = finalMessage;
        }

        var title = $"<color=#{TownOfUsColors.Trapper.ToHtmlStringRGBA()}>{MiraLocaleManager.Get("TownOfUsMira.Role.TrapperMessageTitle")}</color>";
        MiscUtils.AddFakeChat(Player.Data, title, msg, false, true);
        TrappedPlayers.Clear();
    }
}