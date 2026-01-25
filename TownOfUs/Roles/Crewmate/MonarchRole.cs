using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Patches.Stubs;
using Reactor.Utilities.Extensions;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Modifiers.Game.Alliance;

namespace TownOfUs.Roles.Crewmate;

public sealed class MonarchRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Monarch";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");
    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription");
    public RealFlash currentFlashType = RealFlash.Medic;

    [HideFromIl2Cpp]
    public Color? GetFlashColor()
    {
        switch (currentFlashType)
        {
            case RealFlash.Cleric:
                return TownOfUsColors.Cleric;
            case RealFlash.Medic:
                return TownOfUsColors.Medic;
            case RealFlash.Mercenary:
                return TownOfUsColors.Mercenary;
            case RealFlash.Warden:
                return TownOfUsColors.Warden;
        }
        return null;
    }

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public Color RoleColor => TownOfUsColors.Monarch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public static string VoteInfoString = TouLocale.GetParsed("TouRoleMonarchTabVoteInfo");
    public static string DefenseEgoString = TouLocale.GetParsed("TouRoleMonarchTabDefenseInfoEgo");
    public static string DefenseString = TouLocale.GetParsed("TouRoleMonarchTabDefenseInfo");
    public static string DeathInfoString = TouLocale.GetParsed("TouRoleMonarchTabDeathInfo");

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        VoteInfoString = TouLocale.GetParsed("TouRoleMonarchTabVoteInfo");
        DefenseEgoString = TouLocale.GetParsed("TouRoleMonarchTabDefenseInfoEgo");
        DefenseString = TouLocale.GetParsed("TouRoleMonarchTabDefenseInfo");
        DeathInfoString = TouLocale.GetParsed("TouRoleMonarchTabDeathInfo");
        var monOpts = OptionGroupSingleton<MonarchOptions>.Instance;
        if (monOpts.CrewKnightsGrantKillImmunity)
        {
            var flashColor = (ProtectionFlash)monOpts.ProtectionFlashColor.Value;
            switch (flashColor)
            {
                case ProtectionFlash.NoFlash:
                    currentFlashType = RealFlash.NoFlash;
                    break;
                case ProtectionFlash.Cleric:
                    currentFlashType = RealFlash.Cleric;
                    break;
                case ProtectionFlash.Medic:
                    currentFlashType = RealFlash.Medic;
                    break;
                case ProtectionFlash.Mercenary:
                    currentFlashType = RealFlash.Mercenary;
                    break;
                case ProtectionFlash.Warden:
                    currentFlashType = RealFlash.Warden;
                    break;
            }

            if (Player.AmOwner)
            {
                CustomButtonSingleton<MonarchProtectionFlashButton>.Instance.SetShieldType(currentFlashType);
            }
        }
    }

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = TouRoleIcons.Monarch,
        IntroSound = TouAudio.ToppatIntroSound,
        MaxRoleCount = 1
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        var votes = (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight;

        // Add a blank line before extra info for spacing
        sb.AppendLine();

        sb.AppendLine(TownOfUsPlugin.Culture, $"{VoteInfoString.Replace("<amount>", votes.ToString(TownOfUsPlugin.Culture))}");

        var egoIsThriving = PlayerControl.LocalPlayer?.HasModifier<EgotistModifier>() ?? false;

        if (OptionGroupSingleton<MonarchOptions>.Instance.CrewKnightsGrantKillImmunity)
        {
            if (egoIsThriving)
                sb.AppendLine(TownOfUsPlugin.Culture, $"{DefenseEgoString}");
            else
                sb.AppendLine(TownOfUsPlugin.Culture, $"{DefenseString}");
        }

        if (OptionGroupSingleton<MonarchOptions>.Instance.InformWhenKnightDies)
            sb.AppendLine(TownOfUsPlugin.Culture, $"{DeathInfoString}");

        return sb;
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new(TouLocale.GetParsed($"TouRole{LocaleKey}Knight", "Knight"),
                    TouLocale.GetParsed($"TouRole{LocaleKey}KnightDescription").Replace("<amount>",
                        ((int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight).ToString(TownOfUsPlugin.Culture)),
                    TouCrewAssets.KnightSprite),
                new(TouLocale.GetParsed($"TouRole{LocaleKey}ChangeFlash", "Change Flash"),
                TouLocale.GetParsed($"TouRole{LocaleKey}ChangeFlashDescription"),
                MonarchProtectionFlashButton.ProtectionButtons.AsEnumerable().Random()!)
            };
        }
    }

    [MethodRpc((uint)TownOfUsRpc.Knight)]
    public static void RpcKnight(PlayerControl player, PlayerControl target)
    {
        var targetName = target.CachedPlayerData.PlayerName;
        var icon = TouRoleIcons.Monarch.LoadAsset();

        if (target.HasDied())
        {
            if (player.AmOwner)
            {
                ShowNotification($"{targetName} died before you could knight them.");
            }
            return;
        }

        target.AddModifier<KnightedModifier>();

        if (player.AmOwner)
        {
            ShowNotification($"{targetName} was knighted!");
        }

        if (target.AmOwner)
        {
            ShowNotification($"You were knighted by a {TownOfUsColors.Monarch.ToTextColor()}Monarch</color>. You gained {(int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight} vote(s)!");
        }


        void ShowNotification(string message)
        {
            var notif = Helpers.CreateAndShowNotification($"<b>{message}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: icon);
            notif.Text.SetOutlineThickness(0.35f);
        }
    }

    [MethodRpc((uint)TownOfUsRpc.UpdateMonShield)]
    public static void RpcUpdateMonShield(PlayerControl monarch, int shieldId)
    {
        if (monarch.Data.Role is not MonarchRole role)
        {
            Error("RpcUpdateMonShield - Invalid monarch");
            return;
        }

        var newFlash = (RealFlash)shieldId;
        if (Enum.IsDefined(newFlash))
        {
            role.currentFlashType = newFlash;
        }
        else
        {
            Error("RpcUpdateMonShield - Invalid shield type!");
        }

        if (monarch.AmOwner)
        {
            CustomButtonSingleton<MonarchProtectionFlashButton>.Instance.SetShieldType(role.currentFlashType);
        }
    }

    public static void OnRoundStart()
    {
        CustomButtonSingleton<MonarchKnightButton>.Instance.Usable = true;
    }

}

public enum RealFlash
{
    NoFlash,
    Cleric,
    Medic,
    Mercenary,
    Warden
}