using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Modifiers.Impostor;

public sealed class BlackmailedModifier(byte blackMailerId) : BaseModifier
{
    public bool ShookAlready = true;
    public static int MaxAlivesNeeded => (int)OptionGroupSingleton<BlackmailerOptions>.Instance.MaxAliveForVoting;
    public static bool OnlyTargetSees => OptionGroupSingleton<BlackmailerOptions>.Instance.OnlyTargetSeesBlackmail;
    public SpriteRenderer BmOverlay;
    public PlayerVoteArea VoteArea;
    public override string ModifierName => "Blackmailed";
    public override bool HideOnUi => true;

    public byte BlackMailerId { get; } = blackMailerId;

    public override void OnActivate()
    {
        base.OnActivate();
        var meetingInstance = MeetingHud.Instance;
        if (meetingInstance == null)
        {
            return;
        }

        VoteArea = meetingInstance.playerStates.FirstOrDefault(x => x.TargetPlayerId == Player.PlayerId)!;

        var amOwner = Player.AmOwner;
        var bmOwns = BlackMailerId == PlayerControl.LocalPlayer.PlayerId;

        if (amOwner || bmOwns || !OnlyTargetSees)
        {
            ShookAlready = false;
            var bmIcon = Object.Instantiate(VoteArea.XMark, VoteArea.XMark.transform.parent);
            bmIcon.transform.localPosition = new Vector3(-0.804f, -0.212f, -2);
            bmIcon.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            bmIcon.sprite = TouAssets.BlackmailLetterSprite.LoadAsset();
            bmIcon.gameObject.SetActive(true);

            BmOverlay = Object.Instantiate(VoteArea.XMark, VoteArea.XMark.transform.parent);
            BmOverlay.transform.localPosition = new Vector3(0, 0, -2);
            BmOverlay.transform.localScale = new Vector3(0.769f, 1, 1);
            BmOverlay.sprite = TouAssets.BlackmailOverlaySprite.LoadAsset();
            BmOverlay.gameObject.SetActive(true);
            VoteArea.ColorBlindName.gameObject.SetActive(false);
        }
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        var meetingInstance = MeetingHud.Instance;
        VoteArea = meetingInstance.playerStates.FirstOrDefault(x => x.TargetPlayerId == Player.PlayerId)!;

        var amOwner = Player.AmOwner;
        var bmOwns = BlackMailerId == PlayerControl.LocalPlayer.PlayerId;

        if (amOwner || bmOwns || !OnlyTargetSees)
        {
            ShookAlready = false;
            var bmIcon = Object.Instantiate(VoteArea.XMark, VoteArea.XMark.transform.parent);
            bmIcon.transform.localPosition = new Vector3(-0.804f, -0.212f, -2);
            bmIcon.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            bmIcon.sprite = TouAssets.BlackmailLetterSprite.LoadAsset();
            bmIcon.gameObject.SetActive(true);

            BmOverlay = Object.Instantiate(VoteArea.XMark, VoteArea.XMark.transform.parent);
            BmOverlay.transform.localPosition = new Vector3(0, 0, -2);
            BmOverlay.transform.localScale = new Vector3(0.769f, 1, 1);
            BmOverlay.sprite = TouAssets.BlackmailOverlaySprite.LoadAsset();
            BmOverlay.gameObject.SetActive(true);
            VoteArea.ColorBlindName.gameObject.SetActive(false);
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        var meetingInstance = MeetingHud.Instance;
        if (meetingInstance == null || !VoteArea)
        {
            return;
        }
        
        if (meetingInstance.state != MeetingHud.VoteStates.Animating && !ShookAlready)
        {
            ShookAlready = true;
            meetingInstance.StartCoroutine(Effects.SwayX(BmOverlay.transform));
        }

        if (!VoteArea.DidVote && meetingInstance.state == MeetingHud.VoteStates.NotVoted &&
            (Helpers.GetAlivePlayers().Count > MaxAlivesNeeded))
        {
            VoteArea.SetVote(252);
            if (OnlyTargetSees)
            {
                VoteArea.Flag.enabled = false;
            }

            if (Player.AmOwner)
            {
                MeetingHud.Instance.Confirm(252);
            }
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}