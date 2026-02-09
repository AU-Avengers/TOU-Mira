using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers.Impostor.Herbalist;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class HerbalistAbilityButton : TownOfUsRoleButton<HerbalistRole, PlayerControl>
{
    public override string Name => "Kill";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<HerbalistOptions>.Instance.HerbCooldown + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => ProtectionButtons[0];
    public HerbAbilities CurrentAbility = HerbAbilities.Kill;

    public static List<LoadableAsset<Sprite>> ProtectionButtons { get; set; } = new()
    {
        TouAssets.KillSprite,
        TouImpAssets.BlackmailSprite,
        TouImpAssets.HypnotiseButtonSprite,
        //TouImpAssets.FlashSprite,
        TouCrewAssets.BarrierSprite,
    };

    public static List<string> ProtectionText { get; set; } = new()
    {
        "Kill",
        "Expose",
        "Confuse",
        //"Glamour",
        "Protect",
    };

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }
        switch (CurrentAbility)
        {
            case HerbAbilities.Kill:
                PlayerControl.LocalPlayer.RpcCustomMurder(Target, MeetingCheck.OutsideMeeting);
                break;
            case HerbAbilities.Expose:
                Target.RpcAddModifier<HerbalistExposedModifier>(PlayerControl.LocalPlayer);
                break;
            case HerbAbilities.Confuse:
                Target.RpcAddModifier<HerbalistConfusedModifier>(PlayerControl.LocalPlayer);
                break;
            case HerbAbilities.Protect:
                Target.RpcAddModifier<HerbalistProtectionModifier>(PlayerControl.LocalPlayer);
                break;
        }
    }

    public void CycleAbility()
    {
        var stepUp = (HerbAbilities)((int)CurrentAbility + 1);
        if (Enum.IsDefined(stepUp))
        {
            CurrentAbility = stepUp;
        }
        else
        {
            CurrentAbility = HerbAbilities.Kill;
        }
        OverrideSprite(ProtectionButtons[(int)CurrentAbility].LoadAsset());
        OverrideName(ProtectionText[(int)CurrentAbility]);
    }
    
    private static Func<HerbalistExposedModifier, bool> ExposedPredicate { get; } =
        msModifier => msModifier.Herbalist.AmOwner;
    
    private static Func<HerbalistConfusedModifier, bool> ConfusedPredicate { get; } =
        msModifier => msModifier.Herbalist.AmOwner;
    
    private static Func<HerbalistProtectionModifier, bool> ProtectedPredicate { get; } =
        msModifier => msModifier.Herbalist.AmOwner;

    public override PlayerControl? GetTarget()
    {
        if (CurrentAbility is HerbAbilities.Expose)
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsImpostorAligned() && !x.HasModifier(ExposedPredicate));
        }
        if (CurrentAbility is HerbAbilities.Confuse)
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsImpostorAligned() && !x.HasModifier(ConfusedPredicate));
        }
        if (CurrentAbility is HerbAbilities.Protect)
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.HasModifier(ProtectedPredicate));
        }
        return MiscUtils.GetImpostorTarget(Distance);
    }
}
