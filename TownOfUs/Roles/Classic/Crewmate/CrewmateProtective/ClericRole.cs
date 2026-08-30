using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Crewmate;

public sealed class ClericRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Protective;
    public string IdPart => "Cleric";
    public string RoleName => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.TabDescription");

    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}.WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }

    public static string ProtectionString = MiraLocaleManager.Get("TownOfUsMira.Role.ClericTabProtecting");

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        ProtectionString = MiraLocaleManager.Get("TownOfUsMira.Role.ClericTabProtecting");
    }

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var stringB = ITownOfUsRole.SetNewTabText(this);

        var barrieredPlayer = ModifierUtils.GetPlayersWithModifier<ClericBarrierModifier>(x => x.Cleric.AmOwner).FirstOrDefault();
        if (barrieredPlayer != null)
        {
            stringB.AppendLine(TownOfUsPlugin.Culture, $"\n<b>{ProtectionString.Replace("<player>", barrieredPlayer.Data.PlayerName)}</b>");
        }

        return stringB;
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Barrier", "Barrier"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Barrier.WikiDescription").Replace("<BarrierCooldown>",
                        $"{OptionGroupSingleton<ClericOptions>.Instance.BarrierCooldown}"),
                    TouCrewAssets.BarrierSprite),
                new(MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Cleanse", "Cleanse"),
                    MiraLocaleManager.Get($"TownOfUsMira.Role.{IdPart}Cleanse.WikiDescription"),
                    TouCrewAssets.CleanseSprite)
            ];
        }
    }

    public Color RoleColor => TownOfUsColors.Cleric;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateProtective;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Cleric.LoadAsset(), "TouMira.Role.Crewmate.Cleric", 1.45f),
        IntroSound = TouAudio.PotionIntro,
        OptionsScreenshot = TouBanners.ClericRoleBanner,
        Icon = TouRoleIcons.Cleric
    };

    [MethodRpc((uint)TownOfUsRpc.ClericBarrierAttacked)]
    public static void RpcClericBarrierAttacked(PlayerControl source, PlayerControl cleric, PlayerControl shielded)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }
        if (cleric.Data.Role is not ClericRole)
        {
            Error("RpcClericBarrierAttacked - Invalid cleric");
            return;
        }

        if (source.AmOwner ||
            (cleric.AmOwner &&
             OptionGroupSingleton<ClericOptions>.Instance.AttackNotif))
        {
            Coroutines.Start(MiscUtils.CoFlash(OptionGroupSingleton<GameMechanicOptions>.Instance.AnonymousShields && !cleric.AmOwner ? TownOfUsColors.NeutralWiki : TownOfUsColors.Cleric));
        }
    }
}