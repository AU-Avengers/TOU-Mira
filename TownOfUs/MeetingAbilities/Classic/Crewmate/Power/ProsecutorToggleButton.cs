using MiraAPI.GameOptions;
using MiraAPI.MeetingAbilities;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles.Crewmate;
using UnityEngine;

namespace TownOfUs.MeetingAbilities.Classic.Crewmate.Power;

public class ProsecutorToggleButton : MeetingActionButton
{
    public override string Name => MiraLocaleManager.Get("Disabled");

    public override float Cooldown => 0.0001f;

    public override float InitialCooldown => 0.0001f;

    public override int MaxUses => 0;

    public override LoadableAsset<Sprite> Sprite =>
        TouAssets.ProsecutorToggleSprite;

    public override bool HideUponWrapUp => true;
    public override bool DisableUponVoting => true;
    public override Color TextOutlineColor => TownOfUsColors.Prosecutor;
    private SpriteRenderer _toggleSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        _toggleSprite = Button!.usesRemainingSprite;
        _toggleSprite.gameObject.SetActive(true);
        _toggleSprite.color = Color.white;
        _toggleSprite.sprite = TouAssets.ToggleDisabledSprite.LoadAsset();
        _toggleSprite.transform.localPosition = new Vector3(0, 0, -0.001f);
        _toggleSprite.transform.localScale = new Vector3(1.1f, 1.1f, 1);
    }


    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ProsecutorRole pros && !pros.HideProsButton && pros.WantsToPros is not ProsecuteToggleMode.NoToggle && !pros.HasProsecuted && pros.ProsecutionsCompleted <
            OptionGroupSingleton<ProsecutorOptions>.Instance.MaxProsecutions;
    }

    public override bool CanUse()
    {
        return base.CanUse() &&
               MeetingHud.Instance.CurrentState is MeetingHud.MeetingStates.NotVoted;
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.Data.Role is ProsecutorRole pros)
        {
            pros.WantsToPros = pros.WantsToPros is ProsecuteToggleMode.ToggledOn ? ProsecuteToggleMode.ToggledOff : ProsecuteToggleMode.ToggledOn;
            OverrideName(pros.WantsToPros is ProsecuteToggleMode.ToggledOn ? "Enabled" : "Disabled");
            _toggleSprite.color = Color.white;
            _toggleSprite.sprite = pros.WantsToPros is ProsecuteToggleMode.ToggledOn ? TouAssets.ToggleEnabledSprite.LoadAsset() : TouAssets.ToggleDisabledSprite.LoadAsset();
        }
    }
}
