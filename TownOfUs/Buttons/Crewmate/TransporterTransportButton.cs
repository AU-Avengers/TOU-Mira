using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class TransporterTransportButton : TownOfUsRoleButton<TransporterRole>
{
    public override string Name => TouLocale.Get("TouRoleTransporterTransport", "Transport");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Transporter;

    public override float Cooldown =>
        OptionGroupSingleton<TransporterOptions>.Instance.TransporterCooldown + MapCooldown;

    public override int MaxUses => (int)OptionGroupSingleton<TransporterOptions>.Instance.MaxNumTransports;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.Transport;
    public int ExtraUses { get; set; }

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
        if (!OptionGroupSingleton<TransporterOptions>.Instance.MoveWithMenu)
        {
            PlayerControl.LocalPlayer.NetTransform.Halt();
        }

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
            plr => ((!plr.Data.Disconnected && !plr.Data.IsDead) || Helpers.GetBodyById(plr.PlayerId)) &&
                   (plr.moveable || plr.inVent),
            plr =>
            {
                player1Menu.ForceClose();

                if (plr == null)
                {
                    return;
                }

                var player2Menu = CustomPlayerMenu.Create();
                player2Menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
                    PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
                player2Menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
                    PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

                player2Menu.Begin(
                    plr2 => plr2.PlayerId != plr.PlayerId &&
                            (!plr2.HasDied() ||
                             Helpers.GetBodyById(plr2.PlayerId) /*  || MiscUtils.GetFakePlayer(plr2)?.body */) &&
                            (plr2.moveable || plr2.inVent),
                    plr2 =>
                    {
                        player2Menu.Close();
                        if (plr2 == null)
                        {
                            return;
                        }
                        TransporterRole.RpcTransport(PlayerControl.LocalPlayer, plr.PlayerId, plr2.PlayerId);
                    }
                );
                foreach (var panel in player2Menu.potentialVictims)
                {
                    panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
                    if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
                    {
                        panel.NameText.color = Color.white;
                    }
                }
            }
        );
        foreach (var panel in player1Menu.potentialVictims)
        {
            panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
            if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
            {
                panel.NameText.color = Color.white;
            }
        }
    }
}