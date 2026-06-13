using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.Buttons.Crewmate;

public sealed class PlumberBlockButton : TownOfUsVentRoleButton<PlumberRole>, ILegacyCapable
{
    public override string Name => TouLocale.GetParsed("TouRolePlumberBlock", "Block");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Plumber;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<PlumberOptions>.Instance.BlockCooldown + MapCooldown, 5f, 120f);
    public override int MaxUses => (int)OptionGroupSingleton<PlumberOptions>.Instance.MaxBarricades;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? LegacyCrewAssets.BlockSprite : TouCrewAssets.BlockSprite;
    public int ExtraUses { get; set; }

    public override bool IsTargetValid(Vent? target)
    {
        return base.IsTargetValid(target) && !Role.FutureBlocks.Contains(target!.Id);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error($"{Name}: Target is null");
            return;
        }

        var notif1 = Helpers.CreateAndShowNotification(
            TouLocale.Get("TouRolePlumberBlockNotif"),
            Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Plumber.LoadAsset());
        notif1.AdjustNotification();

        PlumberRole.RpcPlumberBlockVent(PlayerControl.LocalPlayer, Target.Id);

        var flush = CustomButtonSingleton<PlumberFlushButton>.Instance;

        flush?.SetTimer(flush.Cooldown);
    }
}