using System.Collections;
using PowerTools;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Networking;
using UnityEngine;

namespace TownOfUs.AnimationSystem.SpecialKills;

[RegisterInIl2Cpp]
public class AmbushKillAnimation(IntPtr cppPtr) : CustomKillAnimation(cppPtr)
{
    public override bool ShouldInheritFromAnimation => true;
    public override bool ShouldShowPets => false;
    public static KillAnimType PublicKillAnimType { get; } = KillAnimationHelpers.CreateNewKillAnimType();
    public SpriteAnim AmbushAnim { get; set; }
    public GameObject AmbushAnimHolder { get; set; }

    public override void InheritFromAnimation(OverlayKillAnimation animation)
    {
        animation.killerParts.gameObject.SetActive(false);
        base.InheritFromAnimation(animation);
        victimParts.transform.localPosition = new Vector3(-0.8f, 0.48f, -0.5f);
    }

    public static AmbushKillAnimation CreateKillAnimation(NetworkedPlayerInfo killer, NetworkedPlayerInfo victim)
    {
        Transform parent = new GameObject("AmbushKillAnimation").DontUnload().DontDestroy().transform;
        parent.gameObject.SetActive(false);
        OverlayKillAnimation original = Instantiate(CustomTouMurderRpcs.GetKillAnimation(HudManager.Instance.KillOverlay, ExtendedKillAnimType.Werewolf_Slash), parent);

        var newKillAnim = original.gameObject.AddComponent<AmbushKillAnimation>();
        newKillAnim.InitAnimation(HudManager.Instance.KillOverlay, original, killer, victim);
        return newKillAnim;
    }

    public override void InitAnimation(KillOverlay overlay, OverlayKillAnimation prefab, NetworkedPlayerInfo killer,
        NetworkedPlayerInfo victim)
    {
        KillType = PublicKillAnimType;
        base.InitAnimation(overlay, prefab, killer, victim);
    }

    public override IEnumerator CoWaitForFinish()
    {
        yield return new WaitForSeconds(0.1f);
        AmbushAnimHolder = Instantiate(TouAssets.AmbushKillOverlayPrefab.LoadAsset(), victimParts.transform.parent);
        AmbushAnim = AmbushAnimHolder.transform.GetChild(0).GetComponent<SpriteAnim>();
        AmbushAnimHolder.transform.localPosition = new Vector3(0.4f, 0.1f, -0.51f);
        yield return new WaitForSeconds(0.8f);
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 0.8f);
        }
        AmbushAnimHolder.transform.localPosition = new Vector3(0.6f, 0, -0.51f);
        yield return new WaitForSeconds(1.5f);
    }
}