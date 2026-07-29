using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Impostor;
using UnityEngine;

namespace TownOfUs.Buttons.Impostor;

public sealed class BootleggerRoleblockButton : TownOfUsRoleButton<BootleggerRole, PlayerControl>
{
    private string _normalRb => TouLocale.Get("TouRoleBarkeeperRoleblock");
    private string _sickRb => TouLocale.Get("TouRoleBootleggerSicken");
    private string _poisRb => TouLocale.Get("TouRoleBootleggerPoison");
    private string _normalRbStart => TouLocale.GetParsed("TouRoleBarkeeperRoleblocking");
    private string _sickRbStart => TouLocale.Get("TouRoleBootleggerSickening");
    private string _poisRbStart => TouLocale.Get("TouRoleBootleggerPoisoning");

    private string GetRbTitle(PlayerControl? player)
    {
        if (player == null || !player.TryGetModifier<BootleggerPoisonModifier>(out var mod)) return _normalRb;
        if (mod.Poison == PoisonProgress.Begun)
        {
                return _sickRb;
        }
        return _poisRb;
    }
    private string GetRbStartTitle(PlayerControl player)
    {
        if (!player.TryGetModifier<BootleggerPoisonModifier>(out var mod)) return _normalRbStart;
        if (mod.Poison == PoisonProgress.Begun)
        {
            return _sickRbStart;
        }
        return _poisRbStart;
    }
    public override string Name => _normalRb;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration => SelectedDuration;

    public float SelectedDuration = 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.SampleSprite;
    private PlayerControl? _roleblockedTarget;

    public override PlayerControl? GetTarget()
    {
        var target = PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance);
        if (!EffectActive)
        {
            OverrideName(GetRbTitle(target));
        }

        return target;
    }

    public override void ClickHandler()
    {
        if (CanClick())
        {
            var opts = OptionGroupSingleton<BootleggerOptions>.Instance;
            SelectedDuration = UnityEngine.Random.RandomRange(opts.RoleblockDelayMin.Value, opts.RoleblockDelayMax.Value);
        }
        base.ClickHandler();
    }

    public LobbyNotificationMessage? NotifMessage;
    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        OverrideName(GetRbStartTitle(Target));

        _roleblockedTarget = Target;

        if (PlayerControl.LocalPlayer.AmOwner)
        {
            NotifMessage = Helpers.CreateAndShowNotification(
                $"<b>You chose to roleblock {_roleblockedTarget.CachedPlayerData.PlayerName}.</b>",
                Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Bootlegger.LoadAsset());
            NotifMessage.Text.SetOutlineThickness(0.35f);
        }
    }

    public override void OnEffectEnd()
    {
        OverrideName(_normalRb);

        if (_roleblockedTarget == null) return;

        BarkeeperRole.RpcRoleblock(PlayerControl.LocalPlayer, _roleblockedTarget);
        _roleblockedTarget = null;
    }

}
