using HarmonyLib;
using MiraAPI.GameOptions;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Patches.Options;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
public static class DeadSeeVoteColorsPatch
{
    public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] NetworkedPlayerInfo voterPlayer,
        [HarmonyArgument(1)] int index, [HarmonyArgument(2)] Transform parent)
    {
        var spriteRenderer = Object.Instantiate(__instance.PlayerVotePrefab);
        var player = MiscUtils.PlayerById(voterPlayer.PlayerId);
        if (PlayerControl.LocalPlayer.Data.Role is ProsecutorRole)
        {
            PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
        }
        else if (player != null && player.Data.Role is ProsecutorRole pros && pros.HasProsecuted &&
                 !PlayerControl.LocalPlayer.Data.IsDead)
        {
            PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
        }
        else if (GameOptionsManager.Instance.currentNormalGameOptions.AnonymousVotes &&
                 (!OptionGroupSingleton<PostmortemOptions>.Instance.DeadSeeVotes || !PlayerControl.LocalPlayer.Data.IsDead))
        {
            PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
        }
        else
        {
            PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
        }

        spriteRenderer.transform.SetParent(parent);
        spriteRenderer.transform.localScale = Vector3.zero;
        var component = parent.GetComponent<PlayerVoteArea>();
        if (component != null)
        {
            spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
        }

        __instance.StartCoroutine(Effects.Bloop(index * 0.3f, spriteRenderer.transform));
        parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
        return false;
    }
}