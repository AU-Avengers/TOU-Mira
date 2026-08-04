using System.Text;
using AchievementsAPI.API;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Achievements;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class LookoutWatchedModifier(PlayerControl lookout) : BaseModifier
{
    public override string ModifierName => "Watched";
    public override bool HideOnUi => true;

    public PlayerControl Lookout { get; set; } = lookout;
    public Dictionary<PlayerControl, RoleBehaviour> SeenPlayers { get; set; } = [];

    public override void OnActivate()
    {
        base.OnActivate();

        var touAbilityEvent = new TouAbilityEvent(AbilityType.LookoutWatch, Lookout, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Lookout.AmOwner)
        {
            Player?.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(TownOfUsColors.Lookout));
        }
    }

    public override void OnMeetingStart()
    {
        if (Lookout.HasDied() || !Lookout.AmOwner || PlayerControl.LocalPlayer.Data.Role is not LookoutRole)
        {
            return;
        }

        var title = $"<color=#{TownOfUsColors.Lookout.ToHtmlStringRGBA()}>{TouLocale.GetParsed("TouRoleLookoutFeedbackTitle")}</color>";
        var msg = TouLocale.GetParsed("TouRoleLookoutNoInteractionFeedback").Replace("<player>", Player.Data.PlayerName);

        var showRoles = (LookoutView)OptionGroupSingleton<LookoutOptions>.Instance.WatchType.Value is LookoutView.Roles;
        if (SeenPlayers.Count != 0)
        {
            var playerList = SeenPlayers.Select(x => x.Key).ToList();
            var roleList = SeenPlayers.Select(x => x.Value).ToList();
            var message = new StringBuilder($"{TouLocale.GetParsed(showRoles ? "TouRoleLookoutInteractionFeedback" : "TouRoleLookoutAltInteractionFeedback").Replace("<player>", Player.Data.PlayerName)}:\n");

            playerList.Shuffle();
            roleList.Shuffle();

            if (showRoles)
            {
                foreach (var role in roleList)
                {
                    message.Append(TownOfUsPlugin.Culture, $"{role.GetRoleName()}, ");
                }
            }
            else
            {
                foreach (var plr in playerList)
                {
                    message.Append(TownOfUsPlugin.Culture, $"{plr.CachedPlayerData.PlayerName}, ");
                }
            }

            if (SeenPlayers.Count > 4)
            {
                AchievementsTabSingleton<TouCrewRoleAchievementsTab>.Instance.SearchParty.Unlock();
            }

            message = message.Remove(message.Length - 2, 2);

            var final = message.ToString();

            if (string.IsNullOrWhiteSpace(final))
            {
                return;
            }

            msg = final;
        }
        else if (Player.HasDied())
        {
            AchievementsTabSingleton<TouCrewRoleAchievementsTab>.Instance.ParanormalActivity.Unlock();
        }

        MiscUtils.AddFakeChat(Player.Data, title, msg, false, true);

        SeenPlayers.Clear();
    }
}