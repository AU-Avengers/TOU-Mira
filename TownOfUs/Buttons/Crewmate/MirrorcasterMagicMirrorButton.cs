using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Buttons.Crewmate;

public sealed class MirrorcasterMagicMirrorButton : TownOfUsRoleButton<MirrorcasterRole>, IAftermathableButton
{
    public override string Name => TouLocale.Get("TouRoleMirrorcasterMagicMirror", "Magic Mirror");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Mirrorcaster;

    public override float Cooldown =>
        OptionGroupSingleton<MirrorcasterOptions>.Instance.MirrorCooldown.Value + MapCooldown + 0.001f;

    public override float EffectDuration => OptionGroupSingleton<MirrorcasterOptions>.Instance.MirrorDuration.Value;
    public override int MaxUses => (int)OptionGroupSingleton<MirrorcasterOptions>.Instance.MaxMirrors;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.MagicMirrorSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;
    public override bool ShouldPauseInVent => false;
    public bool TargetWasValid { get; set; }

    public override bool CanUse()
    {
        return base.CanUse() && Role is { Protected: null } &&
               (OptionGroupSingleton<MirrorcasterOptions>.Instance.MultiUnleash || Role.UnleashesAvailable <= 0) &&
               !EffectActive;
    }

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
    }

    public void AftermathHandler()
    {
        var player = PlayerControl.AllPlayerControls.ToArray().Where(plr => !plr.HasDied()).Random();
        if (player == null)
        {
            return;
        }

        MirrorcasterRole.RpcMagicMirror(PlayerControl.LocalPlayer, player);
        EffectActive = true;
        Timer = EffectDuration;
        OverrideName(TouLocale.Get("TouRoleMirrorcasterMagicMirrorProtecting", "Protecting"));
        TargetWasValid = true;
    }

    protected override void OnClick()
    {
        /*if (!OptionGroupSingleton<GlitchOptions>.Instance.MoveWithMenu)
        {
            PlayerControl.LocalPlayer.NetTransform.Halt();
        }*/

        var playerMenu = CustomPlayerMenu.Create();
        playerMenu.transform.FindChild("PhoneUI").GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.transform.FindChild("PhoneUI").GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        playerMenu.Begin(
            plr => (!plr.HasDied() ||
                    Object.FindObjectsOfType<DeadBody>().FirstOrDefault(x => x.ParentId == plr.PlayerId) ||
                    FakePlayer.FakePlayers.FirstOrDefault(x => x?.body?.name == $"Fake {plr.gameObject.name}")
                        ?.body) && plr != PlayerControl.LocalPlayer,
            plr =>
            {
                playerMenu.ForceClose();

                if (plr != null)
                {
                    MirrorcasterRole.RpcMagicMirror(PlayerControl.LocalPlayer, plr);

                    EffectActive = true;
                    Timer = EffectDuration;
                    OverrideName(TouLocale.Get("TouRoleMirrorcasterMagicMirrorProtecting", "Protecting"));
                    TargetWasValid = !plr.HasDied();
                }
                else
                {
                    EffectActive = false;
                    Timer = 0.01f;
                }
            });
        foreach (var panel in playerMenu.potentialVictims)
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
        var text = string.Empty;
        if (TargetWasValid)
        {
            DecreaseUses();
        }
        else
        {
            text = TouLocale.GetParsed("TouRoleMirrorcasterAlreadyDiedNotif");
        }

        if (Role.Protected != null && Role.Protected.HasDied())
        {
            text = TouLocale.GetParsed("TouRoleMirrorcasterTargetDiedNotif");
        }
        else if (Role.Protected != null && !Role.Protected.HasDied())
        {
            text = TouLocale.GetParsed("TouRoleMirrorcasterTargetDidNotDieNotif");
        }

        if (text.Contains("<player>") && Role.Protected != null)
        {
            text = text.Replace("<player>", Role.Protected.Data.PlayerName);
        }

        if (text != string.Empty && MeetingHud.Instance == null)
        {
            var notif1 = Helpers.CreateAndShowNotification(text,
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Mirrorcaster.LoadAsset());
            notif1.AdjustNotification();
        }

        TargetWasValid = false;
        MirrorcasterRole.RpcClearMagicMirror(PlayerControl.LocalPlayer);
        OverrideName(TouLocale.Get("TouRoleMirrorcasterMagicMirror", "Magic Mirror"));
    }
}