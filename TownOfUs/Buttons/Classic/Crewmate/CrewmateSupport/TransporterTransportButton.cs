using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class TransporterTransportButton : TownOfUsRoleButton<TransporterRole>, ILegacyCapable
{
    public override string Name => TouLocale.GetParsed("TouRoleTransporterTransport", "Transport");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Transporter;

    public override float Cooldown =>
        Math.Clamp(OptionGroupSingleton<TransporterOptions>.Instance.TransporterCooldown + MapCooldown, 5f, 120f);

    public override int MaxUses => (int)OptionGroupSingleton<TransporterOptions>.Instance.MaxNumTransports;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyCrewAssets.Transport : TouCrewAssets.Transport;
    public int ExtraUses { get; set; }

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
    }

    private PlayerControl? target1;

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

        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        playerMenu.Begin(
            plr => ((!plr.Data.Disconnected && !plr.Data.IsDead) || Helpers.GetBodyById(plr.PlayerId)) &&
                   (plr.moveable || plr.inVent),
            plr =>
            {
                if (plr == null)
                {
                    return;
                }

                if (target1 == null) // Set first choice
                {
                    target1 = plr;
                    var targetPanel = playerMenu.potentialVictims.First(victim => victim.NameText.text == target1.Data.PlayerName);
                    // set outline for targetPanel
                    return;
                }
                if (target1.PlayerId == plr.PlayerId) // Unselect first choice
                {
                    var targetPanel = playerMenu.potentialVictims.First(victim => victim.NameText.text == target1.Data.PlayerName);
                    // clear outline for targetPanel
                    target1 = null;
                    return;
                }

                playerMenu.Close();

                TransporterRole.RpcTransport(PlayerControl.LocalPlayer, target1.PlayerId, plr.PlayerId);

                target1 = null;
            }
        );
        foreach (var panel in playerMenu.potentialVictims)
        {
            panel.PlayerIcon.cosmetics.SetPhantomRoleAlpha(1f);
            if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
            {
                panel.NameText.color = Color.white;
            }
        }
    }
}