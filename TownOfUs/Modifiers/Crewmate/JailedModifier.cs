using MiraAPI.Events;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Crewmate;

public sealed class JailedModifier(byte jailorId) : BaseModifier
{
    private GameObject? jailCell;
    public override string ModifierName => "Jailed";
    public override bool HideOnUi => true;
    public byte JailorId { get; } = jailorId;
    public bool HasOpenedQuickChat { get; set; }

    public bool IsJailorValid => !GameData.Instance.GetPlayerById(JailorId).Object.HasDied() &&
                                 GameData.Instance.GetPlayerById(JailorId).Object.Data.Role is JailorRole;

    public override void OnActivate()
    {
        base.OnActivate();
        var jailor = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == JailorId);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.JailorJail, jailor!, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnMeetingStart()
    {
        Clear();
        if (GameData.Instance.GetPlayerById(JailorId).Object.HasDied() ||
            GameData.Instance.GetPlayerById(JailorId).Object.Data.Role is not JailorRole || Player.HasDied() ||
            !MeetingHud.Instance)
        {
            ModifierComponent!.RemoveModifier(this);
            return;
        }

        if (Player.Data.Role is ProsecutorRole pros)
        {
            pros.HideProsButton = true;
        }

        if (Player.AmOwner)
        {
            var title = $"<color=#{TownOfUsColors.Jailor.ToHtmlStringRGBA()}>Jailee Feedback</color>";
            var text =
                "You are jailed, convince the Jailor that you are Crew to avoid being executed in the chatbox above the user report button.";
            if (PlayerControl.LocalPlayer.Is(ModdedRoleTeams.Crewmate))
            {
                text =
                    "You are jailed, provide relevant information to the Jailor to prove you are Crew in the chatbox above the user report button.";
            }

            MiscUtils.AddFakeChat(PlayerControl.LocalPlayer.Data, title, text, false, true);

            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{TownOfUsColors.Jailor.ToTextColor()}{text}</color></b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Jailor.LoadAsset());

            notif1.AdjustNotification();
        }

        foreach (var voteArea in MeetingHud.Instance.playerStates)
        {
            if (Player.PlayerId == voteArea.TargetPlayerId)
            {
                GenCell(voteArea);
            }
        }
    }

    public void Clear()
    {
        jailCell?.Destroy();
    }

    private void GenCell(PlayerVoteArea voteArea)
    {
        var confirmButton = voteArea.Buttons.transform.GetChild(0).gameObject;
        var parent = confirmButton.transform.parent.parent;

        var jailCellObj = Object.Instantiate(confirmButton, voteArea.transform);

        var cellRenderer = jailCellObj.GetComponent<SpriteRenderer>();
        cellRenderer.sprite = TouAssets.InJailSprite.LoadAsset();

        jailCellObj.transform.localPosition = new Vector3(-0.95f, 0f, -2f);
        jailCellObj.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        jailCellObj.layer = 5;
        jailCellObj.transform.parent = parent;
        jailCellObj.transform.GetChild(0).gameObject.Destroy();

        var passive = jailCellObj.GetComponent<PassiveButton>();
        passive.OnClick = new Button.ButtonClickedEvent();

        jailCell = jailCellObj;
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}