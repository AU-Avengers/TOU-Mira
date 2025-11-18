using MiraAPI.Hud;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Buttons.Freeplay;

public sealed class RemoteKillButton : TownOfUsButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => 0.001f;
    public override float InitialCooldown => 0.001f;
    public override float EffectDuration => 3;
    public override ButtonLocation Location => ButtonLocation.BottomLeft;
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
        return PlayerControl.LocalPlayer != null &&
               TutorialManager.InstanceExists;
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

        var player1Menu = CustomPlayerMenu.Create();
        player1Menu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        player1Menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        player1Menu.Begin(
            plr => !plr.Data.Disconnected && !plr.Data.IsDead &&
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
                    plr.cosmetics.currentBodySprite.BodySprite.material;
                player2Menu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
                    plr.cosmetics.currentBodySprite.BodySprite.material;

                player2Menu.Begin(
                    plr2 => plr2.PlayerId != plr.PlayerId &&
                            !plr.Data.Disconnected && !plr.Data.IsDead &&
                            (plr2.moveable || plr2.inVent),
                    plr2 =>
                    {
                        player2Menu.Close();
                        if (plr2 == null)
                        {
                            return;
                        }

                        Killer = plr;
                        Victim = plr2;
                        EffectActive = true;
                        Timer = EffectDuration;
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

    public override void OnEffectEnd()
    {
        if (Killer == null || Victim == null || Killer.HasDied() || Victim.HasDied())
        {
            return;
        }
        Killer.RpcCustomMurder(Victim);
    }
}