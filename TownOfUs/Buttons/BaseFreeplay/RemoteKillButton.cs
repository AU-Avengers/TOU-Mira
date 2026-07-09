using MiraAPI.Hud;
using MiraAPI.Networking;
using Reactor.Networking.Rpc;
using TownOfUs.Networking;
using TownOfUs.Modules;
using UnityEngine;
using TownOfUs.Modules.Components;

namespace TownOfUs.Buttons.BaseFreeplay;

public sealed class RemoteKillButton : TownOfUsButton
{
    public override string Name => TouLocale.GetParsed("FreeplayKillButton", "Remote Kill");
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override float EffectDuration => 3;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;

    public override bool ZeroIsInfinite { get; set; } = true;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    public PlayerControl? Killer;
    public PlayerControl? Victim;
    public override bool UsableInDeath => true;

    public override void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        OnClick();
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer &&
               (TutorialManager.InstanceExists || MultiplayerFreeplayMode.Enabled) &&
               !FreeplayButtonsVisibility.Hidden;
    }

    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.NetTransform.Halt();

        if (Minigame.Instance)
        {
            return;
        }

        Killer = null;
        Victim = null;

        var playerMenu = DoublePlayerMenu.Create(TownOfUsColors.Impostor, TouAssets.KillSprite);
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        playerMenu.Begin(
            plr => !plr.Data.Disconnected &&
                   (plr.moveable || plr.inVent),
            (plr1, plr2) =>
            {
                playerMenu.Close();

                Killer = plr1;
                Victim = plr2;
                EffectActive = true;
                Timer = EffectDuration;

                playerMenu.target1 = null;
            }
        );
        foreach (var panel in playerMenu.potentialVictims)
        {
            if (panel.NameText.text != PlayerControl.LocalPlayer.Data.PlayerName)
            {
                panel.NameText.color = Color.white;
            }
        }
    }

    public override void OnEffectEnd()
    {
        if (Killer == null || Victim == null || Victim.HasDied())
        {
            return;
        }

        if (MultiplayerFreeplayMode.Enabled)
        {
            Rpc<MultiplayerFreeplayRequestRpc>.Instance.Send(
                PlayerControl.LocalPlayer,
                new MultiplayerFreeplayRequest(MultiplayerFreeplayAction.RemoteKill, Killer.PlayerId, Victim.PlayerId, 0));
            return;
        }

        Killer.RpcCustomMurder(Victim);
    }
}