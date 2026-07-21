
using TownOfUs.Options;
using MiraAPI.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using TownOfUs.Buttons;
using UnityEngine;
using HarmonyLib;


namespace TownOfUs.Modules.DraftMode;

public sealed class DraftShuffleButton : TownOfUsButton
{
    public static void Show()
    {
        CustomButtonSingleton<DraftShuffleButton>.Instance.Disabled = false;
    }

    public static void Hide()
    {
        CustomButtonSingleton<DraftShuffleButton>.Instance.Disabled = true;
    }

    public static void ShowAndReset()
    {
        Show();
        CustomButtonSingleton<DraftShuffleButton>.Instance.SetUses(
            (int)OptionGroupSingleton<RoleOptions>.Instance.ShufflesPerPlayer.Value);
    }

    public static void HideAndReset()
    {
        Hide();
        CustomButtonSingleton<DraftShuffleButton>.Instance.SetUses(
            (int)OptionGroupSingleton<RoleOptions>.Instance.ShufflesPerPlayer.Value);
    }

    public override string Name => TouLocale.GetParsed("TouDraftShuffleButton", "Shuffle");
    public override float InitialCooldown => 0.001f;
    public override float Cooldown => 0.001f;
    public override int MaxUses => (int)OptionGroupSingleton<RoleOptions>.Instance.ShufflesPerPlayer.Value;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;

    public override LoadableAsset<Sprite> Sprite => TouImpAssets.TraitorSelect;

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
        Helpers.CreateAndShowNotification(TouLocale.GetParsed("TouDraftShuffledNotif", "Your picks have been Shuffled!"), Color.white,
                new Vector3(0f, 1f, -80f), spr: TouImpAssets.TraitorSelect.LoadAsset());
        DraftNetworkHelper.RequestShuffle();    
        }

    [HarmonyPatch(typeof(DraftRpcs), nameof(DraftRpcs.RpcStartDraft))]
    public static class ShowDraftShuffleButtonOnDraftStart
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Show();
            CustomButtonSingleton<DraftShuffleButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.ShufflesPerPlayer.Value);
        }
    }


    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastRecap))]
    public static class HideDraftShuffle
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftShuffleButton.HideAndReset();
        }
    }


    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastCancelDraft))]
    public static class HideDraftShuffleOnCancelDraft
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftShuffleButton.Hide();
            CustomButtonSingleton<DraftShuffleButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.ShufflesPerPlayer.Value);
        }
    }


    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastDraftEnd))]
    public static class HideDraftShuffleOnDraftEnd
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftShuffleButton.Hide();
            CustomButtonSingleton<DraftShuffleButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.ShufflesPerPlayer.Value);
        }
    }
    [HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastDraftEnd))]
    public static class HideDraftShuffleAfterUsedUp
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            DraftShuffleButton.Hide();
            CustomButtonSingleton<DraftShuffleButton>.Instance.SetUses((int)OptionGroupSingleton<RoleOptions>.Instance.ShufflesPerPlayer.Value);
        }
    }
}