using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TownOfUs.Options.Roles.Neutral;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace TownOfUs.Buttons.Classic.Neutral.NeutralEvil;

public sealed class JesterPokeButton : TownOfUsRoleButton<JesterRole, PlayerControl>
{
    public override string Name => TouLocale.GetParsed("TouRoleJesterPoke", "Poke");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Jester;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<JesterOptions>.Instance.PokeCooldown.Value + MapCooldown, 5f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouNeutAssets.JesterPokeSprite;
    public override ButtonLocation Location => ButtonLocation.BottomRight;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(this));
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && OptionGroupSingleton<JesterOptions>.Instance.CanPoke.Value;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        var notif1 = Helpers.CreateAndShowNotification(
            TouLocale.GetParsed("TouNotifJesterPoke").Replace("<player>", $"{TownOfUsColors.Jester.ToTextColor()}{Target.Data.PlayerName}</color>"),
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Jester.LoadAsset());

        notif1.AdjustNotification();
    }
}
