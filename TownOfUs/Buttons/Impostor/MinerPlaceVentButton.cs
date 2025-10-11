using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Roles.Impostor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TownOfUs.Buttons.Impostor;

public sealed class MinerPlaceVentButton : TownOfUsRoleButton<MinerRole>, IAftermathableButton
{
    public override string Name => TouLocale.Get("TouRoleMinerMine", "Mine");
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => OptionGroupSingleton<MinerOptions>.Instance.MineCooldown + MapCooldown;

    public override float EffectDuration =>
        OptionGroupSingleton<MinerOptions>.Instance.MineVisibility is MineVisiblityOptions.Immediate
            ? OptionGroupSingleton<MinerOptions>.Instance.MineDelay.Value + 0.001f
            : 0.001f;

    public override int MaxUses => (int)OptionGroupSingleton<MinerOptions>.Instance.MaxMines;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.MineSprite;

    public Vector2 VentSize { get; set; }
    public Vector3? SavedPos { get; set; }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);

        var vents = Object.FindObjectsOfType<Vent>();

        if (vents.Count > 0)
        {
            VentSize = Vector2.Scale(vents[0].GetComponent<BoxCollider2D>().size, vents[0].transform.localScale) *
                       0.75f;
        }
    }

    public override bool CanUse()
    {
        if (!base.CanUse())
        {
            return false;
        }

        var hits = Physics2D.OverlapBoxAll(PlayerControl.LocalPlayer.transform.position, VentSize, 0);

        hits = hits.Where(c =>
            (c.name.Contains("Vent") || c.name.Contains("Door") || !c.isTrigger) && c.gameObject.layer != 8 &&
            c.gameObject.layer != 5).ToArray();

        var noConflict = !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.Collider,
            PlayerControl.LocalPlayer.Collider.bounds.center, PlayerControl.LocalPlayer.transform.position,
            Constants.ShipAndAllObjectsMask,
            false);

        return hits.Count == 0 && noConflict && !ModCompatibility.GetPlayerElevator(PlayerControl.LocalPlayer).Item1;
    }

    public void AftermathHandler()
    {
        ClickHandler();
    }

    protected override void OnClick()
    {
        SavedPos = PlayerControl.LocalPlayer.transform.position;
    }

    public override void OnEffectEnd()
    {
        base.OnEffectEnd();

        var id = GetNextVentId();

        var immediate = OptionGroupSingleton<MinerOptions>.Instance.MineVisibility == MineVisiblityOptions.Immediate;

        MinerRole.RpcPlaceVent(PlayerControl.LocalPlayer, id, SavedPos!.Value, SavedPos.Value.z + 0.0004f, immediate);
        TouAudio.PlaySound(TouAudio.MineSound);
        SavedPos = null;
    }

    public static int GetNextVentId()
    {
        var id = 0;

        while (true)
        {
            if (ShipStatus.Instance.AllVents.All(v => v.Id != id))
            {
                return id;
            }

            id++;
        }
    }
}