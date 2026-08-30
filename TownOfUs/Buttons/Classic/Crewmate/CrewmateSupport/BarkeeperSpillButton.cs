using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class BarkeeperSpillButton : TownOfUsRoleButton<BarkeeperRole>
{
    public override string Name => MiraLocaleManager.Get("TownOfUsMira.Role.BarkeeperSpill");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Barkeeper;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<BarkeeperOptions>.Instance.RoleblockCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration => OptionGroupSingleton<BarkeeperOptions>.Instance.SpillDelay.Value;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.SpillSprite;

    public LobbyNotificationMessage? NotifMessage;
    protected override void OnClick()
    {
        var message = MiraLocaleManager.Get("TownOfUsMira.Role.BarkeeperSpillNotification") .Replace("<time>", EffectDuration.ToString(TownOfUsPlugin.Culture));
        NotifMessage = Helpers.CreateAndShowNotification($"<b>{message}</b>", Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Barkeeper.LoadAsset());
        NotifMessage.Text.SetOutlineThickness(0.35f);
        CustomButtonSingleton<BarkeeperRoleblockButton>.Instance.ResetCooldownAndOrEffect();
        var pos = PlayerControl.LocalPlayer.transform.position;
        BarkeeperRole.RpcSpillDrink(PlayerControl.LocalPlayer, new Vector2(pos.x, pos.y));
    }

    public override void OnEffectEnd()
    {
        OverrideName(MiraLocaleManager.Get("TownOfUsMira.Role.BarkeeperSpill"));
    }

}
