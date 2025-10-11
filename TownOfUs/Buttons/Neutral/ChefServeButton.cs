using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Neutral;

public sealed class ChefServeButton : TownOfUsRoleButton<ChefRole, PlayerControl>
{
    public override string Name => TouLocale.Get("TouRoleChefServe", "Serve");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Chef;
    public override float Cooldown => OptionGroupSingleton<ChefOptions>.Instance.ServeCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.ChefServeEmptySprite;

    public void UpdateServingType()
    {
        var sprite = TouNeutAssets.ChefServeSprites[0].LoadAsset();
        if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.Role is ChefRole && Role.StoredBodies.Count > 0)
        {
            sprite = TouNeutAssets.ChefServeSprites[(int)Role.StoredBodies[0].Value].LoadAsset();
        }
        OverrideSprite(sprite);
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        if (KeybindIcon != null)
        {
            KeybindIcon.transform.localPosition = new Vector3(0.4f, 0.45f, -9f);
        }
        UpdateServingType();
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (playerControl.Data.Role is ChefRole chefRole)
        {
            Button?.usesRemainingText.gameObject.SetActive(true);
            Button?.usesRemainingSprite.gameObject.SetActive(true);
            Button!.usesRemainingText.text = $"{chefRole.StoredBodies.Count}";
        }
    }

    public override bool CanUse()
    {
        return base.CanUse() && Role.StoredBodies.Count != 0;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Logger<TownOfUsPlugin>.Error("Chef Serve: Target is null");
            return;
        }

        ChefRole.RpcServeBody(PlayerControl.LocalPlayer, Target);
        UpdateServingType();
        if (OptionGroupSingleton<ChefOptions>.Instance.ResetCooldowns)
        {
            CustomButtonSingleton<ChefCookButton>.Instance.ResetCooldownAndOrEffect();
        }
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance,
            predicate: x => !x.HasModifier<ChefServedModifier>());
    }
}