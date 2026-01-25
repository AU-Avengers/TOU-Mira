using System.Collections;
using HarmonyLib;
using MiraAPI.Modifiers;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Impostor;
using UnityEngine;

namespace TownOfUs.Patches.Misc;

// used for jailer and blackmailed players.
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingShhPatches
{
    public static void Postfix(MeetingHud __instance)
    {
        if (PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.Data.IsDead &&
            (PlayerControl.LocalPlayer.HasModifier<BlackmailedModifier>() ||
             PlayerControl.LocalPlayer.TryGetModifier<JailedModifier>(out var jailMod) && jailMod.IsJailorValid))
        {
            Coroutines.Start(MeetingShhh());
        }
    }

    public static IEnumerator MeetingShhh()
    {
        yield return HudManager.Instance.CoFadeFullScreen(Color.clear, new Color(0f, 0f, 0f, 0.98f));
        var tempPosition = HudManager.Instance.shhhEmblem.transform.localPosition;
        var tempDuration = HudManager.Instance.shhhEmblem.HoldDuration;
        HudManager.Instance.shhhEmblem.transform.localPosition = new Vector3(
            HudManager.Instance.shhhEmblem.transform.localPosition.x,
            HudManager.Instance.shhhEmblem.transform.localPosition.y,
            HudManager.Instance.FullScreen.transform.position.z + 1f);
        var jailCell = new GameObject("jailCell");
        if (PlayerControl.LocalPlayer.TryGetModifier<JailedModifier>(out var jailMod) && jailMod.IsJailorValid)
        {
            jailCell.transform.SetParent(HudManager.Instance.shhhEmblem!.transform);
            jailCell.transform.localPosition =
                new Vector3(0, 0, HudManager.Instance.shhhEmblem.Hand.transform.localPosition.z);
            jailCell.transform.localScale = new Vector3(0.83f, 0.83f, 1f);
            jailCell.gameObject.layer = HudManager.Instance.shhhEmblem!.gameObject.layer;

            var render = jailCell.AddComponent<SpriteRenderer>();
            render.sprite = TouAssets.JailCellSprite.LoadAsset();
            jailCell.gameObject.SetActive(true);
            jailCell.GetComponent<SpriteRenderer>().enabled = true;
            HudManager.Instance.shhhEmblem.TextImage.text = PlayerControl.LocalPlayer.HasModifier<BlackmailedModifier>()
                ? "<size=55%>YOU ARE JAILED</size><size=40%>\nAND BLACKMAILED</size>"
                : "YOU ARE JAILED!";
            HudManager.Instance.shhhEmblem.Hand.gameObject.SetActive(false);
        }
        else
        {
            HudManager.Instance.shhhEmblem.TextImage.text = "YOU ARE BLACKMAILED!";
        }

        HudManager.Instance.shhhEmblem.HoldDuration = 2.5f;
        yield return HudManager.Instance.ShowEmblem(true);
        HudManager.Instance.shhhEmblem.transform.localPosition = tempPosition;
        HudManager.Instance.shhhEmblem.HoldDuration = tempDuration;
        yield return HudManager.Instance.CoFadeFullScreen(new Color(0f, 0f, 0f, 0.98f), Color.clear);
        jailCell.Destroy();
        yield return null;
    }
}