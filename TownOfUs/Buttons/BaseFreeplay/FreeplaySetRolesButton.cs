using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.BaseFreeplay;

public sealed class FreeplaySetRolesButton : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("FreeplaySetRoleButton", "Set Roles");
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override float EffectDuration => 3;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
    public override LoadableAsset<Sprite> Sprite => TouAssets.FreeplayRoleSprite;

    public override bool ZeroIsInfinite { get; set; } = true;
    public override bool UsableInDeath => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer != null &&
               TutorialManager.InstanceExists &&
               !FreeplayButtonsVisibility.Hidden;
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
        PlayerControl.LocalPlayer.NetTransform.Halt();

        if (Minigame.Instance)
        {
            return;
        }

        var player1Menu = CustomPlayerMenu.Create();
        player1Menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        player1Menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        player1Menu.Begin(
            _ => true,
            plr =>
            {
                player1Menu.ForceClose();

                if (plr == null)
                {
                    return;
                }

                var roleMenu = GuesserMenu.Create();
                roleMenu.Begin(IsRoleValid, ClickRoleHandle);

                void ClickRoleHandle(RoleBehaviour role)
                {
                    if (plr.HasDied())
                    {
                        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>()
                            .FirstOrDefault(b => b.ParentId == plr.PlayerId);
                        var position = new Vector2(PlayerControl.LocalPlayer.transform.localPosition.x, PlayerControl.LocalPlayer.transform.localPosition.y);

                        if (body != null)
                        {
                            position = new Vector2(body.transform.localPosition.x, body.transform.localPosition.y + 0.3636f);
                            UnityEngine.Object.Destroy(body.gameObject);
                        }

                        GameHistory.ClearMurder(plr);

                        plr.Revive();

                        plr.transform.position = new Vector2(position.x, position.y);
                    }
                    plr.ChangeRole((ushort)role.Role);
                    roleMenu.ForceClose();
                }
            });

        foreach (var panel in player1Menu.potentialVictims)
        {
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

    private static bool IsRoleValid(RoleBehaviour role)
    {
        if (role is IGhostRole)
        {
            return true;
        }

        if (role.IsDead)
        {
            return false;
        }

        return true;
    }
}