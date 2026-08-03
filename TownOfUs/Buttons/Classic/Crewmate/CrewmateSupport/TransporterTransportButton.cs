using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Modules.Components;
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

        var playerMenu = DoublePlayerMenu.Create(TownOfUsColors.Transporter, TouCrewAssets.Transport);
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        playerMenu.Begin(
            plr => ((!plr.Data.Disconnected && !plr.Data.IsDead && (plr.moveable || plr.inVent)) ||
                    Helpers.GetBodyById(plr.PlayerId)) || MiscUtils.GetFreshStonedPlayerById(plr.PlayerId),
            (plr1, plr2) =>
            {
                playerMenu.Close();

                TransporterRole.RpcTransport(PlayerControl.LocalPlayer, plr1.PlayerId, plr2.PlayerId);

                playerMenu.target1 = null;
            },
            MouseOutEvent,
            MouseOverEvent
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
    private static void MouseOutEvent(SpriteRenderer highlight, SpriteRenderer icon, bool isSelected)
    {
        highlight.color = isSelected ? new Color32(0, 237, 255, 175) : new Color32(255, 255, 255, 0);
        icon.enabled = isSelected;
    }
    private static void MouseOverEvent(SpriteRenderer highlight, SpriteRenderer icon, bool isSelected)
    {
        highlight.color = isSelected ? new Color32(0, 237, 255, 255) : new Color32(0, 237, 255, 200);
        icon.enabled = true;
    }
}