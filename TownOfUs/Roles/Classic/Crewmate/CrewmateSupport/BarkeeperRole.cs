using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using UnityEngine;
using MiraAPI.Hud;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modifiers.Other;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;

namespace TownOfUs.Roles.Crewmate;

public sealed class BarkeeperRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Barkeeper";
    public string RoleName => TouLocale.Get($"TouRole{LocaleKey}");
    public string RoleDescription => TouLocale.GetParsed($"TouRole{LocaleKey}IntroBlurb");

    public string RoleLongDescription => TouLocale.GetParsed($"TouRole{LocaleKey}TabDescription").Replace("<blockTime>",
        OptionGroupSingleton<RoleblockOptions>.Instance.RoleblockDuration.Value.ToString(TownOfUsPlugin.Culture));

    public string GetAdvancedDescription()
    {
        return
            TouLocale.GetParsed($"TouRole{LocaleKey}WikiDescription") +
            MiscUtils.AppendOptionsText(GetType());
    }
    public Color RoleColor => TownOfUsColors.Barkeeper;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Barkeeper.LoadAsset(), "TouMira.Role.Crewmate.Barkeeper", 1.45f),
        Icon = TouRoleIcons.Barkeeper,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.PotionIntro
    };

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new(TouLocale.Get("TouRoleBarkeeperRoleblock"),
            (OptionGroupSingleton<RoleblockOptions>.Instance.Hangover.Value
                ? TouLocale.GetParsed("TouRoleBarkeeperRoleblockWikiDescriptionWithHangover").Replace("<overTime>",
                    OptionGroupSingleton<RoleblockOptions>.Instance.HangoverDuration.Value.ToString(TownOfUsPlugin
                        .Culture))
                : TouLocale.GetParsed("TouRoleBarkeeperRoleblockWikiDescription")).Replace("<blockTime>",
                OptionGroupSingleton<RoleblockOptions>.Instance.RoleblockDuration.Value
                    .ToString(TownOfUsPlugin.Culture)),
            TouCrewAssets.RoleblockSprite),
        new(TouLocale.Get("TouRoleBarkeeperSpill"),
            TouLocale.GetParsed("TouRoleBarkeeperSpillWikiDescription"),
            TouCrewAssets.SpillSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.SpillDrink)]
    public static void RpcSpillDrink(PlayerControl player, Vector2 pos)
    {
        if (player.Data.Role is not BarkeeperRole)
        {
            MiscUtils.RunAnticheatWarning(player);
            return;
        }
        DrinkSpillComponent.CreateDrinkSpill(player, pos);
    }

    [MethodRpc((uint)TownOfUsRpc.Roleblock)]
    public static void RpcRoleblock(PlayerControl player, PlayerControl target)
    {
        var options = OptionGroupSingleton<RoleblockOptions>.Instance;
        var roleblockDuration = options.RoleblockDuration.Value;
        var hangoverDuration = options.HangoverDuration.Value;
        var applyHangover = options.Hangover.Value;
        var invertControls = options.InvertControlsOfRoleblocked.Value;
        var targetName = target.CachedPlayerData.PlayerName;
        var rbText = $"{targetName} was roleblocked!";
        var poisonPlayer = false;
        if (player.Data.Role is BootleggerRole)
        {
            var poisTrigger =
                (PoisonTrigger)OptionGroupSingleton<BootleggerOptions>.Instance.PoisonRoleblockTrigger.Value;
            var progress = PoisonProgress.Begun;
            if (target.TryGetModifier<BootleggerPoisonModifier>(out var bootProgress))
            {
                if (bootProgress.Poison < PoisonProgress.Poison)
                {
                    bootProgress.Poison++;
                }
                progress = bootProgress.Poison;
                if (bootProgress.Poison is PoisonProgress.Poison && poisTrigger is PoisonTrigger.OnDurationEnd)
                {
                    bootProgress.StartTimer();
                }
                poisonPlayer = bootProgress.Poison is PoisonProgress.Poison;
            }
            else
            {
                target.AddModifier<BootleggerPoisonModifier>(player);
            }

            if (player.AmOwner)
            {
                switch (progress)
                {
                    case PoisonProgress.Begun:
                        rbText += "\nNext time, they will become sick.";
                        break;
                    case PoisonProgress.Sick:
                        rbText += "\nNext time, they will be poisoned.";
                        break;
                    case PoisonProgress.Poison:
                        rbText += "\nWait for the poison to kick in.";
                        break;
                }
                var notif = CustomButtonSingleton<BootleggerRoleblockButton>.Instance.NotifMessage;
                if (notif != null)
                {
                    notif.UpdateMessage(rbText);
                    notif.alphaTimer = 4f;
                    notif.AdjustNotification();
                }
                else
                {
                    ShowNotification($"<b>{rbText}</b>", TouRoleIcons.Bootlegger.LoadAsset());
                }
            }
        }
        else if (player.AmOwner)
        {
            var notif = CustomButtonSingleton<BarkeeperRoleblockButton>.Instance.NotifMessage;
            if (notif != null)
            {
                notif.UpdateMessage(rbText);
                notif.alphaTimer = 4f;
                notif.AdjustNotification();
            }
            else
            {
                ShowNotification($"<b>{rbText}</b>", TouRoleIcons.Barkeeper.LoadAsset());
            }
        }

        var immune = true;
        if (!target.HasModifier<HangoverModifier>() && !target.HasModifier<DrunkModifier>() &&
            !target.HasModifier<RoleblockedModifier>() && target.Data.Role is not BootleggerRole &&
            target.Data.Role is not BarkeeperRole)
        {
            immune = false;
            target.AddModifier<RoleblockedModifier>(player, invertControls, applyHangover, roleblockDuration, hangoverDuration);
        }

        if (target.AmOwner)
        {
            var iconTarget = TouRoleIcons.Barkeeper.LoadAsset();
            var msg = immune
                ? "Someone gave you a drink, but you are too hungover!"
                : "Someone gave you a drink, you were roleblocked!";
            if (poisonPlayer)
            {
                msg += "\n<color=#D64042>You feel a sense of impending doom.</color>";
                iconTarget = TouRoleIcons.Bootlegger.LoadAsset();
            }
            ShowNotification(msg, iconTarget);
        }


        static void ShowNotification(string message, Sprite icon)
        {
            var notif = Helpers.CreateAndShowNotification($"<b>{message}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: icon);
            notif.AdjustNotification();
            notif.alphaTimer = 4f;
        }
    }

}