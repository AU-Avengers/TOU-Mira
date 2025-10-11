using System.Text;
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
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Roles.Impostor;

public sealed class BlackmailerRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public void FixedUpdate()
    {
        if (Player == null || Player.Data.Role is not BlackmailerRole || Player.HasDied() || !Player.AmOwner ||
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
    public string LocaleKey => "Blackmailer";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = true,
        Icon = TouRoleIcons.Blackmailer
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        return ITownOfUsRole.SetNewTabText(this);
    }

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription").Replace("<symbol>", "<color=#2A1119>M</color>") +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Blackmail", "Blackmail"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}BlackmailWikiDescription"),
                    TouImpAssets.BlackmailSprite)
            };
        }
    }

    [MethodRpc((uint)TownOfUsRpc.Blackmail, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcBlackmail(PlayerControl source, PlayerControl target)
    {
        var existingBmed = PlayerControl.AllPlayerControls.ToArray()
            .FirstOrDefault(x => x.GetModifier<BlackmailedModifier>()?.BlackMailerId == source.PlayerId);

        existingBmed?.RemoveModifier<BlackmailedModifier>();

        var modifier = new BlackmailedModifier(source.PlayerId);
        target.AddModifier(modifier);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.BlackmailerBlackmail, source, target);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }
}