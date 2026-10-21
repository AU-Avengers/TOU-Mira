using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class InquisitorInquireButton : TownOfUsRoleButton<InquisitorRole, PlayerControl>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.InquisitorInquire", "Inquire");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override int MaxUses => (int)OptionGroupSingleton<InquisitorOptions>.Instance.MaxUses;
    public override Color TextOutlineColor => TownOfUsColors.Inquisitor;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<InquisitorOptions>.Instance.InquireCooldown.Value + MapCooldown, 5f, 120f);

    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.InquireSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && !OptionGroupSingleton<InquisitorOptions>.Instance.CantInquire;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<InquisitorInquiredModifier>());
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        if (ModifierUtils.GetActiveModifiers<InquisitorInquiredModifier>().Any())
        {
            ++UsesLeft;
            SetUses(UsesLeft);
        }

        ModifierUtils.GetPlayersWithModifier<InquisitorInquiredModifier>()
            .Do(x => x.RemoveModifier<InquisitorInquiredModifier>());

        Target.AddModifier<InquisitorInquiredModifier>();

        new TouAbilityEvent(AbilityType.InquisitorInquire, PlayerControl.LocalPlayer, Target).InvokeEvent();

        var notif1 = Helpers.CreateAndShowNotification(
            MiraLocaleManager.Get("TownOfUsMira.Role.InquisitorInquireNotif").Replace("<player>", $"{TownOfUsColors.Inquisitor.ToTextColor()}{Target.Data.PlayerName}</color>"),
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Inquisitor);

        notif1.AdjustNotification();
    }
}