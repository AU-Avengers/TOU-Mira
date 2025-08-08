using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using TownOfUs.Modifiers;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using System.Globalization;
using TownOfUs.Modifiers.Game.Alliance;

namespace TownOfUs.Roles.Crewmate;

public sealed class MonarchRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public override bool IsAffectedByComms => false;
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleName => TouLocale.Get(TouNames.Monarch, "Monarch");
    public string RoleDescription => "Knight Players To Give Extra Votes";
    public string RoleLongDescription => "Knight crewmates to aid in meetings, but don't knight Impostors";
    public Color RoleColor => TownOfUsColors.Monarch;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

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
        var formatProvider = CultureInfo.InvariantCulture;
        var votes = (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight;

        // Add a blank line before extra info for spacing
        sb.AppendLine();

        sb.AppendLine(formatProvider, $"Knighted players each gain {votes} vote(s).");

        var egoIsThriving = PlayerControl.LocalPlayer?.HasModifier<EgotistModifier>() ?? false;

        if (OptionGroupSingleton<MonarchOptions>.Instance.CrewKnightsGrantKillImmunity)
        {
            if (egoIsThriving)
                sb.AppendLine("If at least one knighted player is alive, you are immune to kills.");
            else
                sb.AppendLine("If at least one knighted crewmate is alive, you are immune to kills.");
        }

        if (OptionGroupSingleton<MonarchOptions>.Instance.InformWhenKnightDies)
            sb.AppendLine("You are notified when a knighted player dies.");

        return sb;
    }
    public string GetAdvancedDescription()
    {
        var votes = (int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight;
        var desc = $"The {RoleName} is a Crewmate Power role that can knight other players, granting {votes} vote(s) each.";

        if (OptionGroupSingleton<MonarchOptions>.Instance.CrewKnightsGrantKillImmunity)
            desc += "\n\nIf a knighted crewmate is alive, the Monarch is immune to kills. Evil knights do not grant said immunity.";

        if (OptionGroupSingleton<MonarchOptions>.Instance.CrewKnightsGrantKillImmunity && OptionGroupSingleton<AllianceModifierOptions>.Instance.EgotistChance != 0)
            desc += "\nAn Egotist Monarch, however, gains kill immunity from anyone being knighted.";

        if (OptionGroupSingleton<MonarchOptions>.Instance.InformWhenKnightDies)
                desc += "\n\nThe Monarch is notified when a knighted player dies.";

        return desc + MiscUtils.AppendOptionsText(GetType());
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities { get; } =
    [
        new("Knight",
            $"Knight a player, giving them {(int)OptionGroupSingleton<MonarchOptions>.Instance.VotesPerKnight} vote(s)",
            TouCrewAssets.KnightButtonSprite)
    ];

    [MethodRpc((uint)TownOfUsRpc.Knight, SendImmediately = true)]
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

}