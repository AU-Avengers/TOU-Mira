using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class JanitorCleanButton : TownOfUsRoleButton<JanitorRole, DeadBody>, IAftermathableBodyButton,
    IDiseaseableButton
{
    public override string Name => TouLocale.Get("TouRoleJanitorClean", "Clean");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<JanitorOptions>.Instance.CleanCooldown + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<JanitorOptions>.Instance.CleanDelay + 0.001f;
    public override int MaxUses => (int)OptionGroupSingleton<JanitorOptions>.Instance.MaxClean;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.CleanButtonSprite;

    public DeadBody? CleaningBody { get; set; }

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
    }

    public void AftermathHandler()
    {
        var body = PlayerControl.LocalPlayer.GetNearestDeadBody(Distance);
        if (body == null)
        {
            return;
        }
        CleaningBody = body;

        OnEffectEnd();
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        CleaningBody = Target;
        OverrideName(TouLocale.Get("TouRoleJanitorCleaning", "Cleaning"));
    }

    public override void OnEffectEnd()
    {
        OverrideName(TouLocale.Get("TouRoleJanitorClean", "Clean"));
        if (CleaningBody == Target && CleaningBody != null)
        {
            JanitorRole.RpcCleanBody(PlayerControl.LocalPlayer, CleaningBody.ParentId);
            TouAudio.PlaySound(TouAudio.JanitorCleanSound);
        }

        CleaningBody = null;
        if (OptionGroupSingleton<JanitorOptions>.Instance.ResetCooldowns)
        {
            PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());
        }
    }
}