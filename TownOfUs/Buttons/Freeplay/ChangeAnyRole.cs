using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Components;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Freeplay;

public sealed class GhangeAnyRole : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("TouRoleTraitorChangeRole", "Change Role");
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override float EffectDuration => 3;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.CameraSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               TutorialManager.InstanceExists;
    }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
    }

    protected override void OnClick()
    {

        var player1Menu = CustomPlayerMenu.Create();
        player1Menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        player1Menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        player1Menu.Begin(
            plr => true,
            plr =>
            {
                player1Menu.ForceClose();

                if (plr == null)
                {
                    return;
                }

                var roleMenu = GuesserMenu.Create();
                roleMenu.Begin(IsRoleValid, ClickRoleHandle, IsModifierValid, ClickModifierHandle);

                void ClickRoleHandle(RoleBehaviour role)
                {
                    if (plr.HasDied())
                    {
                        plr.Revive();
                    }
                    plr.ChangeRole((ushort)role.Role);
                    roleMenu.ForceClose();
                }

                void ClickModifierHandle(BaseModifier modifier)
                {
                    // Ignored
                }
            });

        foreach (var panel in player1Menu.potentialVictims)
        {
            panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
            if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
            {
                panel.NameText.color = Color.white;
            }
        }
    }

    public override void OnEffectEnd()
    {
        if (!PlayerControl.LocalPlayer.HasDied())
        {
            PlayerControl.LocalPlayer.RpcCustomMurder(PlayerControl.LocalPlayer);
        }
    }

    private bool IsRoleValid(RoleBehaviour role)
    {
        if (role.IsDead)
        {
            return false;
        }

        var options = OptionGroupSingleton<AssassinOptions>.Instance;

        if (role is IGhostRole)
        {
            return false;
        }
        return true;
    }

    private static bool IsModifierValid(BaseModifier modifier)
    {
        return false;
    }
}