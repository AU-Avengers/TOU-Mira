
using TownOfUs.Options;
using MiraAPI.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons;
using UnityEngine;
using HarmonyLib;


namespace TownOfUs.Modules.DraftMode;

public sealed class DraftReshuffleButton : TownOfUsButton
{
    public static void Show()
    {
        CustomButtonSingleton<DraftReshuffleButton>.Instance.Disabled = false;
    }

    public static void Hide()
    {
        CustomButtonSingleton<DraftReshuffleButton>.Instance.Disabled = true;
    }

    public static void ShowAndReset()
    {
        Show();
        CustomButtonSingleton<DraftReshuffleButton>.Instance.SetUses(
            (int)OptionGroupSingleton<RoleOptions>.Instance.ReshufflesPerPlayer.Value);
    }

    public static void HideAndReset()
    {
        Hide();
        CustomButtonSingleton<DraftReshuffleButton>.Instance.SetUses(
            (int)OptionGroupSingleton<RoleOptions>.Instance.ReshufflesPerPlayer.Value);
    }

    public override string Name => "Reshuffle";
    public override float InitialCooldown => 0.001f;
    public override float Cooldown => 0.001f;
    public override int MaxUses => (int)OptionGroupSingleton<RoleOptions>.Instance.ReshufflesPerPlayer.Value;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override Color TextOutlineColor => TownOfUsColors.Inquisitor;

    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.InquireSprite;

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
        Helpers.CreateAndShowNotification("Your picks have been Reshuffleed!", Color.white,
                new Vector3(0f, 1f, -80f), spr: TouNeutAssets.InquireSprite.LoadAsset());
        DraftNetworkHelper.RequestReshuffle();    
        }

    [HarmonyPatch(typeof(DraftRpcs), nameof(DraftRpcs.RpcStartDraft))]
    public static class ShowDraftReshuffleButtonOnDraftStart
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Show();
            CustomButtonSingleton<DraftReshuffleButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.ReshufflesPerPlayer.Value);
        }
    }


    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastRecap))]
    public static class HideDraftReshuffle
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftReshuffleButton.HideAndReset();
        }
    }


    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastCancelDraft))]
    public static class HideDraftReshuffleOnCancelDraft
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftReshuffleButton.Hide();
            CustomButtonSingleton<DraftReshuffleButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.ReshufflesPerPlayer.Value);
        }
    }


    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastDraftEnd))]
    public static class HideDraftReshuffleOnDraftEnd
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftReshuffleButton.Hide();
            CustomButtonSingleton<DraftReshuffleButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.ReshufflesPerPlayer.Value);
        }
    }
    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastDraftEnd))]
    public static class HideDraftReshuffleAfterUsedUp
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftReshuffleButton.Hide();
            CustomButtonSingleton<DraftReshuffleButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.ReshufflesPerPlayer.Value);
        }
    }
}