using System.Collections;
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
    public override void InitAnimation(KillOverlay overlay, OverlayKillAnimation prefab, NetworkedPlayerInfo killer, NetworkedPlayerInfo victim)
    {
        KillType = PublicKillAnimType;
        base.InitAnimation(overlay, prefab, killer, victim);
    }
    public override IEnumerator CoWaitForFinish()
    {
        //yield return new WaitForSeconds(3.25f);
        yield return new WaitForSeconds(300f);
    }
}