using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class BlackmailerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SpyRole>());
    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not BlackmailerRole || Player.HasDied() || !Player.AmOwner ||
            MeetingHud.Instance || (!HudManager.Instance.UseButton.isActiveAndEnabled &&
                                    !HudManager.Instance.PetButton.isActiveAndEnabled))
        {
            return;
        }

        HudManager.Instance.KillButton.ToggleVisible(
            OptionGroupSingleton<BlackmailerOptions>.Instance.BlackmailerKill ||
            (Player != null && Player.GetModifiers<BaseModifier>().Any(x => x is ICachedRole)) ||
            (Player != null && MiscUtils.ImpAliveCount == 1));
    }

    public DoomableType DoomHintType => DoomableType.Insight;
    public string IdPart => "Blackmailer";
    public string RoleName => MiraLocaleManager.Get($"TouRole{IdPart}");
    public string RoleDescription => MiraLocaleManager.Get($"TouRole{IdPart}IntroBlurb");
    public string RoleLongDescription => MiraLocaleManager.Get($"TouRole{IdPart}TabDescription");

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Blackmailer.LoadAsset(), "TouMira.Role.Impostor.Blackmailer", 1.45f),
        UseVanillaKillButton = true,
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        Icon = TouRoleIcons.Blackmailer
    };



    public string GetAdvancedDescription()
    {
        return
            MiraLocaleManager.Get($"TouRole{IdPart}WikiDescription").Replace("<symbol>", "<color=#2A1119>M</color>") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new(MiraLocaleManager.Get($"TouRole{IdPart}Blackmail", "Blackmail"),
                    MiraLocaleManager.Get($"TouRole{IdPart}BlackmailWikiDescription"),
                    TouImpAssets.BlackmailSprite)
            ];
        }
    }

    [MethodRpc((uint)TownOfUsRpc.Blackmail, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBlackmail(PlayerControl source, PlayerControl target)
    {
        if (LobbyBehaviour.Instance)
        {
            MiscUtils.RunAnticheatWarning(source);
            return;
        }
        var existingBmed = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(x => x.GetModifier<BlackmailedModifier>()?.BlackMailerId == source.PlayerId);

        existingBmed?.RemoveModifier<BlackmailedModifier>();

        var modifier = new BlackmailedModifier(source.PlayerId);
        target.AddModifier(modifier);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.BlackmailerBlackmail, source, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
}