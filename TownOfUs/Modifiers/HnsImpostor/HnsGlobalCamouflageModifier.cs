using MiraAPI.GameOptions;
using TownOfUs.Options.Roles.HnsImpostor;
using TownOfUs.Roles.HnsImpostor;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.HnsImpostor;

public sealed class HnsGlobalCamouflageModifier(PlayerControl camoSeeker) : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "Camouflaged";
    public override float Duration => OptionGroupSingleton<HnsCamouflagerOptions>.Instance.CamoDuration;
    public override bool AutoStart => true;
    public bool VisualPriority => true;
    public override bool VisibleToOthers => true;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = camoSeeker.GetDefaultAppearance();
        appearance.Speed = 1f;
        appearance.Size = new Vector3(0.7f, 0.7f, 1f);
        appearance.PlayerName = "???";
        appearance.PetId = string.Empty;
        appearance.NameVisible = GameManager.Instance.LogicOptions.GetShowCrewmateNames();
        return appearance;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);
        Player.MyPhysics.SetForcedBodyType(HnsCamouflagerRole.HiderBodyType);
    }

    public override void OnDeactivate()
    {
        Player?.ResetAppearance();
        if (Player?.IsImpostor() == true)
        {
            Player.MyPhysics.SetForcedBodyType(HnsCamouflagerRole.SeekerBodyType);
        }
        else
        {
            Player?.MyPhysics.SetForcedBodyType(HnsCamouflagerRole.HiderBodyType);
        }
    }
}