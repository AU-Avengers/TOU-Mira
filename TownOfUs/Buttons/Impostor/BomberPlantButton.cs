using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class BomberPlantButton : TownOfUsRoleButton<BomberRole>, IAftermathableButton, IDiseaseableButton
{
    public override string Name => TouLocale.Get("TouRoleBomberPlace", "Place");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => PlayerControl.LocalPlayer.GetKillCooldown() + MapCooldown;
    public override float EffectDuration => OptionGroupSingleton<BomberOptions>.Instance.DetonateDelay;
    public override int MaxUses => (int)OptionGroupSingleton<BomberOptions>.Instance.MaxBombs;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.PlaceSprite;

    public bool Usable { get; set; } = OptionGroupSingleton<BomberOptions>.Instance.CanBombFirstRound ||
                                       TutorialManager.InstanceExists;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override bool CanUse()
    {
        return base.CanUse() && Usable;
    }

    public void AftermathHandler()
    {
        ClickHandler();
    }
    protected override void OnClick()
    {
        OverrideSprite(TouImpAssets.DetonatingSprite.LoadAsset());
        OverrideName(TouLocale.Get("TouRoleBomberDetonating", "Detonating"));

        PlayerControl.LocalPlayer.killTimer = EffectDuration + 1f;

        BomberRole.RpcPlantBomb(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.transform.position);
    }

    public override void OnEffectEnd()
    {
        OverrideSprite(TouImpAssets.PlaceSprite.LoadAsset());
        OverrideName(TouLocale.Get("TouRoleBomberPlace", "Place"));

        PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());

        Role.Bomb?.Detonate();
    }
}