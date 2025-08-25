using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options.Modifiers;
using TownOfUs.Utilities;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class ImpostorDoubleShotModifier : DoubleShotModifier, IWikiDiscoverable
{
    public override string ModifierName => TouLocale.Get(TouNames.DoubleShot, "Double Shot");
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);
    
    public override bool ShowInFreeplay => true;
    public bool IsHiddenFromList => true;
    // YES this is scuffed, a better solution will be used at a later time
    public uint FakeTypeId => ModifierManager.GetModifierTypeId(ModifierManager.Modifiers.FirstOrDefault(x => x.ModifierName == "Double Shot")!.GetType()) ?? throw new InvalidOperationException("Modifier is not registered.");
    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DoubleShotChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.DoubleShotAmount;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        if (
            role.Player.IsImpostor()
            && role.Player.GetModifierComponent().HasModifier<ImpostorAssassinModifier>(true)
            && !role.Player.GetModifierComponent().HasModifier<TouGameModifier>(true)
        )
        {
            return true;
        }

        return false;
    }
}