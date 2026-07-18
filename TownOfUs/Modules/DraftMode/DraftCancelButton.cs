using MiraAPI.Hud;
using HarmonyLib;
using TownOfUs.Buttons;
using UnityEngine;

namespace TownOfUs.Modules.DraftMode;

public sealed class DraftCancelButton : TownOfUsButton
{
    public static void Show()
    {
        CustomButtonSingleton<DraftCancelButton>.Instance.Disabled = false;
    }

    public static void Hide()
    {
        CustomButtonSingleton<DraftCancelButton>.Instance.Disabled = true;
    }

    public override string Name => "Cancel Draft";
    public override float InitialCooldown => 0.001f;
    public override float Cooldown => 0.001f;

    public override bool ZeroIsInfinite { get; set; } = true;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override Color TextOutlineColor => new Color32(198, 22, 22, 255);

    public override LoadableAsset<Sprite> Sprite => TouImpAssets.MarkSprite;

    public override bool Disabled { get; set; } = true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return AmongUsClient.Instance.AmHost && !Disabled;
    }

    public override bool CanUse()
    {
        return base.CanUse() && DraftManager.IsDraftActive && !Disabled;
    }

    protected override void OnClick()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (!DraftManager.IsDraftActive) return;
        DraftEngineBehaviour.Instance?.CancelDraft();
        DraftRpcs.RpcCreateNotif(PlayerControl.LocalPlayer,"<color=#FF0000>Draft Mode</color> has been cancelled by the <color=#FFBFCC><b>Host</b></color>!");
        DraftNetworkHelper.BroadcastDraftEnd();
        Hide();
    }
}

[HarmonyPatch(typeof(DraftRpcs), nameof(DraftRpcs.RpcStartDraft))]
public static class ShowCancelButtonOnDraftStart
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        DraftCancelButton.Show();
    }
}

[HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastRecap))]
public static class HideCancelButtonOnRecap
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        DraftCancelButton.Hide();
    }
}

[HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastCancelDraft))]
public static class HideCancelButtonOnCancelDraft
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        DraftCancelButton.Hide();
    }
}

[HarmonyPatch(typeof(DraftNetworkHelper), nameof(DraftNetworkHelper.BroadcastDraftEnd))]
public static class HideCancelButtonOnDraftEnd
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        DraftCancelButton.Hide();
    }
}
