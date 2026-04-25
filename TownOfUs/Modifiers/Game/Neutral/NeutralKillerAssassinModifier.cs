using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options;
using TownOfUs.Roles;

namespace TownOfUs.Modifiers.Game.Neutral;

public sealed class NeutralKillerAssassinModifier : AssassinModifier, IWikiDiscoverable
{
    public override bool ShowInFreeplay => true;

    [HideFromIl2Cpp] public bool IsHiddenFromList => true;

    // YES this is scuffed, a better solution will be used at a later time
    public uint FakeTypeId =>
        ModifierManager.GetModifierTypeId(ModifierManager.Modifiers.FirstOrDefault(x =>
            x is TouGameModifier touGameMod && touGameMod.LocaleKey == "Assassin")!.GetType()) ??
        throw new InvalidOperationException("Modifier is not registered.");

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfNeutralAssassins.Value;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.NeutAssassinChance.Value;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role is ITownOfUsRole { RoleAlignment: RoleAlignment.NeutralKilling };
    }
}