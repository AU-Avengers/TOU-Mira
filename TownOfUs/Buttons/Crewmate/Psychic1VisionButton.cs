using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class Psychic1VisionButton : TownOfUsRoleButton<PsychicRole>
{
    public override string Name => "Vision";
    public override string Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Psychic;

    public override float Cooldown =>
        OptionGroupSingleton<PsychicOptions>.Instance.PsychicCooldown + MapCooldown;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.PsychicSprite;

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
        plr =>
            plr != PlayerControl.LocalPlayer &&
            ((!plr.Data.Disconnected && !plr.Data.IsDead) || plr.Data.Role is MayorRole mayor && mayor.Revealed || Helpers.GetBodyById(plr.PlayerId)) &&
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
                plr2 =>
                    plr2 != PlayerControl.LocalPlayer &&
                    plr2.PlayerId != plr.PlayerId &&
                    (!plr2.HasDied() || plr2.Data.Role is MayorRole mayor && mayor.Revealed  || Helpers.GetBodyById(plr2.PlayerId)) &&
                    (plr2.moveable || plr2.inVent),
                plr2 =>
                {
                    player2Menu.Close();
                    if (plr2 == null)
                    {
                        return;
                    }
                    PsychicRole.RpcVision(PlayerControl.LocalPlayer, plr.PlayerId, plr2.PlayerId);
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