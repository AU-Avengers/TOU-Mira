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
    private static string _normalRb => TouLocale.Get("TouRoleBarkeeperRoleblock");
    private static string _normalRbStart => TouLocale.GetParsed("TouRoleBarkeeperRoleblocking");
    private static Sprite _normalRbSprite => TouImpAssets.DrinkRoleblockSprite.LoadAsset();
    private static string _sickRb => TouLocale.Get("TouRoleBootleggerSicken");
    private static string _sickRbStart => TouLocale.Get("TouRoleBootleggerSickening");
    private static Sprite _sickRbSprite => TouImpAssets.DrinkSickenSprite.LoadAsset();
    private static string _poisRb => TouLocale.Get("TouRoleBootleggerPoison");
    private static string _poisRbStart => TouLocale.Get("TouRoleBootleggerPoisoning");
    private static Sprite _poisRbSprite => TouImpAssets.DrinkPoisonSprite.LoadAsset();

    private static void GetRb(PlayerControl? player, out Sprite sprite, out string text)
    {
        if (player == null || !player.TryGetModifier<BootleggerPoisonModifier>(out var mod))
        {
            text = _normalRb;
            sprite = _normalRbSprite;
        }
        else if (mod.Poison == PoisonProgress.Begun)
        {
            text = _sickRb;
            sprite = _sickRbSprite;
        }
        else
        {
            text = _poisRb;
            sprite = _poisRbSprite;
        }
    }
    private static void GetRbStart(PlayerControl player, out Sprite sprite, out string text)
    {
        if (!player.TryGetModifier<BootleggerPoisonModifier>(out var mod))
        {
            text = _normalRbStart;
            sprite = _normalRbSprite;
        }
        else if (mod.Poison == PoisonProgress.Begun)
        {
            text = _sickRbStart;
            sprite = _sickRbSprite;
        }
        else
        {
            text = _poisRbStart;
            sprite = _poisRbSprite;
        }
    }
    public override string Name => _normalRb;
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Impostor;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<BootleggerOptions>.Instance.RoleblockCooldown.Value + MapCooldown, 5f, 120f);
    public override float EffectDuration => SelectedDuration;

    public float SelectedDuration = 0.001f;
    public override LoadableAsset<Sprite> Sprite => TouImpAssets.DrinkRoleblockSprite;
    private PlayerControl? _roleblockedTarget;

    public override PlayerControl? GetTarget()
    {
        var target = PlayerControl.LocalPlayer.GetClosestLivingPlayer(false, Distance);
        if (!EffectActive)
        {
            GetRb(target, out var sprite, out var text);
            OverrideName(text);
            OverrideSprite(sprite);
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

        GetRbStart(Target, out var sprite, out var text);
        OverrideName(text);
        OverrideSprite(sprite);

        _roleblockedTarget = Target;

        NotifMessage = Helpers.CreateAndShowNotification($"<b>{TouLocale.GetParsed("TouRoleBootleggerRoleblockChosen") .Replace("<player>", _roleblockedTarget.CachedPlayerData.PlayerName)}</b>",
        Color.white, new Vector3(0f, 1f, -20f), spr: TouRoleIcons.Bootlegger.LoadAsset());
        NotifMessage.Text.SetOutlineThickness(0.35f);
    }

    public override void OnEffectEnd()
    {
        OverrideName(_normalRb);

        if (_roleblockedTarget == null) return;

        BarkeeperRole.RpcRoleblock(PlayerControl.LocalPlayer, _roleblockedTarget);
        _roleblockedTarget = null;
    }

}
