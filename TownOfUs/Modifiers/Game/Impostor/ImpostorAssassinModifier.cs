using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options;
using UnityEngine;

namespace TownOfUs.Modifiers.Game.Impostor;

public sealed class ImpostorAssassinModifier : AssassinModifier, IWikiDiscoverable
{
    public override bool ShowInFreeplay => true;

    [HideFromIl2Cpp] public bool IsHiddenFromList => true;

    // YES this is scuffed, a better solution will be used at a later time
    public uint FakeTypeId =>
        ModifierManager.GetModifierTypeId(ModifierManager.Modifiers.FirstOrDefault(x =>
            x is TouGameModifier touGameMod && touGameMod.LocaleKey == "Assassin")!.GetType()) ??
        throw new InvalidOperationException("Modifier is not registered.");
    public override Color FreeplayFileColor => new Color32(255, 25, 25, 255);

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.NumberOfImpostorAssassins.Value;
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<AssassinOptions>.Instance.ImpAssassinChance.Value;
    }

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return role.TeamType == RoleTeamTypes.Impostor;
    }
}