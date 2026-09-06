using Reactor.Localization;

namespace TownOfUs.Modules.Localization;

public sealed class TaskProvider : LocalizationProvider
{
    public const SystemTypes DeathValleySystemType = (SystemTypes)250;
    private static readonly StringNames DeathValley = MiraLocaleManager.GetOrCreateLocaleString("Death Valley");

    public override bool TryGetStringName(SystemTypes systemType, out StringNames? result)
    {
        if (systemType == DeathValleySystemType)
        {
            result = DeathValley;
            return true;
        }

        return base.TryGetStringName(systemType, out result);
    }
}