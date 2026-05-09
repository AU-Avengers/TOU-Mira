using MiraAPI.Events;
using MiraAPI.GameOptions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace TownOfUs.Modifiers.Impostor.Venerer;

public sealed class VenererCamouflageModifier : ConcealedModifier, IVenererModifier, IVisualAppearance
{
    public override string ModifierName => "Camouflaged";
    public override float Duration => OptionGroupSingleton<VenererOptions>.Instance.AbilityDuration;
    public override bool AutoStart => true;
    public bool VisualPriority => true;
    public override bool HideOnUi => true;
    public override bool VisibleToOthers => true;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = 1f;
        appearance.Size = new Vector3(0.7f, 0.7f, 1f);
        appearance.ColorId = Player.Data.DefaultOutfit.ColorId;
        appearance.HatId = "hat_NoHat";
        appearance.SkinId = "skin_None";
        appearance.VisorId = "visor_EmptyVisor";
        appearance.PlayerName = string.Empty;
        appearance.PetId = "pet_EmptyPet";
        appearance.NameVisible = false;
        appearance.PlayerMaterialColor = Color.grey;
        return appearance;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);

        var touAbilityEvent = new TouAbilityEvent(AbilityType.VenererCamoAbility, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeactivate()
    {
        Player?.ResetAppearance();
    }
}