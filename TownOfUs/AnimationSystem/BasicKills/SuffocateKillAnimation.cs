using System.Collections;
using PowerTools;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace TownOfUs.AnimationSystem.BasicKills;

[RegisterInIl2Cpp]
public class SuffocateKillAnimation(IntPtr cppPtr) : CustomKillAnimation(cppPtr)
{
    public override bool ShouldInheritFromAnimation => true;
    public override bool ShouldShowPets => false;
    public static KillAnimType PublicKillAnimType { get; } = KillAnimationHelpers.CreateNewKillAnimType();

    public override void InheritFromAnimation(OverlayKillAnimation animation)
    {
        animation.killerParts.gameObject.SetActive(false);
        animation.transform.Find("killstabknife").gameObject.SetActive(false);
        animation.transform.Find("killstabknifehand").gameObject.SetActive(false);
        base.InheritFromAnimation(animation);
        victimParts.transform.localPosition = new Vector3(-1.5f, 0, 0);
    }

    public static SuffocateKillAnimation CreateKillAnimation(NetworkedPlayerInfo killer, NetworkedPlayerInfo victim)
    {
        Transform parent = new GameObject("SuffocateKillAnimation").DontUnload().DontDestroy().transform;
        parent.gameObject.SetActive(false);
        OverlayKillAnimation original = Instantiate(HudManager.Instance.KillOverlay.KillAnims[0], parent);

        var newKillAnim = original.gameObject.AddComponent<SuffocateKillAnimation>();
        newKillAnim.InitAnimation(HudManager.Instance.KillOverlay, original, killer, victim);
        return newKillAnim;
    }

    public override void InitAnimation(KillOverlay overlay, OverlayKillAnimation prefab, NetworkedPlayerInfo killer,
        NetworkedPlayerInfo victim)
    {
        KillType = PublicKillAnimType;
        base.InitAnimation(overlay, prefab, killer, victim);
    }
    public readonly KillAnimFrame[] KillFrames =
        [
            new(0, 0, 4, new Vector2(0, 0)),
            new(0, 1.08f, 2, new Vector2(0, 0)),
            new(0, 1.15f, 2, new Vector2(0, 0)),
            new(0, 1.28f, 2, new Vector2(0, 0)),
            new(0, 1.33f, 4, new Vector2(0, 0)),
            new(0, 1.37f, 2, new Vector2(0, 0)),
            new(0, 1.39f, 4, new Vector2(0, 0)),
            new(0, 1.42f, 4, new Vector2(0, 0)),
            new(0, 1.46f, 4, new Vector2(0, 0)),
            new(0, 1.5f, 4, new Vector2(0, 0)),
            new(0, 1.56f, 4, new Vector2(0, 0)),

            new(3, 0.43f, 3, new Vector2(-0.3f, 0)),
            new(3, 0.66f, 4, new Vector2(-0.5f, 0)),
            new(3, 0.89f, 4, new Vector2(-0.5f, 0)),
            new(3, 0.97f, 4, new Vector2(-0.5f, 0)),
            new(3, 1.01f, 4, new Vector2(-0.5f, 0)),
            new(3, 1.05f, 4, new Vector2(-0.5f, 0)),
            new(3, 1.11f, 4, new Vector2(-0.5f, 0)),
            new(3, 1.14f, 4, new Vector2(-0.5f, 0)),
            new(3, 1.17f, 4, new Vector2(-0.5f, 0)),
            new(3, 1.23f, 4, new Vector2(-0.5f, 0)),
            new(3, 1.27f, 4, new Vector2(-0.5f, 0)),
            new(3, 1.01f, 3, new Vector2(-0.5f, 0)),
            new(3, 1.5f, 4, new Vector2(-0.5f, 0)),
            new(3, 1.56f, 16, new Vector2(-0.5f, 0))
        ];

    public override IEnumerator CoWaitForFinish()
    {
        // 1. Setup references and stop initial animations
        var victimObj = victimParts.gameObject;
        var bodyAnim = victimParts.cosmetics.bodySprites._items[0].BodySprite.GetComponent<SpriteAnim>();
        var skinAnim = victimParts.GetSkinSpriteAnim();

        bodyAnim.Speed = 0;
        bodyAnim.Stop();
        skinAnim.Speed = 0;
        skinAnim.Stop();

        Dictionary<int, KillAnimFrame> frames = new();
        int frameCount = 0;

        foreach (var frame in KillFrames)
        {
            for (int idx = frameCount; idx < frameCount + frame.Length; idx++)
            {
                frames[idx] = frame;
            }

            frameCount += frame.Length;
        }

        if (frameCount == 0) yield break;
        frames[frameCount] = frames[frameCount - 1];

        float playhead = 0f;
        float timePerFrame = 2.5f / frames.Count;

        while (playhead < 2.5f)
        {
            playhead += Time.deltaTime;
            int currentPos = Math.Min((int)Math.Floor(playhead / timePerFrame), frames.Count - 1);
            KillAnimFrame currentFrame = frames[currentPos];

            // Sample visuals
            int animation = currentFrame.Animation;
            CosmeticsLayer animCosmetics = HudManager.Instance.KillOverlay.KillAnims[animation].victimParts.cosmetics;
            AnimationClip bodyClip = animCosmetics.bodySprites._items[0].BodySprite.GetComponent<SpriteAnim>()
                .m_defaultAnim;

            SkinViewData skinData = victimParts.cosmetics.skin.skin;
            var skinClip = skinData.KillStabVictim;

            AnimationClip.SampleAnimation(bodyAnim.gameObject, bodyClip, currentFrame.Time, WrapMode.Default);
            if (skinClip)
                AnimationClip.SampleAnimation(skinAnim.gameObject, skinClip, currentFrame.Time, WrapMode.Default);

            victimObj.transform.localPosition = currentFrame.Offset + new Vector2(-1.5f, 0);

            yield return null;
        }
    }
}

public readonly struct KillAnimFrame(int animation, float time, int length, Vector2 offset)
{
    public readonly int Animation = animation;
    public readonly float Time = time;
    public readonly int Length = length;
    public readonly Vector2 Offset = offset;
}