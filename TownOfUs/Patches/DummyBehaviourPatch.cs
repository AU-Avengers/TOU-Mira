using System.Collections;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using TownOfUs.Modifiers.Game;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TownOfUs.Patches;

[HarmonyPatch(typeof(DummyBehaviour))]
public static class DummyBehaviourPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(DummyBehaviour.Start))]
    public static void DummyStartPatch(DummyBehaviour __instance)
    {
        var dum = __instance.myPlayer;
        Coroutines.Start(TouDummyMode(dum));
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DummyBehaviour.Update))]
    public static bool DummyUpdatePatch(DummyBehaviour __instance)
    {
        NetworkedPlayerInfo data = __instance.myPlayer.Data;
        if (data == null || data.IsDead) return false;
        
        if (MeetingHud.Instance)
        {
            // Decided to add this check here, so during a tribunal dummies vote with the player
            // My attempts in making them vote correctly failed due to how the tribunal voting is handled
            if (MarshalRole.TribunalHappening)
            {
                var localVoteData = PlayerControl.LocalPlayer.GetVoteData();
                if (localVoteData.Votes.Count == 0) return false;
                
                MeetingHud.Instance.CmdCastVote(data.PlayerId, localVoteData.Votes[0].Suspect);
                __instance.voted = true;
                return false;
            }
            
            if (!__instance.voted)
            {
                // This was made to work with tribunals, now it's just a better version of the vanilla method lol
                Coroutines.Start(CustomDoVote(__instance));
                __instance.voted = true;
            }
        }
        else
        {
            __instance.voted = false;
        }

        return false;
    }

    private static IEnumerator CustomDoVote(DummyBehaviour dummy)
    {
        if (dummy.voted) yield break;
        yield return new WaitForSeconds(dummy.voteTime.Next());

        List<byte> potentialSuspects = new();
        potentialSuspects.AddRange(Helpers.GetAlivePlayers()
            .Where(p => p != dummy.myPlayer)
            .Select(p => p.PlayerId));

        if (CanSkip(dummy))
        {
            potentialSuspects.Add(253); // the skip button
        }

        if (dummy.myPlayer.GetVoteData().VotesRemaining > 0)
        {
            MeetingHud.Instance.CmdCastVote(dummy.myPlayer.PlayerId, potentialSuspects.Random());
        }
    }

    private static bool CanSkip(DummyBehaviour dummy)
    {
        return !MarshalRole.TribunalHappening;
    }

    private static IEnumerator TouDummyMode(PlayerControl dummy)
    {
        while (PlayerControl.LocalPlayer == null)
        {
            yield return null;
        }

        while (PlayerControl.LocalPlayer.Data == null)
        {
            yield return null;
        }

        while (PlayerControl.LocalPlayer.Data.Role == null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.01f + 0.01f * dummy.PlayerId);
        var roleList = MiscUtils.AllRoles
            .Where(role => role is ICustomRole)
            .Where(role => !role.IsImpostor())
            .Where(role => role is not NeutralGhostRole)
            .Where(role => role is not CrewmateGhostRole)
            .Where(role => role is not ImpostorGhostRole)
            .Where(role => !role.TryCast<CrewmateGhostRole>())
            .Where(role => !role.TryCast<ImpostorGhostRole>())
            .ToList();

        PlayerControl.AllPlayerControls
            .ToArray()
            .Where(player => roleList.Contains(player.Data.Role))
            .ToList()
            .ForEach(player => roleList.Remove(player.Data.Role));

        var roleType = RoleId.Get(roleList.Random()!.GetType());
        dummy.RpcChangeRole(roleType);

        dummy.RpcSetName(AccountManager.Instance.GetRandomName());
        
        var palette = Palette.PlayerColors;
        var validColors = palette.Select(c => palette.IndexOf(c)).Where(id => PlayerControl.LocalPlayer.cosmetics.ColorId != id).ToArray();
        var random = Random.Range(0, validColors.Length);
        var colorId = validColors[random];

        dummy.SetSkin(HatManager.Instance.allSkins[Random.Range(0, HatManager.Instance.allSkins.Count)].ProdId, 0);
        dummy.SetNamePlate(HatManager.Instance
            .allNamePlates[Random.RandomRangeInt(0, HatManager.Instance.allNamePlates.Count)].ProdId);
        dummy.SetPet(HatManager.Instance.allPets[Random.RandomRangeInt(0, HatManager.Instance.allPets.Count)].ProdId);
        dummy.SetColor(colorId);
        dummy.SetHat(HatManager.Instance.allHats[Random.RandomRangeInt(0, HatManager.Instance.allHats.Count)].ProdId,
            colorId);
        dummy.SetVisor(
            HatManager.Instance.allVisors[Random.RandomRangeInt(0, HatManager.Instance.allVisors.Count)].ProdId,
            colorId);

        var randomUniMod = MiscUtils.AllModifiers.Where(x =>
            x is UniversalGameModifier touGameMod && touGameMod.IsModifierValidOn(dummy.Data.Role)).Random();
        if (randomUniMod != null)
        {
            dummy.RpcAddModifier(randomUniMod.GetType());
        }

        var randomTeamMod = MiscUtils.AllModifiers
            .Where(x => x is TouGameModifier touGameMod && touGameMod.IsModifierValidOn(dummy.Data.Role)).Random();
        if (randomTeamMod != null)
        {
            dummy.RpcAddModifier(randomTeamMod.GetType());
        }
    }
}