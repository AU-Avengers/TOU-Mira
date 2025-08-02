using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers;

public sealed class FirstDeadShield : ExcludedGameModifier, IAnimated
{
    public override string ModifierName => TouLocale.Get(TouNames.FirstDeathShield, "First Death Shield");
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.FirstRoundShield;

    public override bool HideOnUi => !TownOfUsPlugin.ShowShieldHud.Value;
    public override Color FreeplayFileColor => new Color32(100, 220, 100, 255);

    public GameObject? FirstRoundShield { get; set; }
    public bool IsVisible { get; set; } = true;

    public void SetVisible()
    {
    }

    public override int GetAmountPerGame()
    {
        if (FirstDeadPatch.PlayerNames.Count == 0)
        {
            return 0;
        }

        var validPlayer = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => FirstDeadPatch.PlayerNames.Contains(x.name)).AsEnumerable()
            .OrderBy(obj => FirstDeadPatch.PlayerNames.IndexOf(obj.name)).FirstOrDefault();

        return validPlayer != null && OptionGroupSingleton<GeneralOptions>.Instance.FirstDeathShield
            ? 1
            : 0;
    }

    public override int GetAssignmentChance()
    {
        if (FirstDeadPatch.PlayerNames.Count == 0)
        {
            return 0;
        }

        var validPlayer = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => FirstDeadPatch.PlayerNames.Contains(x.name)).AsEnumerable()
            .OrderBy(obj => FirstDeadPatch.PlayerNames.IndexOf(obj.name)).FirstOrDefault();

        return validPlayer != null && OptionGroupSingleton<GeneralOptions>.Instance.FirstDeathShield
            ? 100
            : 0;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        if (FirstDeadPatch.PlayerNames.Count == 0)
        {
            return false;
        }

        var validPlayer = PlayerControl.AllPlayerControls.ToArray()
            .Where(x => FirstDeadPatch.PlayerNames.Contains(x.name)).AsEnumerable()
            .OrderBy(obj => FirstDeadPatch.PlayerNames.IndexOf(obj.name)).FirstOrDefault();

        return role.Player == validPlayer;
    }

    public override string GetDescription()
    {
        return !HideOnUi ? "You have protection because you died first last game" : string.Empty;
    }

    public override void OnActivate()
    {
        FirstRoundShield =
            AnimStore.SpawnAnimBody(Player, TouAssets.FirstRoundShield.LoadAsset(), false, -1.1f, -0.225f, 1.5f)!;
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        FirstRoundShield?.SetActive(false);
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (FirstRoundShield?.gameObject != null)
        {
            FirstRoundShield.Destroy();
        }
    }

    public override void Update()
    {
        if (!MeetingHud.Instance && FirstRoundShield?.gameObject != null)
        {
            FirstRoundShield?.SetActive(!Player.IsConcealed() && IsVisible);
        }
        else if (MeetingHud.Instance)
        {
            FirstRoundShield?.SetActive(false);
            ModifierComponent!.RemoveModifier(this);
        }
    }
}