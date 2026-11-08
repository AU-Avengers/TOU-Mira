using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using PowerTools;
using UnityEngine;

namespace TownOfUs.AnimationSystem;

public class CustomKillAnimation(IntPtr cppPtr) : OverlayKillAnimation(cppPtr)
{
    public virtual bool ShouldInheritFromAnimation => true;
    public virtual bool ShouldShowPets => true;
    public virtual void InheritFromAnimation(OverlayKillAnimation animation)
    {
        // Copy data
        Sfx = animation.Sfx;
        Stinger = animation.Stinger;
        StingerVolume = animation.StingerVolume;
        victimHat = animation.victimHat;
        victimParts = animation.victimParts;
        killerParts = animation.killerParts;
        KillerPetPosition = animation.KillerPetPosition;
        VictimPetPosition = animation.VictimPetPosition;
        Destroy(animation);
    }

    public virtual void InitAnimation(KillOverlay overlay, OverlayKillAnimation prefab, NetworkedPlayerInfo killer,
        NetworkedPlayerInfo victim)
    {
        if (ShouldInheritFromAnimation)
        {
            InheritFromAnimation(prefab);
        }

        petObjects = new Il2CppSystem.Collections.Generic.HashSet<GameObject>();
        foreach (SpriteAnim spriteAnim in GetComponentsInChildren<SpriteAnim>())
        {
            if (spriteAnim.GetComponent<PetBehaviour>() != null)
            {
                if (ShouldShowPets)
                {
                    petObjects.Add(spriteAnim.gameObject);
                }
                else
                {
                    spriteAnim.gameObject.SetActive(false);
                }
            }
        }

        overlay.ShowKillAnimation(this, killer, victim);
    }

    public virtual IEnumerator CoWaitForFinish()
    {
        SpriteAnim[] anims = GetComponentsInChildren<SpriteAnim>();
        if (anims.Length == 0)
        {
            yield return new WaitForSeconds(1f);
        }
        else
        {
            for (;;)
            {
                bool flag = false;
                for (int i = 0; i < anims.Length; i++)
                {
                    if (anims[i].IsPlaying())
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    break;
                }

                yield return null;
            }
        }
    }

    public override Il2CppSystem.Collections.IEnumerator CoShow(KillOverlay parent)
    {
        return Enumerator().WrapToIl2Cpp();

        IEnumerator Enumerator()
        {
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(this.Stinger, false, 1f, null).volume = this.StingerVolume;
            }

            parent.background.enabled = true;
            yield return Effects.Wait(0.083333336f);
            parent.background.enabled = false;
            parent.flameParent.SetActive(true);
            parent.flameParent.transform.localScale = new Vector3(1f, 0.3f, 1f);
            parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 25f);
            yield return Effects.Wait(0.083333336f);
            parent.flameParent.transform.localScale = new Vector3(1f, 0.5f, 1f);
            parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, -15f);
            yield return Effects.Wait(0.083333336f);
            parent.flameParent.transform.localScale = new Vector3(1f, 1f, 1f);
            parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            gameObject.SetActive(true);
            yield return CoWaitForFinish();
            gameObject.SetActive(false);
            yield return new WaitForLerp(0.16666667f, new Action<float>(t =>
            {
                parent.flameParent.transform.localScale = new Vector3(1f, 1f - t, 1f);
            }));
            parent.flameParent.SetActive(false);
        }
    }
}