using System.Collections;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Impostor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TownOfUs.Modifiers.Impostor;

public sealed class BlackmailedModifier(byte blackMailerId) : BaseModifier
{
    public bool ShookAlready = true;
    public bool IsVoteReady;
    public bool AboutToVote;
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
        SetUpOverlay();
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        SetUpOverlay();
    }

    public void SetUpOverlay()
    {
        var meetingInstance = MeetingHud.Instance;
        if (!meetingInstance)
        {
            return;
        }

        VoteArea = meetingInstance.playerStates.FirstOrDefault(x => x.TargetPlayerId == Player.PlayerId)!;

        var amOwner = Player.AmOwner;
        var bmOwns = BlackMailerId == PlayerControl.LocalPlayer.PlayerId;

        if (amOwner || bmOwns || !OnlyTargetSees)
        {
            var classic = LegacyAssets.IsLegacy;
            ShookAlready = false;
            var bmIcon = Object.Instantiate(VoteArea.XMark, VoteArea.XMark.transform.parent);
            bmIcon.transform.localPosition = new Vector3(-0.804f, -0.212f, -2);
            bmIcon.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            bmIcon.sprite = classic ? LegacyAssets.BlackmailLetterSprite.LoadAsset() : TouAssets.BlackmailLetterSprite.LoadAsset();
            bmIcon.gameObject.SetActive(true);

            BmOverlay = Object.Instantiate(VoteArea.XMark, VoteArea.XMark.transform.parent);
            BmOverlay.transform.localPosition = new Vector3(0, 0, -2);
            BmOverlay.transform.localScale = new Vector3(0.769f, 1, 1);
            BmOverlay.sprite = classic ? LegacyAssets.BlackmailOverlaySprite.LoadAsset() : TouAssets.BlackmailOverlaySprite.LoadAsset();
            BmOverlay.gameObject.SetActive(true);
            VoteArea.ColorBlindName.gameObject.SetActive(false);
        }
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        var meetingInstance = MeetingHud.Instance;
        if (!meetingInstance || !VoteArea)
        {
            return;
        }
        
        if (meetingInstance.state != MeetingHud.VoteStates.Animating && !ShookAlready)
        {
            ShookAlready = true;
            meetingInstance.StartCoroutine(Effects.SwayX(BmOverlay.transform));
        }

        if (!Player.AmOwner)
        {
            return;
        }
        if (!IsVoteReady && !VoteArea.DidVote && meetingInstance.state == MeetingHud.VoteStates.NotVoted &&
            (Helpers.GetAlivePlayers().Count > MaxAlivesNeeded))
        {
            Info($"Prepping vote, as {Helpers.GetAlivePlayers().Count} players is more than the requirement of {MaxAlivesNeeded} players");
            Coroutines.Start(CoRandomizeVote());
            IsVoteReady = true;
        }
    }

    private IEnumerator CoRandomizeVote()
    {
        var meetingInstance = MeetingHud.Instance;
        var logicOptionsNormal = GameManager.Instance.LogicOptions.TryCast<LogicOptionsNormal>();
        if (logicOptionsNormal == null)
        {
            Info("logic is not normal!");
            yield break;
        }

        var time = (float)Random.RandomRangeInt(10, logicOptionsNormal.GetVotingTime() - 5);
        var notif1 = Helpers.CreateAndShowNotification(
            $"You are unable to vote this meeting! You will auto-skip after {time} seconds.",
            Color.white,
            new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Blackmailer.LoadAsset());
        notif1.AdjustNotification();
        Info($"vote time is going to be {time}");
        while (time > 5f)
        {
            yield return new WaitForSeconds(5f);
            var allCompleted = true;
            foreach (var voter in meetingInstance.playerStates)
            {
                if (voter.AmDead || voter == VoteArea)
                {
                    continue;
                }

                if (!voter.DidVote)
                {
                    allCompleted = false;
                }
            }

            if (allCompleted)
            {
                AboutToVote = true;
                if (MeetingHud.Instance)
                {
                    VoteArea.SetVote(252);

                    meetingInstance.Confirm(252);

                    AboutToVote = false;
                    Info($"voted because everyone finished!");
                }

                yield break;
            }

            time -= 5f;
            Info($"vote time is going to be {time}");
        }

        yield return new WaitForSeconds(time);
        AboutToVote = true;
        if (MeetingHud.Instance)
        {
            VoteArea.SetVote(252);

            meetingInstance.Confirm(252);

            Info($"voted because time ran out!");
        }

        AboutToVote = false;
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}