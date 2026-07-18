
using TownOfUs.Options;
using MiraAPI.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons;
using UnityEngine;
using HarmonyLib;


namespace TownOfUs.Modules.DraftMode;

public sealed class DraftRerollButton : TownOfUsButton
{
    public static void Show()
    {
        CustomButtonSingleton<DraftRerollButton>.Instance.Disabled = false;
    }

    public static void Hide()
    {
        CustomButtonSingleton<DraftRerollButton>.Instance.Disabled = true;
    }

    public static void ShowAndReset()
    {
        Show();
        CustomButtonSingleton<DraftRerollButton>.Instance.SetUses(
            (int)OptionGroupSingleton<RoleOptions>.Instance.RerollsPerPlayer.Value);
    }

    public static void HideAndReset()
    {
        Hide();
        CustomButtonSingleton<DraftRerollButton>.Instance.SetUses(
            (int)OptionGroupSingleton<RoleOptions>.Instance.RerollsPerPlayer.Value);
    }

    public override string Name => "Reroll";
    public override float InitialCooldown => 0.001f;
    public override float Cooldown => 0.001f;
    public override int MaxUses => (int)OptionGroupSingleton<RoleOptions>.Instance.RerollsPerPlayer.Value;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override Color TextOutlineColor => new Color32(255, 221, 0, 255);

    public override LoadableAsset<Sprite> Sprite => TouAssets.RetrainCleanSprite;

    public override bool Disabled { get; set; } = true;
    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
    }
    public override bool Enabled(RoleBehaviour role)
    {
        return DraftManager.IsDraftActive && !Disabled && MaxUses > 0 ;
    }

    public override bool CanUse()
    {
        var state = DraftManager.GetStateForPlayer(PlayerControl.LocalPlayer.PlayerId);
        return state != null && state.IsPickingNow && DraftManager.IsDraftActive && !Disabled && MaxUses > 0 && UsesLeft>0;
    }

    protected override void OnClick()
    {
        if (!DraftManager.IsDraftActive) return;
        Helpers.CreateAndShowNotification("Your picks have been rerolled!", Color.white,
                new Vector3(0f, 1f, -80f), spr: TouAssets.RetrainCleanSprite.LoadAsset());
        DraftNetworkHelper.RequestReroll();    
        }

    [HarmonyPatch(typeof(DraftRpcs), nameof(DraftRpcs.RpcStartDraft))]
    public static class ShowDraftRerollButtonOnDraftStart
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Show();
            CustomButtonSingleton<DraftRerollButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.RerollsPerPlayer.Value);
        }
    }


    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastRecap))]
    public static class HideDraftReroll
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftRerollButton.HideAndReset();
        }
    }


    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastCancelDraft))]
    public static class HideDraftRerollOnCancelDraft
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftRerollButton.Hide();
            CustomButtonSingleton<DraftRerollButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.RerollsPerPlayer.Value);
        }
    }


    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastDraftEnd))]
    public static class HideDraftRerollOnDraftEnd
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftRerollButton.Hide();
            CustomButtonSingleton<DraftRerollButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.RerollsPerPlayer.Value);
        }
    }
    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastDraftEnd))]
    public static class HideDraftRerollAfterUsedUp
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftRerollButton.Hide();
            CustomButtonSingleton<DraftRerollButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.RerollsPerPlayer.Value);
        }
    }
}