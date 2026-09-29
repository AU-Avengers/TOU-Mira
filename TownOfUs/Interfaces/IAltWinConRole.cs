using MiraAPI.Utilities;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using UnityEngine;

namespace TownOfUs.Interfaces;

public interface IAltWinConRole
{
    bool ReachedWinCondition { get; }

    AltWinResult WinOutcome { get; }

    AltWinResult EffectiveWinOutcome { get; }

    bool RoleIsDisplayed { get; }

    bool AboutToTorment { get; set; }

    bool HasKilled { get; set; }

    virtual Color RoleColor => TownOfUsColors.Neutral;

    virtual LoadableAsset<Sprite> WinIcon => TouRoleIcons.Neutral;

    virtual string OwnerVictoryString => "TouNeutBasicVictoryMessage";
    virtual string NonOwnerVictoryString => "TouNeutBasicVictoryMessageSelf";
    virtual string OwnerTormentString => "TouNeutBasicTormentMessage";

    public virtual void TryResolveQuietWin(RoleBehaviour roleBehaviour)
    {
        if (AboutToTorment || EffectiveWinOutcome == AltWinResult.EndsGame || !ReachedWinCondition)
        {
            return;
        }

        AboutToTorment = true;

        var player = roleBehaviour.Player;
        if (player == null)
        {
            return;
        }

        var roleTag = $"{RoleColor.ToTextColor()}{roleBehaviour.GetRoleName()}</color>";

        if (player.AmOwner)
        {
            if (!player.HasDied())
            {
                player.DelayExile();
                GameHistory.UpdatePlayerDeathData(
                    player,
                    MiraLocaleManager.Get("DiedToWinning"),
                    HudManagerHelper.Instance.CurrentRound,
                    diedThisRound: DeathHandlerOverride.SetTrue,
                    lockInfo: DeathHandlerOverride.SetTrue);
            }

            var selfNotif = Helpers.CreateAndShowNotification(
                $"<b>{MiraLocaleManager.Get(OwnerVictoryString).Replace("<role>", roleTag)}</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: WinIcon.LoadAsset());
            selfNotif.AdjustNotification();

            if (EffectiveWinOutcome == AltWinResult.KillsOnePlayer)
            {
                var tormentNotif = Helpers.CreateAndShowNotification(
                    $"<b>{MiraLocaleManager.Get(OwnerTormentString)}</b>",
                    Color.white, new Vector3(0f, 0.85f, -20f));
                tormentNotif.AdjustNotification();
                SetUpTormentButton(roleBehaviour);
            }
        }
        else
        {
            string message;
            LoadableAsset<Sprite> icon;

            if (!RoleIsDisplayed)
            {
                message = MiraLocaleManager.Get("TouNeutAnonymousVictoryMessage");
                icon = WinIcon;
            }
            else
            {
                message = $"<b>{MiraLocaleManager.Get(NonOwnerVictoryString)
                    .Replace("<role>", roleTag)}</b>";
                icon = TouRoleIcons.Doomsayer;
            }

            var notif1 = Helpers.CreateAndShowNotification(
                message.Replace("<player>", player.Data.PlayerName),
                Color.white, new Vector3(0f, 1f, -20f), spr: icon.LoadAsset());

            notif1.AdjustNotification();
        }
    }

    public virtual void SetUpTormentButton(RoleBehaviour roleBehaviour)
    {
        /*var button = CustomButtonSingleton<NeutralEvilTormentButton>.Instance;
        button.Owner = role;
        button.Show = true;
        button.SetActive(true, roleBehaviour);*/
    }
}
public enum AltWinResult
{
    Leaves,
    KillsOnePlayer,
    EndsGame,
}