using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using PowerTools;
using Reactor.Utilities;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Anims;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Networking;
using TownOfUs.Patches;
using UnityEngine;

namespace TownOfUs.AnimationSystem.SpecialKills;

[RegisterInIl2Cpp]
public class DeputyKillAnimation(IntPtr cppPtr) : CustomKillAnimation(cppPtr)
{
    public override bool ShouldInheritFromAnimation => false;
    public override bool ShouldShowPets => false;
    public static KillAnimType PublicKillAnimType { get; } = KillAnimationHelpers.CreateNewKillAnimType();
    public Il2CppReferenceField<SpriteAnim> DeputyOutfitAnim;
    public Il2CppReferenceField<GameObject> DeputyCloseUp;
    public Il2CppReferenceField<SpriteRenderer> HandSprite;
    public Il2CppReferenceField<SpriteRenderer> BodySprite;
    public int ColorId { get; set; }
    // maybe i set up the prefab poorly but THIS SHIT NEVER GETS SET UNLESS IT'S STATIC WHAT THE FUUUUCK
    public static int TargetId { get; set; }

    public static DeputyKillAnimation CreateKillAnimation(NetworkedPlayerInfo killer, NetworkedPlayerInfo victim,
        int playerColorId)
    {
        Transform parent = new GameObject("DeputyKillAnimation").DontUnload().DontDestroy().transform;
        parent.gameObject.SetActive(false);
        var killScreen = Instantiate(TouAssets.DeputyKillOverlayPrefab.LoadAsset(), parent);
        killScreen.transform.localPosition = new Vector3(0.24f, -0.44f, 0);
        killScreen.transform.localScale *= 1.15f;

        var newKillAnim = killScreen.gameObject.GetComponent<DeputyKillAnimation>();
        var dummyAnim =
            CustomTouMurderRpcs.GetKillAnimation(HudManager.Instance.KillOverlay, ExtendedKillAnimType.Stab);
        newKillAnim.ColorId = playerColorId;
        newKillAnim.InitAnimation(HudManager.Instance.KillOverlay, dummyAnim, killer, victim);
        return newKillAnim;
    }

    public override void InitAnimation(KillOverlay overlay, OverlayKillAnimation prefab, NetworkedPlayerInfo killer,
        NetworkedPlayerInfo victim)
    {
        KillType = PublicKillAnimType;
        StingerVolume = prefab.StingerVolume;
        HandSprite.Value.material = PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        var handRgb = HandSprite.Value.gameObject.GetComponent<RainbowBehaviour>()
                ?? HandSprite.Value.gameObject.AddComponent<RainbowBehaviour>();
        handRgb.AddRend(HandSprite.Value, ColorId);
        BodySprite.Value.material = PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        var bodyRgb = BodySprite.Value.gameObject.GetComponent<RainbowBehaviour>()
                      ?? BodySprite.Value.gameObject.AddComponent<RainbowBehaviour>();
        bodyRgb.AddRend(BodySprite.Value, ColorId);
        PlayerMaterial.SetColors(ColorId, HandSprite.Value);
        PlayerMaterial.SetColors(ColorId, BodySprite.Value);
        base.InitAnimation(overlay, prefab, killer, victim);
    }

    public override Il2CppSystem.Collections.IEnumerator CoShow(KillOverlay parent)
    {
        return Enumerator().WrapToIl2Cpp();

        IEnumerator Enumerator()
        {
            if (Constants.ShouldPlaySfx())
            {
                TouAudio.PlaySound(TouAudio.DeputyReveal, StingerVolume);
            }

            parent.background.enabled = true;
            yield return Effects.Wait(0.083333336f);
            parent.background.enabled = false;
            var flameSprite = parent.flameParent.transform.FindChild("BackgroundFlame").GetComponent<SpriteRenderer>();
            flameSprite.sprite = TouAssets.DeputyRevealBg;
            flameSprite.transform.localPosition = new Vector3(0, -2f);
            if (KillOverlayPatch.material == null)
            {
                KillOverlayPatch.material = new Material(flameSprite.material);
            }

            flameSprite.material = KillOverlayPatch.material;
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

            var clonedForeground = Instantiate(BodySprite.Value, BodySprite.Value.transform.parent);
            clonedForeground.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            var clonedHand = Instantiate(HandSprite.Value, HandSprite.Value.transform.parent);
            clonedHand.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            var depCloseUp = DeputyCloseUp.Value;
            var hasFinishedIntro = false;
            yield return new WaitForLerp(1.25f, new Action<float>(t =>
            {
                var adj = t / 200;
                if (depCloseUp)
                {
                    depCloseUp.transform.localPosition += new Vector3(adj, 0f, 0f);
                }

                if (!hasFinishedIntro && !DeputyOutfitAnim.Value.IsPlaying())
                {
                    DeputyOutfitAnim.Value.Play(TouAssets.DeputyKillOutfitLoop.LoadAsset());
                    hasFinishedIntro = true;
                }
            }));
            gameObject.SetActive(false);
            yield return new WaitForLerp(0.16666667f, new Action<float>(t =>
            {
                if (parent.flameParent)
                {
                    parent.flameParent.transform.localScale = new Vector3(1f, 1f - t, 1f);
                }
            }));
            if (flameSprite)
            {
                flameSprite.sprite = TouAssets.KillBG;
                flameSprite.transform.localPosition = new Vector3(0, 0);
            }

            if (parent.flameParent)
            {
                parent.flameParent.SetActive(false);
            }

            var targetVoteArea = MeetingHud.Instance?.playerStates.FirstOrDefault(x => x.PlayerId == TargetId);
            if (targetVoteArea == null)
            {
                Warning($"Missing Vote Area for {TargetId}");
                yield break;
            }

            Info("Animating Vote Area Death");
            Coroutines.Start(CustomTouMurderRpcs.CoAnimateDeath(targetVoteArea, UnityEngine.Random.RandomRangeInt(0, 2),
                true));
        }
    }
}
