using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class BomberPlantButton : TownOfUsKillRoleButton<BomberRole>, IAftermathableButton, IDiseaseableButton, ILegacyCapable
{
    public override string Name => MiraLocaleManager.Get("TouRoleBomberPlace", "Place");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => PlayerControl.LocalPlayer.GetKillCooldown();
    public override float EffectDuration => OptionGroupSingleton<BomberOptions>.Instance.DetonateDelay;
    public override int MaxUses => (int)OptionGroupSingleton<BomberOptions>.Instance.MaxBombs;
    public override bool ZeroIsInfinite => true;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyImpAssets.PlantSprite : TouImpAssets.PlaceSprite;
    public override bool UsableFirstRound => OptionGroupSingleton<BomberOptions>.Instance.CanBombFirstRound;

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public void AftermathHandler()
    {
        ClickHandler();
    }
    protected override void OnClick()
    {
        OverrideSprite(LegacyAssets.IsLegacy ? LegacyImpAssets.DetonatingSprite.LoadAsset() : TouImpAssets.DetonatingSprite.LoadAsset());
        OverrideName(MiraLocaleManager.Get("TouRoleBomberDetonating", "Detonating"));

        PlayerControl.LocalPlayer.killTimer = EffectDuration + 1f;

        BomberRole.RpcPlantBomb(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.transform.position);
    }

    public override void OnEffectEnd()
    {
        OverrideSprite(LegacyAssets.IsLegacy ? LegacyImpAssets.PlantSprite.LoadAsset() : TouImpAssets.PlaceSprite.LoadAsset());
        OverrideName(MiraLocaleManager.Get("TouRoleBomberPlace", "Place"));

        PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.LocalPlayer.GetKillCooldown());

        Role.Bomb?.Detonate();
    }
}